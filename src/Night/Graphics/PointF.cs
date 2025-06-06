// <copyright file="PointF.cs" company="Night Circle">
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
  /// Represents a 2D point with floating-point coordinates.
  /// </summary>
  public struct PointF
  {
    /// <summary>
    /// The X-coordinate of the point.
    /// </summary>
    public float X;

    /// <summary>
    /// The Y-coordinate of the point.
    /// </summary>
    public float Y;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointF"/> struct.
    /// </summary>
    /// <param name="x">The X-coordinate.</param>
    /// <param name="y">The Y-coordinate.</param>
    public PointF(float x, float y)
    {
      this.X = x;
      this.Y = y;
    }
  }
}
