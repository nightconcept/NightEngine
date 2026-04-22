// <copyright file="Color.cs" company="Night Circle">
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
  /// Represents a color with Red, Green, Blue, and Alpha components (0-255).
  /// Alpha controls transparency (0 = transparent, 255 = opaque).
  /// </summary>
  public struct Color
  {
    // Common color presets

    /// <summary>Represents the color black.</summary>
    public static readonly Color Black = new(0, 0, 0);

    /// <summary>Represents the color white.</summary>
    public static readonly Color White = new(255, 255, 255);

    /// <summary>Represents the color red.</summary>
    public static readonly Color Red = new(255, 0, 0);

    /// <summary>Represents the color green.</summary>
    public static readonly Color Green = new(0, 255, 0);

    /// <summary>Represents the color blue.</summary>
    public static readonly Color Blue = new(0, 0, 255);

    /// <summary>Represents the color yellow.</summary>
    public static readonly Color Yellow = new(255, 255, 0);

    /// <summary>Represents the color magenta.</summary>
    public static readonly Color Magenta = new(255, 0, 255);

    /// <summary>Represents the color cyan.</summary>
    public static readonly Color Cyan = new(0, 255, 255);

    /// <summary>Represents a fully transparent color.</summary>
    public static readonly Color Transparent = new(0, 0, 0, 0);

    /// <summary>
    /// The red component of the color.
    /// </summary>
    public byte R;

    /// <summary>
    /// The green component of the color.
    /// </summary>
    public byte G;

    /// <summary>
    /// The blue component of the color.
    /// </summary>
    public byte B;

    /// <summary>
    /// The alpha (transparency) component of the color.
    /// </summary>
    public byte A;

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha component (0-255). Defaults to 255 (fully opaque).</param>
    public Color(byte r, byte g, byte b, byte a = 255)
    {
      this.R = r;
      this.G = g;
      this.B = b;
      this.A = a;
    }
  }
}
