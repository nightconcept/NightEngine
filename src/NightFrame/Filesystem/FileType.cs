// <copyright file="FileType.cs" company="Night Circle">
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
  /// Represents the type of a file system object.
  /// </summary>
  public enum FileType
  {
    /// <summary>
    /// A regular file.
    /// </summary>
    File,

    /// <summary>
    /// A directory.
    /// </summary>
    Directory,

    /// <summary>
    /// A symbolic link.
    /// </summary>
    Symlink,

    /// <summary>
    /// Other type (e.g., device, pipe).
    /// </summary>
    Other,

    /// <summary>
    /// The path does not exist or its type cannot be determined.
    /// </summary>
    None,
  }
}
