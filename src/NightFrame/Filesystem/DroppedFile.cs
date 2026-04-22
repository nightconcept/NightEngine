// <copyright file="DroppedFile.cs" company="Night Circle">
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
/// Represents a file that has been dropped onto the application window.
/// </summary>
/// <remarks>
/// This object is created by the framework when a file drop event occurs. It provides the absolute path
/// to the dropped file, which can then be used with other <see cref="Filesystem"/> functions.
/// </remarks>
public class DroppedFile
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DroppedFile"/> class.
  /// </summary>
  /// <param name="path">The absolute path of the dropped file.</param>
  internal DroppedFile(string path)
  {
    this.Path = path;
  }

  /// <summary>
  /// Gets the absolute path of the dropped file.
  /// </summary>
  public string Path { get; }
}
