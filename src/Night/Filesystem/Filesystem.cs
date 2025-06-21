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
using System.Text;

using Night.Log;

namespace Night
{
  /// <summary>
  /// Provides an interface to the user's filesystem.
  /// </summary>
  public static partial class Filesystem
  {
    private static readonly ILogger Logger = LogManager.GetLogger("Night.Filesystem.Filesystem");
    private static string _gameIdentity = "NightDefault";
    private static readonly object GameIdentityLock = new object();
    private static string? _saveDirectoryCache;
    private static string? _sourceDirectoryCache;

    /// <summary>
    /// Specifies the type to return file contents as when reading.
    /// </summary>
    public enum ContainerType
    {
      /// <summary>
      /// Read content as a string.
      /// </summary>
      String,

      /// <summary>
      /// Read content as raw byte data.
      /// </summary>
      Data,
    }

    /// <summary>
    /// Sets the identity of the game. This is used to determine the save directory.
    /// </summary>
    /// <param name="identityName">The name to use for the game's identity.
    /// If null or empty, the identity will be reset to "NightDefault".
    /// Invalid path characters will be replaced with underscores.</param>
    public static void SetIdentity(string? identityName)
    {
      lock (GameIdentityLock)
      {
        if (string.IsNullOrWhiteSpace(identityName))
        {
          _gameIdentity = "NightDefault";
          Logger.Info("Game identity reset to default: NightDefault.");
        }
        else
        {
          string sanitizedName = identityName;
          char[] invalidChars = Path.GetInvalidFileNameChars(); // Identity is used as a directory name
          foreach (char c in invalidChars)
          {
            if (sanitizedName.Contains(c))
            {
              sanitizedName = sanitizedName.Replace(c, '_');
            }
          }

          if (sanitizedName != identityName)
          {
            Logger.Warn($"Game identity '{identityName}' contained invalid characters and was sanitized to '{sanitizedName}'.");
          }

          _gameIdentity = sanitizedName;
        }

        // Invalidate cached save directory path as it depends on identity
        _saveDirectoryCache = null;
        Logger.Info($"Game identity set to: {_gameIdentity}");
      }
    }

    /// <summary>
    /// Gets the current identity of the game.
    /// </summary>
    /// <returns>The current game identity.</returns>
    public static string GetIdentity()
    {
      lock (GameIdentityLock)
      {
        return _gameIdentity;
      }
    }

