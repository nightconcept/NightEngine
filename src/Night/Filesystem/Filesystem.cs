// <copyright file="Filesystem.cs" company="Night Circle">
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

using Night.Log;

namespace Night
{
  /// <summary>
  /// Provides basic file system operations.
  /// Inspired by Love2D's love.filesystem module.
  /// </summary>
  public static class Filesystem
  {
    private static readonly ILogger Logger = LogManager.GetLogger("Night.Filesystem.Filesystem");
    private static string gameIdentity = "NightDefault"; // Placeholder, to be managed by SetIdentity/GetIdentity

    /// <summary>
    /// Gets information about the specified file or directory.
    /// </summary>
    /// <param name="path">The file or directory path to check.</param>
    /// <param name="filterType">If supplied, this parameter causes getInfo to only return the info table if the item at the given path matches the specified file type.</param>
    /// <returns>A FileSystemInfo object containing information about the specified path, or null if nothing exists at the path or if it doesn't match the filterType.</returns>
    public static FileSystemInfo? GetInfo(string path, FileType? filterType = null)
    {
      if (string.IsNullOrEmpty(path))
      {
        return null;
      }

      long? size = null;
      FileType type;
      long? modTime;
      try
      {
        if (File.Exists(path))
        {
          var fileInfo = new FileInfo(path);
          if ((fileInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
          {
            type = FileType.Symlink;
          }
          else
          {
            type = FileType.File;
          }

          size = fileInfo.Length;
          modTime = ((DateTimeOffset)fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds();
        }
        else if (Directory.Exists(path))
        {
          var dirInfo = new DirectoryInfo(path);
          if ((dirInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
          {
            type = FileType.Symlink;
          }
          else
          {
            type = FileType.Directory;
          }

          modTime = ((DateTimeOffset)dirInfo.LastWriteTimeUtc).ToUnixTimeSeconds();
        }
        else
        {
          return null;
        }
      }
      catch (Exception ex)
      {
        Logger.Error($"Error getting file info for path '{path}'.", ex);
        return null;
      }

      if (filterType.HasValue && type != filterType.Value)
      {
        return null;
      }

      return new FileSystemInfo(type, size, modTime);
    }

    /// <summary>
    /// Gets information about the specified file or directory and populates an existing FileSystemInfo object.
    /// </summary>
    /// <param name="path">The file or directory path to check.</param>
    /// <param name="info">A FileSystemInfo object which will be filled in.</param>
    /// <returns>The FileSystemInfo object given as an argument, filled with information, or null if nothing exists at the path.</returns>
    public static FileSystemInfo? GetInfo(string path, FileSystemInfo info)
    {
      if (info == null)
      {
        return null;
      }

      var newInfo = GetInfo(path);
      if (newInfo != null)
      {
        info.Type = newInfo.Type;
        info.Size = newInfo.Size;
        info.ModTime = newInfo.ModTime;
        return info;
      }

      return null;
    }

    /// <summary>
    /// Gets information about the specified file or directory, filtered by type, and populates an existing FileSystemInfo object.
    /// </summary>
    /// <param name="path">The file or directory path to check.</param>
    /// <param name="filterType">Causes getInfo to only return the info table if the item at the given path matches the specified file type.</param>
    /// <param name="info">A FileSystemInfo object which will be filled in.</param>
    /// <returns>The FileSystemInfo object given as an argument, filled with information, or null if nothing exists at the path or if it doesn't match the filterType.</returns>
    public static FileSystemInfo? GetInfo(string path, FileType filterType, FileSystemInfo info)
    {
      if (info == null)
      {
        return null;
      }

      var newInfo = GetInfo(path, filterType);
      if (newInfo != null)
      {
        info.Type = newInfo.Type;
        info.Size = newInfo.Size;
        info.ModTime = newInfo.ModTime;
        return info;
      }

      return null;
    }

    /// <summary>
    /// Reads the entire content of a file into a byte array.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <returns>A byte array containing the contents of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
    public static byte[] ReadBytes(string path)
    {
      return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Reads the entire content of a file into a string.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <returns>A string containing the contents of the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
    public static string ReadText(string path)
    {
      return File.ReadAllText(path);
    }

    /// <summary>
    /// Appends data to an existing file. If the file does not exist, it will be created.
    /// </summary>
    /// <param name="filename">The path to the file.</param>
    /// <param name="data">The data to append to the file.</param>
    /// <param name="size">The number of bytes from the data to append. If null, all data is appended.</param>
    /// <exception cref="ArgumentNullException">Thrown if filename or data is null.</exception>
    /// <exception cref="ArgumentException">Thrown if filename is empty.</exception>
    /// <exception cref="IOException">Thrown if an I/O error occurs.</exception>
    public static void Append(string filename, byte[] data, long? size = null)
    {
      if (filename == null)
      {
        throw new ArgumentNullException(nameof(filename));
      }

      if (data == null)
      {
        throw new ArgumentNullException(nameof(data));
      }

      if (string.IsNullOrEmpty(filename))
      {
        throw new ArgumentException("Filename cannot be empty.", nameof(filename));
      }

      long bytesToWrite = data.Length;
      if (size.HasValue)
      {
        if (size.Value < 0)
        {
          // Or throw new ArgumentOutOfRangeException(nameof(size), "Size cannot be negative.");
          // LÖVE's documentation doesn't specify behavior for negative size.
          // Assuming no operation for negative size, or one could throw.
          // For now, let's be lenient and write nothing if size is negative.
          // Consider logging this case if it's unexpected.
          return;
        }

        bytesToWrite = Math.Min(size.Value, data.Length);
      }

      if (bytesToWrite == 0)
      {
        return; // Nothing to write
      }

      using (var stream = new FileStream(filename, global::System.IO.FileMode.Append, FileAccess.Write))
      {
        stream.Write(data, 0, (int)bytesToWrite);
      }
    }

    /// <summary>
    /// Creates a directory.
    /// </summary>
    /// <param name="path">The path of the directory to create.</param>
    /// <returns>True if the directory was created, false if it already existed or an error occurred.</returns>
    /// <exception cref="ArgumentNullException">Thrown if path is null.</exception>
    /// <exception cref="ArgumentException">Thrown if path is empty.</exception>
    public static bool CreateDirectory(string path)
    {
      if (path == null)
      {
        throw new ArgumentNullException(nameof(path));
      }

      if (string.IsNullOrEmpty(path))
      {
        throw new ArgumentException("Path cannot be empty.", nameof(path));
      }

      if (Directory.Exists(path))
      {
        return false;
      }

      try
      {
        _ = Directory.CreateDirectory(path); // Creates all directories in the specified path, if they don't already exist.
        return true;
      }
      catch (Exception ex)
      {
        Logger.Error($"Error creating directory '{path}'.", ex);
        return false;
      }
    }

    /// <summary>
    /// Returns the application data directory.
    /// The directory is created if it doesn't exist.
    /// </summary>
    /// <returns>The full path to the application data directory.</returns>
    public static string GetAppdataDirectory()
    {
      string basePath;
      if (OperatingSystem.IsWindows())
      {
        basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      }
      else if (OperatingSystem.IsMacOS())
      {
        basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support");
      }
      else if (OperatingSystem.IsLinux())
      {
        basePath = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? string.Empty;
        if (string.IsNullOrEmpty(basePath))
        {
          basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }
      }
      else
      {
        // Fallback for other OSes or if above checks fail, though less specific
        // This could also throw an UnsupportedPlatformException
        basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // If even ApplicationData is not available (highly unlikely for supported .NET platforms)
        if (string.IsNullOrEmpty(basePath))
        {
          basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".NightFallback");
        }
      }

      string appDataPath = Path.Combine(basePath, gameIdentity);

      try
      {
        _ = Directory.CreateDirectory(appDataPath);
      }
      catch (Exception ex)
      {
        Logger.Warn($"Could not create appdata directory '{appDataPath}': {ex.Message}");
      }

      return appDataPath;
    }

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
  }
}
