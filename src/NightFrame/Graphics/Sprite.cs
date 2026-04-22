// <copyright file="Sprite.cs" company="Night Circle">
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
  /// A 2D sprite that can be drawn to the screen.
  /// </summary>
  public class Sprite
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class.
    /// </summary>
    /// <param name="texture">The SDL texture handle.</param>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    public Sprite(IntPtr texture, int width, int height)
    {
      this.Texture = texture;
      this.Width = width;
      this.Height = height;
    }

    /// <summary>
    /// Gets the SDL texture handle.
    /// </summary>
    public IntPtr Texture { get; }

    /// <summary>
    /// Gets the width of the sprite in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the sprite in pixels.
    /// </summary>
    public int Height { get; }
  }
}
