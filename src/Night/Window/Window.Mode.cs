// <copyright file="Window.Mode.cs" company="Night Circle">
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
  /// Contains window mode and attribute functionality for the <see cref="Window"/> class.
  /// </summary>
  public static partial class Window
  {
    /// <summary>
    /// Sets the display mode and properties of the window.
    /// </summary>
    /// <param name="width">The width of the window.</param>
    /// <param name="height">The height of the window.</param>
    /// <param name="flags">SDL Window flags to apply.</param>
    /// <returns>True if the mode was set successfully, false otherwise.</returns>
    public static bool SetMode(int width, int height, SDL.WindowFlags flags)
    {
      lock (WindowLock)
      {
        Logger.Info($"Attempting to set mode {width}x{height} with flags: {flags}");
        Logger.Debug($"Current Thread ID: {Thread.CurrentThread.ManagedThreadId}");

        if (window != nint.Zero)
        {
          Logger.Info($"Existing window found (Handle: {window}). Destroying old window and renderer.");
          if (renderer != nint.Zero)
          {
            SDL.DestroyRenderer(renderer);
            renderer = nint.Zero;
            Logger.Debug("Old renderer destroyed.");
          }

          SDL.DestroyWindow(window);
          window = nint.Zero;
          isWindowOpen = false;
          Logger.Debug("Old window destroyed.");
        }

        Logger.Debug("[PRE-CREATE] Clearing any previous SDL errors");
        _ = SDL.ClearError();
        string preCreateError = SDL.GetError();
        Logger.Debug($"[PRE-CREATE] SDL error after clear: '{preCreateError}'");

        if (SDL.GetCurrentVideoDriver() == "offscreen")
        {
          // The offscreen driver on macOS may require a graphics backend hint even for software rendering.
          // We use the OpenGL flag to bypass potential Metal initialization issues in a headless environment.
          if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
          {
            Logger.Info("Offscreen driver on macOS detected. Adding OpenGL flag for test window creation.");
            flags |= SDL.WindowFlags.OpenGL;
          }
        }

        Logger.Debug($"[PRE-CREATE] About to call SDL.CreateWindow with parameters:");
        Logger.Debug($"  - title: 'Night Engine'");
        Logger.Debug($"  - width: {width}");
        Logger.Debug($"  - height: {height}");
        Logger.Debug($"  - flags: {flags} (0x{(uint)flags:X})");
        Logger.Debug($"  - Thread ID: {Thread.CurrentThread.ManagedThreadId}");

        window = SDL.CreateWindow("Night Engine", width, height, flags);

        Logger.Debug($"[POST-CREATE] SDL.CreateWindow returned: {window} (0x{window:X})");

        if (window == nint.Zero)
        {
          isWindowOpen = false;
          Logger.Error($"[POST-CREATE] SDL.CreateWindow FAILED - returned null pointer");

          string immediateError = SDL.GetError();
          Logger.Error($"[POST-CREATE] Immediate SDL.GetError(): '{immediateError}'");

          SDL.Delay(10);
          string delayedError1 = SDL.GetError();
          Logger.Error($"[POST-CREATE] SDL.GetError() after 10ms delay: '{delayedError1}'");

          SDL.Delay(50);
          string delayedError2 = SDL.GetError();
          Logger.Error($"[POST-CREATE] SDL.GetError() after 60ms total delay: '{delayedError2}'");

          Logger.Debug($"[POST-CREATE] Attempting to get video driver info for diagnostics...");
          try
          {
            string videoDriver = SDL.GetCurrentVideoDriver() ?? string.Empty;
            Logger.Debug($"[POST-CREATE] Current video driver: '{videoDriver}'");
          }
          catch (Exception ex)
          {
            Logger.Warn($"[POST-CREATE] Failed to get video driver: {ex.Message}");
          }

          Logger.Error($"SDL.CreateWindow failed. Final SDL Error: '{delayedError2}'");
          return false;
        }

        Logger.Info($"SDL.CreateWindow succeeded. New Window Handle: {window}");

        string? initialRendererError = null;
        renderer = SDL.CreateRenderer(window, null);
        if (renderer == nint.Zero)
        {
          initialRendererError = SDL.GetError() ?? "Unknown error (hardware renderer)";
          Logger.Warn($"SDL.CreateRenderer (hardware) failed: {initialRendererError}. Attempting software renderer.");

          nint surface = SDL.GetWindowSurface(window);
          if (surface == nint.Zero)
          {
            string windowSurfaceError = SDL.GetError() ?? "Unknown error (getting window surface for software renderer)";
            string relevantError = string.IsNullOrEmpty(initialRendererError) || initialRendererError.Contains("Unknown error") ? windowSurfaceError : initialRendererError;
            Logger.Error($"SDL.GetWindowSurface failed. Relevant Error: {relevantError}");
            SDL.DestroyWindow(window);
            window = nint.Zero;
            isWindowOpen = false;
            return false;
          }

          Logger.Debug("SDL.GetWindowSurface succeeded for software fallback.");

          renderer = SDL.CreateSoftwareRenderer(surface);
          if (renderer == nint.Zero)
          {
            string softwareRendererError = SDL.GetError() ?? "Unknown error (software renderer)";
            string combinedError = string.IsNullOrEmpty(initialRendererError) || initialRendererError.Contains("Unknown error") ? softwareRendererError : initialRendererError;
            if (!string.IsNullOrEmpty(softwareRendererError) && !softwareRendererError.Contains("Unknown error") && softwareRendererError != initialRendererError)
            {
              combinedError += $" (Software attempt also failed: {softwareRendererError})";
            }

            Logger.Error($"SDL.CreateSoftwareRenderer failed. Combined/Relevant Error: {combinedError}");
            SDL.DestroyWindow(window);
            window = nint.Zero;
            isWindowOpen = false;
            return false;
          }

          Logger.Info($"Successfully created software renderer. RendererPtr: {renderer}");
        }
        else
        {
          Logger.Info($"Successfully created hardware renderer. RendererPtr: {renderer}");
        }

        isWindowOpen = true;
        Logger.Info($"SetMode completed. isWindowOpen: {isWindowOpen}, Window.Handle: {Handle}, RendererPtr: {RendererPtr}");
        return true;
      }
    }

    /// <summary>
    /// Gets the current window mode (width, height, and flags).
    /// </summary>
    /// <returns>A WindowMode struct containing width, height, and current flags.</returns>
    public static WindowMode GetMode()
    {
      if (window == nint.Zero)
      {
        return new WindowMode
        {
          Width = 0,
          Height = 0,
          PixelWidth = 0,
          PixelHeight = 0,
          Fullscreen = false,
          FullscreenType = currentFullscreenType,
          Borderless = false,
          Resizable = false,
          HighDpi = false,
          MinWidth = 0,
          MinHeight = 0,
          MaxWidth = 0,
          MaxHeight = 0,
          X = 0,
          Y = 0,
          Title = string.Empty,
          Vsync = 0,
          Msaa = 0,
          Centered = false,
          Display = 0,
          RefreshRate = 0,
        };
      }

      _ = SDL.GetWindowSize(window, out int w, out int h);
      _ = SDL.GetWindowSizeInPixels(window, out int pw, out int ph);
      var flags = SDL.GetWindowFlags(window);
      var (isFullscreen, fsType) = GetFullscreen();

      _ = SDL.GetWindowMinimumSize(window, out int minW, out int minH);
      _ = SDL.GetWindowMaximumSize(window, out int maxW, out int maxH);
      _ = SDL.GetWindowPosition(window, out int x, out int y);
      string title = SDL.GetWindowTitle(window) ?? string.Empty;

      int vsyncState = 0;
      if (renderer != nint.Zero)
      {
        if (SDL.GetRenderVSync(renderer, out int vsyncEnabledValue))
        {
          vsyncState = vsyncEnabledValue;
        }
      }

      uint currentDisplayID = SDL.GetDisplayForWindow(window);
      bool isCentered = false;
      if (currentDisplayID != 0)
      {
        isCentered = x == (int)(SDL.WindowposCenteredMask | currentDisplayID) && y == (int)(SDL.WindowposCenteredMask | currentDisplayID);
      }

      return new WindowMode
      {
        Width = w,
        Height = h,
        PixelWidth = pw,
        PixelHeight = ph,
        Fullscreen = isFullscreen,
        FullscreenType = fsType,
        Borderless = (flags & SDL.WindowFlags.Borderless) != 0,
        Resizable = (flags & SDL.WindowFlags.Resizable) != 0,
        HighDpi = (flags & SDL.WindowFlags.HighPixelDensity) != 0,
        MinWidth = minW,
        MinHeight = minH,
        MaxWidth = maxW,
        MaxHeight = maxH,
        X = x,
        Y = y,
        Title = title,
        Vsync = vsyncState,
        Msaa = 0,
        Centered = isCentered,
        Display = (int)currentDisplayID,
        RefreshRate = (int)(SDL.GetCurrentDisplayMode(currentDisplayID)?.RefreshRate ?? 0),
      };
    }
  }
}
