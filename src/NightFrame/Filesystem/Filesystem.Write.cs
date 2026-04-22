// <copyright file="Filesystem.Write.cs" company="Night Circle">
// zlib license
//
// Copyright (c) 2025 Danny Solivan, Night Circle
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
// </copyright>

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using Night.Log;

namespace Night
{
  /// <summary>
  /// Provides an interface to the user's filesystem.
  /// </summary>
  public static partial class Filesystem
  {
    // Logger is already defined in another part of this partial class.

    /// <summary>
    /// Writes data to a file. If the file already exists, it will be completely replaced.
    /// </summary>
    /// <param name="name">The name (and path) of the file.</param>
    /// <param name="data">The string data to write to the file. The string will be UTF-8 encoded.</param>
    /// <param name="size">How many bytes of the encoded string to write.
    /// If null, the entire encoded string is written.
    /// If the requested size exceeds practical limits (e.g., max underlying stream write size),
    /// writing may be capped, and the operation will proceed with the capped amount.</param>
    /// <returns>
    /// A tuple containing:
    /// - <c>Success</c>: True if the operation was successful, false otherwise.
    /// - <c>ErrorMessage</c>: An error message if the operation was unsuccessful, otherwise null.
    /// </returns>
    public static (bool Success, string? ErrorMessage) Write(string name, string data, long? size = null)
    {
      if (data == null)
      {
        return (false, "Data to write cannot be null.");
      }

      byte[] encodedData = Encoding.UTF8.GetBytes(data);
      return Write(name, encodedData, size);
    }

    /// <summary>
    /// Writes data to a file. If the file already exists, it will be completely replaced.
    /// </summary>
    /// <param name="name">The name (and path) of the file.</param>
    /// <param name="data">The byte array data to write to the file.</param>
    /// <param name="size">How many bytes from the data array to write.
    /// If null, the entire data array is written.
    /// If the requested size exceeds practical limits (e.g., max underlying stream write size),
    /// writing may be capped, and the operation will proceed with the capped amount.</param>
    /// <returns>
    /// A tuple containing:
    /// - <c>Success</c>: True if the operation was successful, false otherwise.
    /// - <c>ErrorMessage</c>: An error message if the operation was unsuccessful, otherwise null.
    /// </returns>
    public static (bool Success, string? ErrorMessage) Write(string name, byte[] data, long? size = null)
    {
      if (string.IsNullOrEmpty(name))
      {
        return (false, "File name cannot be null or empty.");
      }

      if (data == null)
      {
        return (false, "Data to write cannot be null.");
      }

      if (size.HasValue && size.Value < 0)
      {
        return (false, "Size to write cannot be negative.");
      }

      if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        // Check if the path starts with a pattern like "X:\" or "X:/" which indicates a Windows drive letter.
        // This is not a standard absolute path format on Unix.
        if (name.Length >= 2 && char.IsLetter(name[0]) && name[1] == ':')
        {
          // Further check if it's followed by a directory separator, making it "X:\..." or "X:/..."
          if (name.Length >= 3 && (name[2] == Path.DirectorySeparatorChar || name[2] == Path.AltDirectorySeparatorChar))
          {
            Logger?.Info($"Path '{name}' on Unix-like system resembles a Windows drive path. Simulating unmapped drive behavior.");
            throw new DirectoryNotFoundException($"Could not find a part of the path '{name}'. (Simulated unmapped drive on Unix for Windows-style path)");
          }
        }
      }

      try
      {
        // Determine actual number of bytes to write
        long bytesToWrite = data.Length;
        if (size.HasValue)
        {
          bytesToWrite = Math.Min(size.Value, data.Length);
        }

        // Ensure parent directory exists
        string? directoryPath = Path.GetDirectoryName(name);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
          _ = Directory.CreateDirectory(directoryPath);
        }

        using (var stream = new FileStream(name, global::System.IO.FileMode.Create, global::System.IO.FileAccess.Write, global::System.IO.FileShare.None))
        {
          if (bytesToWrite == 0)
          {
            // Ensure the file is created (and truncated if it existed) even if writing 0 bytes.
            // FileStream with FileMode.Create already handles this.
            return (true, null);
          }

          // Cap writing at int.MaxValue due to .NET Stream.Write limitations with byte[]
          // FileStream itself might handle larger writes internally if underlying OS supports it,
          // but the byte[] overload of Write takes an int count.
          // For very large data, consider writing in chunks if this cap is an issue.
          int actualBytesToWriteInChunk = (int)Math.Min(bytesToWrite, int.MaxValue);

          if (bytesToWrite > int.MaxValue)
          {
            Logger.Warn($"Requested write size ({bytesToWrite} bytes) for '{name}' exceeds int.MaxValue. Writing in chunks capped at {int.MaxValue} bytes per chunk.");

            long totalBytesWritten = 0;
            while (totalBytesWritten < bytesToWrite)
            {
              int bytesInCurrentChunk = (int)Math.Min(bytesToWrite - totalBytesWritten, int.MaxValue);
              stream.Write(data, (int)totalBytesWritten, bytesInCurrentChunk);
              totalBytesWritten += bytesInCurrentChunk;
            }
          }
          else
          {
            stream.Write(data, 0, actualBytesToWriteInChunk);
          }
        }

        return (true, null);
      }
      catch (ArgumentException ex)
      {
        Logger.Error($"Argument error while trying to write to file '{name}'.", ex);
        return (false, $"Argument error: {ex.Message}");
      }
      catch (PathTooLongException ex)
      {
        Logger.Error($"Path too long for file '{name}'.", ex);
        return (false, "The specified path, file name, or both exceed the system-defined maximum length.");
      }
      catch (DirectoryNotFoundException ex)
      {
        Logger.Error($"Directory not found for file '{name}'.", ex);
        return (false, "The specified path is invalid (for example, it is on an unmapped drive).");
      }
      catch (IOException ex)
      {
        // HResult for ERROR_FILENAME_EXCED_RANGE (path too long / invalid filename component) is 0x800700CE.
        // This can be thrown by Directory.CreateDirectory if a segment of the path is too long,
        // before FileStream itself might throw a PathTooLongException.
        // We want to provide a consistent error message for path length issues.
        const int ERROR_FILENAME_EXCED_RANGE = unchecked((int)0x800700CE);

        if (ex.HResult == ERROR_FILENAME_EXCED_RANGE)
        {
          Logger.Error($"Path too long (caught as IOException with HResult 0x{ex.HResult:X8}) for file '{name}'. Original Message: {ex.Message}", ex);

          // Return the same message as for PathTooLongException for consistency
          return (false, "The specified path, file name, or both exceed the system-defined maximum length.");
        }

        Logger.Error($"IO error trying to write to file '{name}'.", ex);
        return (false, $"IO error: {ex.Message}");
      }
      catch (UnauthorizedAccessException ex)
      {
        Logger.Error($"Unauthorized access trying to write to file '{name}'.", ex);
        return (false, "Unauthorized access. Check file permissions or if the path is a directory.");
      }
      catch (SecurityException ex)
      {
        Logger.Error($"Security error trying to write to file '{name}'.", ex);
        return (false, "A security error occurred.");
      }
      catch (NotSupportedException ex)
      {
        Logger.Error($"Operation not supported for file '{name}'.", ex);
        return (false, $"Operation not supported: {ex.Message}");
      }
      catch (Exception ex)
      {
        Logger.Error($"Unexpected error trying to write to file '{name}'.", ex);
        return (false, $"An unexpected error occurred: {ex.Message}");
      }
    }
  }
}
