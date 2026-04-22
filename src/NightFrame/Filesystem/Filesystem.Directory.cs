// <copyright file="Filesystem.Directory.cs" company="Night Circle">
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
using System.Linq;

namespace Night
{
  /// <summary>
  /// Provides an interface to the user's filesystem.
  /// </summary>
  public static partial class Filesystem
  {
    /// <summary>
    /// Gets a list of all files and subdirectories in a given path.
    /// </summary>
    /// <remarks>
    /// The path is resolved by checking the save directory first, then the source directory.
    /// If the given relative path exists in both the save and source directories, their contents are merged.
    /// If a file or directory with the same name exists in both, the one from the save directory takes precedence.
    /// </remarks>
    /// <param name="path">The relative path of the directory to list items from.</param>
    /// <returns>An enumerable collection of file and directory names relative to the given path.</returns>
    public static IEnumerable<string> GetDirectoryItems(string path)
    {
      var items = new HashSet<string>();

      // 1. Check Save Directory
      string savePath = Path.Combine(GetSaveDirectory(), path);
      if (Directory.Exists(savePath))
      {
        try
        {
          foreach (var entry in Directory.EnumerateFileSystemEntries(savePath))
          {
            _ = items.Add(Path.GetFileName(entry));
          }
        }
        catch (Exception e)
        {
          Logger.Error($"Failed to enumerate items in save directory path '{savePath}': {e.Message}", e);
        }
      }

      // 2. Check Source Directory
      string sourcePath = Path.Combine(GetSource(), path);
      if (Directory.Exists(sourcePath))
      {
        try
        {
          foreach (var entry in Directory.EnumerateFileSystemEntries(sourcePath))
          {
            // Add only if not already present from the save directory
            _ = items.Add(Path.GetFileName(entry));
          }
        }
        catch (Exception e)
        {
          Logger.Error($"Failed to enumerate items in source directory path '{sourcePath}': {e.Message}", e);
        }
      }

      return items.OrderBy(s => s, StringComparer.Ordinal);
    }
  }
}
