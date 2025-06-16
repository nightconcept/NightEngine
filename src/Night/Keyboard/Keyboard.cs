// <copyright file="Keyboard.cs" company="Night Circle">
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
using System.Runtime.InteropServices;

using Night;
using Night.Log;

using SDL3;

namespace Night
{
  /// <summary>
  /// Provides an interface to the user's keyboard.
  /// </summary>
  public static class Keyboard
  {
    private static readonly ILogger Logger = LogManager.GetLogger("Night.Keyboard.Keyboard");

    /// <summary>
    /// Checks whether a certain key is down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is down, false otherwise.</returns>
    public static bool IsDown(KeyCode key)
    {
      if (!Framework.IsInputInitialized)
      {
        Logger.Warn("Night.Keyboard.IsDown called before input system is initialized. Returning false.");
        return false;
      }

      bool[] keyboardState = SDL.GetKeyboardState(out int _);

      if (keyboardState == null)
      {
        Logger.Warn("SDL.GetKeyboardState returned a null array.");
        return false;
      }

      SDL.Scancode sdlScancode = (SDL.Scancode)key;

      if (sdlScancode == SDL.Scancode.Unknown)
      {
        return false;
      }

      if ((int)sdlScancode >= keyboardState.Length || (int)sdlScancode < 0)
      {
        Logger.Warn($"Scancode {(int)sdlScancode} is out of bounds (numKeys: {keyboardState.Length}).");
        return false;
      }

      return keyboardState[(int)sdlScancode];
    }
  }
}
