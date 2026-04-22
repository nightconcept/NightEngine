// <copyright file="ImageData.cs" company="Night Circle">
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
  /// Represents raw pixel data of an image.
  /// </summary>
  public class ImageData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageData"/> class.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="data">The pixel data (assumed RGBA, 4 bytes per pixel).</param>
    public ImageData(int width, int height, byte[] data)
    {
      if (data == null)
      {
        throw new ArgumentNullException(nameof(data));
      }

      if (width <= 0 || height <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
      }

      // Assuming 4 bytes per pixel (RGBA)
      // SDL_Surface pixels are often in BGRA or ARGB depending on the surface format and endianness.
      // We need to be careful here. SDL_PIXELFORMAT_RGBA32 is a common target.
      // For SDL_LoadBMP, the surface is often in BGR format.
      // A conversion step might be needed if the surface is not RGBA.
      // For simplicity now, we assume data provided is already RGBA.
      if (data.Length != width * height * 4)
      {
        throw new ArgumentException("Data length does not match width, height, and RGBA format (4 bytes per pixel).", nameof(data));
      }

      this.Width = width;
      this.Height = height;
      this.Data = data;
    }

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the raw pixel data.
    /// Assumed to be in RGBA format, 4 bytes per pixel.
    /// The data is stored as a copy of the input array.
    /// </summary>
    public byte[] Data { get; }
  }
}
