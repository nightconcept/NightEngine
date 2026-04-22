// <copyright file="MouseButton.cs" company="Night Circle">
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
  /// Represents mouse buttons. Values correspond to SDL_MouseButtonFlags/
  /// SDL3.SDL.Button* constants.
  /// (e.g., Left is 1, Middle is 2, etc.)
  /// </summary>
  public enum MouseButton
  {
    /// <summary>
    /// An unknown or unspecified mouse button.
    /// </summary>
    Unknown = 0, // Not a direct SDL button constant

    /// <summary>
    /// The left mouse button.
    /// </summary>
    Left = 1,    // Corresponds to SDL.ButtonLeft

    /// <summary>
    /// The middle mouse button.
    /// </summary>
    Middle = 2,  // Corresponds to SDL.ButtonMiddle

    /// <summary>
    /// The right mouse button.
    /// </summary>
    Right = 3,   // Corresponds to SDL.ButtonRight

    /// <summary>
    /// The first extra mouse button (often "back").
    /// </summary>
    X1 = 4,      // Corresponds to SDL.ButtonX1 (Typically "back")

    /// <summary>
    /// The second extra mouse button (often "forward").
    /// </summary>
    X2 = 5,       // Corresponds to SDL.ButtonX2 (Typically "forward")
  }
}