    /// <summary>
    /// Gets the full path to the directory where the game can save files.
    /// The directory is created if it doesn't exist.
    /// The path depends on the operating system and the game's identity.
    /// </summary>
    /// <returns>The absolute path to the save directory.</returns>
    /// <exception cref="IOException">Thrown if the save directory could not be created.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if permissions are insufficient to create the save directory.</exception>
    public static string GetSaveDirectory()
    {
      lock (GameIdentityLock)
      {
        if (_saveDirectoryCache != null)
        {
          return _saveDirectoryCache;
        }

        string basePath;
        string nightFolderName = "Night"; // For Windows and macOS

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
          nightFolderName = "night"; // Lowercase for Linux as per Love2D convention
          basePath = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? string.Empty;
          if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath)) // Check if XDG_DATA_HOME is valid
          {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
          }
        }
        else
        {
          // Fallback for other OSes, though less specific.
          // Consider throwing UnsupportedPlatformException if strict adherence to defined platforms is required.
          Logger.Warn($"Unsupported OS detected for save directory. Falling back to ApplicationData folder with 'Night' subfolder.");
          basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
          if (string.IsNullOrEmpty(basePath)) // Highly unlikely, but as a last resort
          {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".NightFallbackData");
            Logger.Warn($"ApplicationData folder not found. Using fallback: {basePath}");
          }
        }

        string savePath = Path.Combine(basePath, nightFolderName, _gameIdentity);

        try
        {
          if (!Directory.Exists(savePath))
          {
            Directory.CreateDirectory(savePath);
            Logger.Info($"Created save directory: {savePath}");
          }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
        {
          Logger.Error($"Failed to create or access save directory '{savePath}'. Error: {ex.Message}", ex);
          throw; // Re-throw critical exceptions
        }
        catch (Exception ex)
        {
          Logger.Error($"An unexpected error occurred while creating save directory '{savePath}'. Error: {ex.Message}", ex);
          // Depending on policy, might throw a more generic exception or a custom one.
          // For now, re-throw to indicate failure.
          throw new IOException($"Could not ensure save directory exists at '{savePath}'.", ex);
        }

        _saveDirectoryCache = savePath;
        return savePath;
      }
    }

    /// <summary>
    /// Gets the full path to the application's source directory (usually the directory containing the executable).
    /// </summary>
    /// <returns>The absolute path to the source directory.</returns>
    public static string GetSource()
    {
      if (_sourceDirectoryCache == null)
      {
        _sourceDirectoryCache = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
      }

      return _sourceDirectoryCache;
    }

    /// <summary>
    /// Gets the full path to the directory containing the application's source directory.
    /// </summary>
    /// <returns>The absolute path to the base directory of the source, or null if the source is a root directory.</returns>
    public static string? GetSourceBaseDirectory()
    {
      return Path.GetDirectoryName(GetSource());
    }

    /// <summary>
    /// Gets the full path to the current user's home directory.
    /// </summary>
    /// <returns>The absolute path to the user's home directory.</returns>
    public static string GetUserDirectory()
    {
      return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    /// <summary>
    /// Gets the current working directory of the application.
    /// </summary>
    /// <returns>The absolute path to the current working directory.</returns>
    public static string GetWorkingDirectory()
    {
      return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Gets whether the game is in "fused mode".
    /// In Night, this concept is not directly applicable as with .love files, so it always returns false.
    /// </summary>
    /// <returns><c>false</c>.</returns>
    public static bool IsFused()
    {
      return false;
    }

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

      if (string.IsNullOrWhiteSpace(path))
      {
        throw new ArgumentException("Path cannot be empty or consist only of whitespace.", nameof(path));
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
      // This method's behavior might need to be re-evaluated in light of GetSaveDirectory().
      // For now, it retains its original logic but uses the locked GetIdentity().
      string currentIdentity = GetIdentity(); // Use the thread-safe getter

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
        if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
        {
          basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }
      }
      else
      {
        basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(basePath))
        {
          basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".NightFallbackData"); // Ensure distinct from GetSaveDirectory fallback
        }
      }

      // Original GetAppdataDirectory combines directly with identity, e.g., %APPDATA%\MyGame
      // GetSaveDirectory combines with "Night" then identity, e.g., %APPDATA%\Night\MyGame
      // This distinction is maintained here.
      string appDataPath = Path.Combine(basePath, currentIdentity);

      try
      {
        if (!Directory.Exists(appDataPath))
        {
          Directory.CreateDirectory(appDataPath);
          Logger.Info($"Created appdata directory (legacy GetAppdataDirectory call): {appDataPath}");
        }
      }
      catch (Exception ex)
      {
        Logger.Warn($"Could not create appdata directory '{appDataPath}' (legacy GetAppdataDirectory call): {ex.Message}");
        // Depending on requirements, this might throw or return a non-guaranteed path.
        // For now, it returns the path even if creation failed, consistent with original.
      }

      return appDataPath;
    }

    /// <summary>
    /// Creates a new File object. It needs to be opened before it can be accessed.
    /// </summary>
    /// <param name="filename">The filename of the file.</param>
    /// <returns>The new NightFile object.</returns>
    /// <exception cref="ArgumentNullException">Thrown if filename is null or empty.</exception>
    public static NightFile NewFile(string filename)
    {
      // Note: LÖVE's love.filesystem.newFile(filename) does not error at this stage
      // for invalid filenames, deferring errors to File:open.
      // Our NightFile constructor will throw ArgumentNullException if filename is null/empty,
      // which is a reasonable basic validation.
      return new NightFile(filename);
    }

    /// <summary>
    /// Creates a File object and opens it for reading, writing, or appending.
    /// </summary>
    /// <param name="filename">The filename of the file.</param>
    /// <param name="mode">The mode to open the file in.</param>
    /// <returns>A tuple containing the new NightFile object (or null if an error occurred) and an error string if an error occurred.</returns>
    public static (NightFile? File, string? ErrorStr) NewFile(string filename, FileMode mode)
    {
      try
      {
        var file = new NightFile(filename);
        (bool success, string? error) = file.Open(mode);
        if (success)
        {
          return (file, null);
        }
        else
        {
          // Ensure the file object is disposed if open failed, though NightFile's Open should handle internal state.
          // If Open fails, the FileStream might not be created, or if created and failed, it should be handled there.
          // For safety, we could call Dispose, but it might be redundant if Open cleans up.
          // LÖVE returns nil for the file object on error.
          return (null, error);
        }
      }
      catch (ArgumentNullException ex)
      {
        return (null, ex.Message);
      }
      catch (Exception ex)
      {
        Logger.Error($"Unexpected error in Filesystem.NewFile('{filename}', '{mode}'): {ex.Message}", ex);
        return (null, $"An unexpected error occurred: {ex.Message}");
      }
    }
  }
}
