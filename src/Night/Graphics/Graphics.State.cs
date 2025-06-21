// <copyright file="Graphics.State.cs" company="Night Circle">
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
  /// Provides 2D graphics rendering functionality. This part handles graphics state.
  /// </summary>
  public static partial class Graphics
  {
    private static Color backgroundColor = Color.Black; // Default to black (0,0,0,255)

    /// <summary>
    /// Gets the current background color.
    /// </summary>
    /// <returns>The current background <see cref="Color"/>.</returns>
    /// <remarks>
    /// This reflects the color set by the last call to <see cref="Graphics.Clear(Night.Color)"/>,
    /// or the default color if Clear hasn't been called.
    /// </remarks>
    public static Color GetBackgroundColor()
    {
      return backgroundColor;
    }
  }
}
