// <copyright file="Window.Display.cs" company="Night Circle">
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using Night.Log;

using SDL3;

namespace Night
{
  /// <summary>
  /// Contains display-related functionality for the <see cref="Window"/> class.
  /// </summary>
  public static partial class Window
  {
    /// <summary>
    /// Gets the number of connected monitors.
    /// </summary>
    /// <returns>The number of currently connected displays.</returns>
    public static int GetDisplayCount()
    {
      uint[]? displays = SDL.GetDisplays(out int count);
      if (displays == null || count < 0)
      {
        return 0;
      }

      return count;
    }

    /// <summary>
    /// Gets the width and height of the desktop.
    /// </summary>
    /// <param name="displayIndex">The index of the display to query (0 for the primary display).</param>
    /// <returns>A tuple containing the width and height of the desktop, or (0,0) if an error occurs.</returns>
    public static (int Width, int Height) GetDesktopDimensions(int displayIndex = 0)
    {
      uint[]? actualDisplayIDs = SDL.GetDisplays(out int displayCount);
      if (actualDisplayIDs == null || displayCount <= 0 || displayIndex < 0 || displayIndex >= displayCount)
      {
        return (0, 0);
      }

      uint targetDisplayID = actualDisplayIDs[displayIndex];

      SDL.DisplayMode? mode = SDL.GetDesktopDisplayMode(targetDisplayID);
      if (mode == null)
      {
        return (0, 0);
      }

      return (mode.Value.W, mode.Value.H);
    }

    /// <summary>
    /// Gets whether the window is fullscreen.
    /// </summary>
    /// <returns>A tuple: (bool IsFullscreen, FullscreenType FsType).
    /// IsFullscreen is true if the window is in any fullscreen mode, false otherwise.
    /// FsType indicates the type of fullscreen mode used.</returns>
    public static (bool IsFullscreen, FullscreenType FsType) GetFullscreen()
    {
      if (window == nint.Zero)
      {
        return (false, currentFullscreenType);
      }

      var flags = SDL.GetWindowFlags(window);

      if ((flags & SDL.WindowFlags.Fullscreen) != 0)
      {
        return (true, FullscreenType.Exclusive);
      }

      if (currentFullscreenType == FullscreenType.Desktop && (flags & SDL.WindowFlags.Borderless) != 0)
      {
        return (true, FullscreenType.Desktop);
      }

      return (false, currentFullscreenType);
    }

    /// <summary>
    /// Enters or exits fullscreen.
    /// </summary>
    /// <param name="fullscreen">Whether to enter or exit fullscreen mode.</param>
    /// <param name="fsType">The type of fullscreen mode to use (Desktop or Exclusive).</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    public static bool SetFullscreen(bool fullscreen, FullscreenType fsType = FullscreenType.Desktop)
    {
      if (window == nint.Zero)
      {
        return false;
      }

      if (fullscreen)
      {
        currentFullscreenType = fsType;
        if (fsType == FullscreenType.Exclusive)
        {
          uint displayID = SDL.GetDisplayForWindow(window);
          if (displayID == 0 && !string.IsNullOrEmpty(SDL.GetError()))
          {
            return false;
          }

          SDL.DisplayMode? dm = SDL.GetDesktopDisplayMode(displayID);
          if (dm.HasValue)
          {
            if (!SDL.SetWindowFullscreenMode(window, dm.Value))
            {
              return false;
            }
          }
          else
          {
            return false;
          }
        }
        else
        {
          if (!SDL.SetWindowFullscreenMode(window, nint.Zero))
          {
            Logger.Warn($"SetFullscreen (Desktop): SDL_SetWindowFullscreenMode(NULL) failed: {SDL.GetError()}");
          }

          if (!SDL.SetWindowBordered(window, false))
          {
            Logger.Error($"SetFullscreen (Desktop): SDL_SetWindowBordered(false) failed: {SDL.GetError()}");
            return false;
          }

          uint displayID = SDL.GetDisplayForWindow(window);
          string errorCheck = SDL.GetError();
          if (displayID == 0 && !string.IsNullOrEmpty(errorCheck))
          {
            Logger.Error($"SetFullscreen (Desktop): SDL_GetDisplayForWindow failed: {errorCheck}");
            return false;
          }

          var (desktopW, desktopH) = GetDesktopDimensionsForDisplayID(displayID);

          if (desktopW > 0 && desktopH > 0)
          {
            _ = SDL.SetWindowPosition(window, 0, 0);
            if (!SDL.SetWindowSize(window, desktopW, desktopH))
            {
              Logger.Warn($"SetFullscreen (Desktop): SDL_SetWindowSize({desktopW},{desktopH}) failed: {SDL.GetError()}");
            }
          }
          else
          {
            Logger.Error($"SetFullscreen (Desktop): GetDesktopDimensionsForDisplayID failed for display {displayID}.");
            return false;
          }
        }
      }
      else
      {
        currentFullscreenType = FullscreenType.Desktop;
        if (!SDL.SetWindowFullscreenMode(window, nint.Zero))
        {
          Logger.Warn($"SetFullscreen (Exit): SDL_SetWindowFullscreenMode(NULL) failed: {SDL.GetError()}");
        }

        if (!SDL.SetWindowBordered(window, true))
        {
          Logger.Error($"SetFullscreen (Exit): SDL_SetWindowBordered(true) failed: {SDL.GetError()}");
          return false;
        }

        var config = ConfigurationManager.CurrentConfig.Window;
        int restoreWidth = config.Width > 0 ? config.Width : 800;
        int restoreHeight = config.Height > 0 ? config.Height : 600;

        if (!SDL.SetWindowSize(window, restoreWidth, restoreHeight))
        {
          Logger.Warn($"SetFullscreen (Exit): SDL_SetWindowSize({restoreWidth},{restoreHeight}) failed: {SDL.GetError()}");
        }

        if (config.X.HasValue && config.Y.HasValue)
        {
          _ = SDL.SetWindowPosition(window, config.X.Value, config.Y.Value);
        }
        else
        {
          _ = SDL.SetWindowPosition(window, (int)SDL.WindowposCenteredMask, (int)SDL.WindowposCenteredMask); // Assumes primary display (display 0)
        }

        _ = SDL.RaiseWindow(window);
      }

      return true;
    }
  }
}
