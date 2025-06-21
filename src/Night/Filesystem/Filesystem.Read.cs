// <copyright file="Filesystem.Read.cs" company="Night Circle">
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
using System.Collections.Generic;
using System.IO;
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
    /// <summary>
    /// Returns an iterator function that iterates over all the lines in a file.
    /// </summary>
    /// <param name="filePath">The name (and path) of the file.</param>
    /// <returns>An enumerable collection of strings, where each string is a line in the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if filePath is null.</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the file specified in filePath was not found.</exception>
    /// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the caller does not have the required permission, or path specified a directory, or the caller does not have read access.</exception>
    public static IEnumerable<string> Lines(string filePath)
    {
      if (filePath == null)
      {
        throw new ArgumentNullException(nameof(filePath));
      }

      if (string.IsNullOrEmpty(filePath))
      {
        throw new ArgumentException("File path cannot be empty.", nameof(filePath));
      }

      return File.ReadLines(filePath);
    }

    /// <summary>
    /// Reads the contents of a file into a string.
    /// </summary>
    /// <param name="name">The name (and path) of the file.</param>
    /// <param name="sizeToRead">How many bytes to read. If null, reads the entire file.
    /// If the requested size exceeds practical limits (e.g., max string size),
    /// reading may be capped, and the returned bytesRead will reflect the actual amount.</param>
    /// <returns>
    /// A tuple containing:
    /// - <c>contents</c>: The file contents as a string. Null if an error occurs.
    /// - <c>bytesRead</c>: How many bytes were read. Null if an error occurs before reading attempt or on critical failure.
    /// - <c>errorMsg</c>: An error message if reading fails, otherwise null.
    /// </returns>
    /// <remarks>
    /// This method mimics LÖVE's love.filesystem.read(name, size), defaulting to string content.
    /// Content is UTF-8 decoded.
    /// </remarks>
    public static (string? Contents, long? BytesRead, string? ErrorMsg) Read(string name, long? sizeToRead = null)
    {
      var result = Read(ContainerType.String, name, sizeToRead);
      if (result.ErrorMsg != null)
      {
        // Ensure contents is null if there's an error message, bytesRead might be 0 or null depending on when error occurred.
        return (null, result.BytesRead, result.ErrorMsg);
      }

      return ((string?)result.Contents, result.BytesRead, result.ErrorMsg);
    }

    /// <summary>
    /// Reads the contents of a file.
    /// </summary>
    /// <param name="container">What type to return the file's contents as (string or raw data).</param>
    /// <param name="name">The name (and path) of the file.</param>
    /// <param name="sizeToRead">How many bytes to read. If null, reads the entire file.
    /// If the requested size exceeds practical limits (e.g., max array/string size),
    /// reading may be capped, and the returned bytesRead will reflect the actual amount.</param>
    /// <returns>
    /// A tuple containing:
    /// - <c>contents</c>: The file contents as an object (string or byte[]). Null if an error occurs.
    /// - <c>bytesRead</c>: How many bytes were read. Null if an error occurs before reading attempt or on critical failure.
    /// - <c>errorMsg</c>: An error message if reading fails, otherwise null.
    /// </returns>
    /// <remarks>
    /// This method mimics LÖVE's love.filesystem.read(container, name, size).
    /// When <paramref name="container"/> is <see cref="ContainerType.Data"/>, contents will be byte[].
    /// When <paramref name="container"/> is <see cref="ContainerType.String"/>, contents will be a string (UTF-8 decoded).
    /// Reading is capped at int.MaxValue bytes due to .NET array/string limitations.
    /// </remarks>
    public static (object? Contents, long? BytesRead, string? ErrorMsg) Read(ContainerType container, string name, long? sizeToRead = null)
    {
      if (string.IsNullOrEmpty(name))
      {
        return (null, null, "File name cannot be null or empty.");
      }

      if (sizeToRead.HasValue && sizeToRead.Value < 0)
      {
        // LÖVE's behavior for negative size is not explicitly defined for read,
        // but typically means read all or error. Let's treat as an error or invalid argument.
        // For consistency with Append, we could return 0 bytes read, but an error seems more appropriate for read.
        return (null, 0, "Size to read cannot be negative.");
      }

      try
      {
        if (!File.Exists(name))
        {
          return (null, null, "File not found.");
        }

        using (var stream = new FileStream(name, global::System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          long fileLength = stream.Length;
          long actualBytesToRead;

          if (sizeToRead.HasValue)
          {
            actualBytesToRead = Math.Min(sizeToRead.Value, fileLength);
          }
          else
          {
            actualBytesToRead = fileLength;
          }

          // Cap reading at int.MaxValue due to .NET array/string limitations
          if (actualBytesToRead > int.MaxValue)
          {
            Logger.Warn($"Requested read size ({actualBytesToRead} bytes) for '{name}' exceeds int.MaxValue. Capping read at {int.MaxValue} bytes.");
            actualBytesToRead = int.MaxValue;
          }

          if (actualBytesToRead == 0)
          {
            return (container == ContainerType.String ? string.Empty : Array.Empty<byte>(), 0, null);
          }

          byte[] buffer = new byte[(int)actualBytesToRead];
          int bytesActuallyReadFromStream = stream.Read(buffer, 0, (int)actualBytesToRead);

          if (bytesActuallyReadFromStream < actualBytesToRead)
          {
            // This might happen if the file is modified concurrently, or other rare FS issues.
            // Adjust buffer if fewer bytes were read than expected.
            Array.Resize(ref buffer, bytesActuallyReadFromStream);
            Logger.Warn($"Read fewer bytes ({bytesActuallyReadFromStream}) than expected ({actualBytesToRead}) for file '{name}'.");
          }

          object resultContents;
          if (container == ContainerType.String)
          {
            resultContents = global::System.Text.Encoding.UTF8.GetString(buffer);
          }
          else
          {
            resultContents = buffer;
          }

          return (resultContents, bytesActuallyReadFromStream, null);
        }
      }
      catch (FileNotFoundException)
      {
        return (null, null, "File not found.");
      }
      catch (UnauthorizedAccessException ex)
      {
        Logger.Error($"Unauthorized access trying to read file '{name}'.", ex);
        return (null, null, "Unauthorized access.");
      }
      catch (SecurityException ex)
      {
        Logger.Error($"Security error trying to read file '{name}'.", ex);
        return (null, null, "Security error.");
      }
      catch (IOException ex)
      {
        Logger.Error($"IO error trying to read file '{name}'.", ex);
        return (null, null, $"IO error: {ex.Message}");
      }
      catch (Exception ex)
      {
        Logger.Error($"Unexpected error trying to read file '{name}'.", ex);
        return (null, null, $"An unexpected error occurred: {ex.Message}");
      }
    }
  }
}
