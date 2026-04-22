// <copyright file="NightSDL.cs" company="Night Circle">
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

using SDL3;

namespace Night
{
  /// <summary>
  /// Provides direct access to some SDL3 functions using SDL3-CS bindings.
  /// </summary>
  public static class NightSDL
  {
    /// <summary>
    /// Get the version of SDL that is linked against the Night Engine.
    /// Calls the SDL3-CS binding for SDL_GetVersion() and returns a packed int.
    /// https://wiki.libsdl.org/SDL3/SDL_GetVersion.
    /// </summary>
    /// <returns>A string representing the SDL version "major.minor.patch".</returns>
    public static string GetVersion()
    {
      int sdl_version = SDL.GetVersion();
      int major = sdl_version / 1000000;
      int minor = (sdl_version / 1000) % 1000;
      int patch = sdl_version % 1000;
      return $"{major}.{minor}.{patch}";
    }

    /// <summary>
    /// Retrieve a message about the last error that occurred on the current thread.
    /// Calls the SDL3-CS binding for SDL_GetError() and returns a string.
    /// https://wiki.libsdl.org/SDL3/SDL_GetError.
    /// </summary>
    /// <returns>Returns a message with information about the specific error that occurred, or an empty string if there hasn't been an error message set since the last call to SDL_ClearError().</returns>
    public static string GetError()
    {
      return SDL.GetError();
    }
  }
}
