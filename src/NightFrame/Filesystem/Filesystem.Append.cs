// <copyright file="Filesystem.Append.cs" company="Night Circle">
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
using System.Security;
using System.Text;

using IOFileMode = System.IO.FileMode;

namespace Night
{
  /// <summary>
  /// Provides an interface to the user's filesystem.
  /// </summary>
  public static partial class Filesystem
  {
    /// <summary>
    /// Appends string data to a file in the save directory. If the file does not exist, it will be created.
    /// </summary>
    /// <param name="filepath">The relative path of the file within the save directory.</param>
    /// <param name="data">The string data to append. The string will be UTF-8 encoded.</param>
    /// <param name="size">The number of bytes of the encoded string to append. If null, the entire string is appended.</param>
    /// <returns>A tuple indicating success and providing an error message on failure.</returns>
    public static (bool Success, string? ErrorMessage) Append(string filepath, string data, long? size = null)
    {
      if (data == null)
      {
        return (false, "Data to append cannot be null.");
      }

      byte[] encodedData = Encoding.UTF8.GetBytes(data);
      return Append(filepath, encodedData, size);
    }

    /// <summary>
    /// Appends byte data to a file in the save directory. If the file does not exist, it will be created.
    /// </summary>
    /// <param name="filepath">The relative path of the file within the save directory.</param>
    /// <param name="data">The byte array data to append.</param>
    /// <param name="size">The number of bytes to append. If null, the entire array is appended.</param>
    /// <returns>A tuple indicating success and providing an error message on failure.</returns>
    public static (bool Success, string? ErrorMessage) Append(string filepath, byte[] data, long? size = null)
    {
      if (string.IsNullOrEmpty(filepath))
      {
        return (false, "Filepath cannot be null or empty.");
      }

      if (data == null)
      {
        return (false, "Data to append cannot be null.");
      }

      try
      {
        string fullPath = GetFullPathInSaveDirectory(filepath);

        // Ensure parent directory exists
        string? directoryPath = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
          _ = Directory.CreateDirectory(directoryPath);
        }

        long bytesToWrite = data.Length;
        if (size.HasValue)
        {
          if (size.Value < 0)
          {
            return (true, null); // LÖVE does nothing for negative size, so we succeed with no action.
          }

          bytesToWrite = Math.Min(size.Value, data.Length);
        }

        if (bytesToWrite == 0)
        {
          // Ensure file exists even if writing 0 bytes, consistent with append mode creating a file.
          if (!File.Exists(fullPath))
          {
            File.Create(fullPath).Dispose();
          }

          return (true, null);
        }

        using (var stream = new FileStream(fullPath, IOFileMode.Append, FileAccess.Write, FileShare.None))
        {
          int bytesToWriteInChunk = (int)Math.Min(bytesToWrite, int.MaxValue);
          if (bytesToWrite > int.MaxValue)
          {
            Logger.Warn($"Requested append size ({bytesToWrite} bytes) for '{filepath}' exceeds int.MaxValue. Appending in chunks.");
            long totalBytesWritten = 0;
            while (totalBytesWritten < bytesToWrite)
            {
              int currentChunkSize = (int)Math.Min(bytesToWrite - totalBytesWritten, int.MaxValue);
              stream.Write(data, (int)totalBytesWritten, currentChunkSize);
              totalBytesWritten += currentChunkSize;
            }
          }
          else
          {
            stream.Write(data, 0, bytesToWriteInChunk);
          }
        }

        return (true, null);
      }
      catch (Exception ex) when (
        ex is ArgumentException ||
        ex is PathTooLongException ||
        ex is DirectoryNotFoundException ||
        ex is IOException ||
        ex is UnauthorizedAccessException ||
        ex is SecurityException ||
        ex is NotSupportedException)
      {
        Logger.Error($"Failed to append to file '{filepath}'. Reason: {ex.Message}", ex);
        return (false, ex.Message);
      }
      catch (Exception ex)
      {
        Logger.Error($"An unexpected error occurred while appending to '{filepath}'.", ex);
        return (false, $"An unexpected error occurred: {ex.Message}");
      }
    }
  }
}
