// <copyright file="BufferMode.cs" company="Night Circle">
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

namespace Night
{
  /// <summary>
  /// Specifies how a file's buffer is flushed.
  /// </summary>
  public enum BufferMode
  {
    /// <summary>
    /// No buffering. Data is written as soon as possible.
    /// </summary>
    None,

    /// <summary>
    /// Line buffering. Data is written when a newline character is output, or when the buffer is full.
    /// </summary>
    Line,

    /// <summary>
    /// Full buffering. Data is written only when the buffer is full.
    /// </summary>
    Full,
  }
}
