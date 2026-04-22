// <copyright file="FileSystemInfo.cs" company="Night Circle">
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

namespace Night
{
  /// <summary>
  /// Contains information about a file or directory.
  /// </summary>
  public class FileSystemInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemInfo"/> class.
    /// </summary>
    public FileSystemInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemInfo"/> class with specified values.
    /// </summary>
    /// <param name="type">The type of the file system object.</param>
    /// <param name="size">The size of the file in bytes.</param>
    /// <param name="modTime">The last modification time in Unix epoch seconds.</param>
    public FileSystemInfo(FileType type, long? size, long? modTime)
    {
      this.Type = type;
      this.Size = size;
      this.ModTime = modTime;
    }

    /// <summary>
    /// Gets or sets the type of the object at the path (file, directory, symlink, etc.).
    /// Default is None.
    /// </summary>
    public FileType Type { get; set; } = FileType.None;

    /// <summary>
    /// Gets or sets the size in bytes of the file, or null if it can't be determined or not applicable (e.g. for a directory).
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Gets or sets the file's last modification time in seconds since the Unix epoch, or null if it can't be determined.
    /// </summary>
    public long? ModTime { get; set; }
  }
}
