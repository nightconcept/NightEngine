// <copyright file="Filesystem.Remove.cs" company="Night Circle">
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

namespace Night
{
  /// <summary>
  /// Provides an interface to the user's filesystem.
  /// </summary>
  public static partial class Filesystem
  {
    /// <summary>
    /// Removes a file or an empty directory from the save directory.
    /// </summary>
    /// <param name="filepath">The path of the file or directory to remove, relative to the save directory.</param>
    /// <returns><c>true</c> if the file or directory was successfully removed, <c>false</c> otherwise.</returns>
    /// <remarks>
    /// This operation is restricted to the game's save directory. Attempting to remove files
    /// or directories outside of this location will fail. This method will also fail if
    /// attempting to remove a directory that is not empty.
    /// </remarks>
    public static bool Remove(string filepath)
    {
      if (string.IsNullOrWhiteSpace(filepath))
      {
        Logger.Warn("Remove failed: filepath cannot be null or empty.");
        return false;
      }

      try
      {
        string saveDir = GetSaveDirectory();
        string fullPath = Path.GetFullPath(Path.Combine(saveDir, filepath));

        // Security check: Ensure the resolved path is within the save directory.
        if (!fullPath.StartsWith(saveDir, StringComparison.Ordinal))
        {
          Logger.Error($"Remove failed: Cannot remove '{filepath}' as it is outside the save directory.");
          return false;
        }

        if (File.Exists(fullPath))
        {
          File.Delete(fullPath);
          Logger.Info($"Successfully removed file: '{filepath}'");
          return true;
        }

        if (Directory.Exists(fullPath))
        {
          // Directory.Delete(path) throws an IOException if the directory is not empty.
          Directory.Delete(fullPath);
          Logger.Info($"Successfully removed empty directory: '{filepath}'");
          return true;
        }

        // Path does not exist
        Logger.Warn($"Remove failed: File or directory not found at '{filepath}'.");
        return false;
      }
      catch (IOException ex)
      {
        // This can happen if the directory is not empty, or file is in use.
        Logger.Error($"Remove failed for '{filepath}'. IO Error: {ex.Message}", ex);
        return false;
      }
      catch (UnauthorizedAccessException ex)
      {
        Logger.Error($"Remove failed for '{filepath}'. Insufficient permissions. Error: {ex.Message}", ex);
        return false;
      }
      catch (SecurityException ex)
      {
        Logger.Error($"Remove failed for '{filepath}'. Security error. Error: {ex.Message}", ex);
        return false;
      }
      catch (Exception ex)
      {
        Logger.Error($"An unexpected error occurred while trying to remove '{filepath}'. Error: {ex.Message}", ex);
        return false;
      }
    }
  }
}
