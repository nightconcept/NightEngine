// <copyright file="Filesystem.NewFileData.cs" company="Night Circle">
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

namespace Night;

/// <summary>
/// Provides an interface to the user's filesystem, mirroring Love2D's `love.filesystem` module.
/// </summary>
public static partial class Filesystem
{
  /// <summary>
  /// Creates a new <see cref="FileData"/> object from a byte array.
  /// </summary>
  /// <param name="data">The byte array containing the file data.</param>
  /// <param name="name">The name to use as the filename hint.</param>
  /// <returns>A new <see cref="FileData"/> object.</returns>
  public static FileData NewFileData(byte[] data, string name)
  {
    return new FileData(data, name);
  }

  /// <summary>
  /// Creates a new <see cref="FileData"/> object from a string.
  /// </summary>
  /// <param name="content">The string content.</param>
  /// <param name="name">The name to use as the filename hint.</param>
  /// <returns>A new <see cref="FileData"/> object.</returns>
  public static FileData NewFileData(string content, string name)
  {
    return new FileData(content, name);
  }
}
