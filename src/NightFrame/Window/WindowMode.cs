// <copyright file="WindowMode.cs" company="Night Circle">
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
  /// Represents the display mode of a window, including width, height, and other properties.
  /// </summary>
  public struct WindowMode
  {
    /// <summary>
    /// Gets or sets the window width in logical units.
    /// </summary>
    public int Width;

    /// <summary>
    /// Gets or sets the window height in logical units.
    /// </summary>
    public int Height;

    /// <summary>
    /// Gets or sets the window width in physical pixels.
    /// </summary>
    public int PixelWidth;

    /// <summary>
    /// Gets or sets the window height in physical pixels.
    /// </summary>
    public int PixelHeight;

    /// <summary>
    /// Gets or sets a value indicating whether the window is in fullscreen mode.
    /// </summary>
    public bool Fullscreen;

    /// <summary>
    /// Gets or sets the type of fullscreen mode used.
    /// </summary>
    public FullscreenType FullscreenType;

    /// <summary>
    /// Gets or sets a value indicating whether the window is borderless.
    /// </summary>
    public bool Borderless;

    /// <summary>
    /// Gets or sets the VSync mode. 1 if the graphics framerate is synchronized with the monitor's refresh rate, 0 otherwise.
    /// </summary>
    public int Vsync;

    /// <summary>
    /// Gets or sets the number of antialiasing samples used (0 if MSAA is disabled).
    /// </summary>
    public int Msaa;

    /// <summary>
    /// Gets or sets a value indicating whether the window is resizable in windowed mode.
    /// </summary>
    public bool Resizable;

    /// <summary>
    /// Gets or sets a value indicating whether the window is centered in windowed mode.
    /// </summary>
    public bool Centered;

    /// <summary>
    /// Gets or sets the 1-based index of the display the window is currently in.
    /// </summary>
    public int Display;

    /// <summary>
    /// Gets or sets the minimum width of the window, if resizable.
    /// </summary>
    public int MinWidth;

    /// <summary>
    /// Gets or sets the minimum height of the window, if resizable.
    /// </summary>
    public int MinHeight;

    /// <summary>
    /// Gets or sets the maximum width of the window, if resizable.
    /// </summary>
    public int MaxWidth;

    /// <summary>
    /// Gets or sets the maximum height of the window, if resizable.
    /// </summary>
    public int MaxHeight;

    /// <summary>
    /// Gets or sets a value indicating whether high-dpi mode is allowed.
    /// </summary>
    public bool HighDpi;

    /// <summary>
    /// Gets or sets the refresh rate of the screen's current display mode in Hz (0 if undetermined).
    /// </summary>
    public int RefreshRate;

    /// <summary>
    /// Gets or sets the x-coordinate of the window's position.
    /// </summary>
    public int X;

    /// <summary>
    /// Gets or sets the y-coordinate of the window's position.
    /// </summary>
    public int Y;

    /// <summary>
    /// Gets or sets the window title.
    /// </summary>
    public string Title;
  }
}
