// <copyright file="FileMode.cs" company="Night Circle">
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
  /// Represents the different modes you can open a File in.
  /// </summary>
  public enum FileMode
  {
    /// <summary>
    /// Open a file for read.
    /// </summary>
    Read,

    /// <summary>
    /// Open a file for write.
    /// </summary>
    Write,

    /// <summary>
    /// Open a file for append.
    /// </summary>
    Append,

    /// <summary>
    /// Do not open a file (represents a closed file.)
    /// </summary>
    Close,

    /// <summary>
    /// Open a file for write.
    /// </summary>
    W = Write,

    /// <summary>
    /// Open a file for read.
    /// </summary>
    R = Read,

    /// <summary>
    /// Open a file for append.
    /// </summary>
    A = Append,

    /// <summary>
    /// Do not open a file (represents a closed file.)
    /// </summary>
    C = Close,
  }
}
