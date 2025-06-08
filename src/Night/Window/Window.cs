// <copyright file="Window.cs" company="Night Circle">
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
  /// Provides an interface for modifying and retrieving information about the program's window.
  /// </summary>
  public static class Window
  {
    private static readonly ILogger Logger = LogManager.GetLogger("Night.Window.Window");
    private static readonly object WindowLock = new object(); // Thread synchronization for window operations
    private static nint window = nint.Zero;
    private static nint renderer = nint.Zero;

    private static bool isWindowOpen = false;
    private static FullscreenType currentFullscreenType = FullscreenType.Desktop;
    private static ImageData? currentIconData = null;

    /// <summary>
    /// Gets the pointer to the internal SDL renderer. For use by Night.Graphics.
    /// </summary>
    internal static nint RendererPtr => renderer;

    /// <summary>
    /// Gets the handle to the internal SDL window. For use by other Night modules or internal methods.
    /// </summary>
    internal static nint Handle => window;

    /// <summary>
    /// Sets the window icon.
    /// </summary>
    /// <param name="imagePath">The path to the icon image file (e.g., .ico, .png, .bmp).
    /// Uses SDL_image for loading, so supports various formats.</param>
    /// <returns>True if the icon was set successfully, false otherwise.</returns>
    public static bool SetIcon(string imagePath)
    {
      currentIconData = null;

      if (window == nint.Zero)
      {
        Logger.Warn("Window handle is null. Icon not set.");
        return false;
      }

      if (string.IsNullOrEmpty(imagePath))
      {
        Logger.Warn("imagePath is null or empty. Icon not set.");
        return false;
      }

      _ = SDL.ClearError();
      nint loadedSurfacePtr = SDL3.Image.Load(imagePath);
      if (loadedSurfacePtr == nint.Zero)
      {
        string imgError = SDL.GetError();
        Logger.Error($"Failed to load image '{imagePath}' using SDL_image. Error: {imgError}");
        return false;
      }

      SDL.PixelFormat targetFormatEnum = SDL.PixelFormat.RGBA8888;
      nint convertedSurfacePtr = SDL.ConvertSurface(loadedSurfacePtr, targetFormatEnum);

      if (convertedSurfacePtr == nint.Zero)
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Failed to convert surface to target format. SDL Error: {sdlError}");
        SDL.DestroySurface(loadedSurfacePtr);
        return false;
      }

      try
      {
        if (!SDL.SetWindowIcon(window, convertedSurfacePtr))
        {
          string sdlError = SDL.GetError();
          Logger.Error($"SDL_SetWindowIcon failed. SDL Error: {sdlError}");
          return false;
        }

        SDL.Surface convertedSurfaceStruct = Marshal.PtrToStructure<SDL.Surface>(convertedSurfacePtr);
        int width = convertedSurfaceStruct.Width;
        int height = convertedSurfaceStruct.Height;

        IntPtr detailsPtr = SDL.GetPixelFormatDetails(convertedSurfaceStruct.Format);
        if (detailsPtr == IntPtr.Zero)
        {
          string sdlError = SDL.GetError();
          Logger.Error($"Failed to get pixel format details. SDL Error: {sdlError}");
          return false;
        }

        SDL.PixelFormatDetails pixelFormatDetails = Marshal.PtrToStructure<SDL.PixelFormatDetails>(detailsPtr);
        int bytesPerPixel = pixelFormatDetails.BytesPerPixel;

        if (bytesPerPixel != 4)
        {
          Logger.Error($"Converted surface is not 4bpp as expected for RGBA. Actual bpp: {bytesPerPixel}, Format: {convertedSurfaceStruct.Format}");
          return false;
        }

        byte[] pixelData = new byte[width * height * bytesPerPixel];
        Marshal.Copy(convertedSurfaceStruct.Pixels, pixelData, 0, pixelData.Length);

        currentIconData = new ImageData(width, height, pixelData);
        return true;
      }
      catch (Exception e)
      {
        Logger.Error($"Error processing surface or creating ImageData.", e);
        return false;
      }
      finally
      {
        if (convertedSurfacePtr != nint.Zero)
        {
          SDL.DestroySurface(convertedSurfacePtr);
        }

        if (loadedSurfacePtr != nint.Zero)
        {
          SDL.DestroySurface(loadedSurfacePtr);
        }
      }
    }

    /// <summary>
    /// Gets the image data of the currently set window icon.
    /// </summary>
    /// <returns>The <see cref="ImageData"/> of the icon, or null if no icon has been set or an error occurred.</returns>
    public static ImageData? GetIcon()
    {
      return currentIconData;
    }

    /// <summary>
    ///     Sets the display mode and properties of the window.
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
            Logger.Debug($"Old renderer destroyed.");
          }

          SDL.DestroyWindow(window);
          window = nint.Zero;
          isWindowOpen = false;
          Logger.Debug($"Old window destroyed.");
        }

        Logger.Debug($"[PRE-CREATE] Clearing any previous SDL errors");
        _ = SDL.ClearError();
        string preCreateError = SDL.GetError();
        Logger.Debug($"[PRE-CREATE] SDL error after clear: '{preCreateError}'");

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

          Logger.Debug($"SDL.GetWindowSurface succeeded for software fallback.");

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
    /// Sets the window title.
    /// </summary>
    /// <param name="title">The new window title.</param>
    public static void SetTitle(string title)
    {
      if (window == nint.Zero)
      {
        string errorMsg = "Error in Night.Window.SetTitle: Window handle is null. Was SetMode called successfully?";
        throw new InvalidOperationException(errorMsg);
      }

      if (!SDL.SetWindowTitle(window, title))
      {
        string sdlError = SDL.GetError();
        throw new Exception($"SDL Error in Night.Window.SetTitle: {sdlError}");
      }
    }

    /// <summary>
    /// Checks if the window is open.
    /// </summary>
    /// <returns>True if the window is open, false otherwise.</returns>
    public static bool IsOpen()
    {
      // Added more explicit check for debugging
      bool result = isWindowOpen && window != nint.Zero && renderer != nint.Zero;

      return result;
    }

    /// <summary>
    /// Signals that the window should close.
    /// </summary>
    public static void Close()
    {
      Logger.Info($"Window.Close called. Setting isWindowOpen to false. Current window handle: {window}");
      isWindowOpen = false;
    }

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

    /// <summary>
    /// Gets a list of available fullscreen display modes for a given display.
    /// </summary>
    /// <param name="displayIndex">The index of the display (0 for primary).</param>
    /// <returns>A list of (Width, Height) tuples representing available modes, or an empty list on error.</returns>
    public static List<(int Width, int Height)> GetFullscreenModes(int displayIndex = 0)
    {
      var modesList = new List<(int Width, int Height)>();
      var uniqueModes = new HashSet<(int Width, int Height)>();

      uint[]? actualDisplayIDs = SDL.GetDisplays(out int displayCount);
      if (actualDisplayIDs == null || displayCount <= 0 || displayIndex < 0 || displayIndex >= displayCount)
      {
        return modesList;
      }

      uint targetDisplayID = actualDisplayIDs[displayIndex];
      SDL.DisplayMode[]? displayModes = SDL.GetFullscreenDisplayModes(targetDisplayID, out int count);

      if (displayModes == null || count <= 0 || displayModes.Length != count)
      {
        return modesList;
      }

      foreach (var mode in displayModes)
      {
        var currentModeTuple = (mode.W, mode.H);
        if (uniqueModes.Add(currentModeTuple))
        {
          modesList.Add(currentModeTuple);
        }
      }

      return modesList;
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

    /// <summary>
    /// Gets the DPI scaling factor of the display containing the window.
    /// </summary>
    /// <returns>The DPI scaling factor (e.g., 1.0f for 96 DPI, 2.0f for 192 DPI), or 1.0f if unable to determine.</returns>
    public static float GetDPIScale()
    {
      if (window == nint.Zero)
      {
        return 1.0f;
      }

      uint displayID = SDL.GetDisplayForWindow(window);
      if (displayID == 0 && !string.IsNullOrEmpty(SDL.GetError()))
      {
        displayID = SDL.GetPrimaryDisplay();
        if (displayID == 0 && !string.IsNullOrEmpty(SDL.GetError()))
        {
          return 1.0f;
        }
      }

      if (displayID == 0)
      {
        return 1.0f;
      }

      float contentScale = SDL.GetDisplayContentScale(displayID);
      if (contentScale > 0.0f)
      {
        return contentScale;
      }
      else
      {
        _ = SDL.GetWindowSize(window, out int windowWidth, out _);
        _ = SDL.GetWindowSizeInPixels(window, out int pixelWidth, out _);

        if (windowWidth > 0 && pixelWidth > 0 && pixelWidth != windowWidth)
        {
          return (float)pixelWidth / windowWidth;
        }

        return 1.0f;
      }
    }

    /// <summary>
    /// Converts a value from logical units to pixels, using the window's DPI scale.
    /// </summary>
    /// <param name="value">The value in logical units.</param>
    /// <returns>The value in pixels.</returns>
    public static float ToPixels(float value)
    {
      return value * GetDPIScale();
    }

    /// <summary>
    /// Converts a value from pixels to logical units, using the window's DPI scale.
    /// </summary>
    /// <param name="value">The value in pixels.</param>
    /// <returns>The value in logical units.</returns>
    public static float FromPixels(float value)
    {
      float scale = GetDPIScale();
      return scale == 0 ? value : value / scale;
    }

    /// <summary>
    /// Cleans up window and renderer resources.
    /// </summary>
    internal static void Shutdown()
    {
      lock (WindowLock)
      {
        Logger.Info($"Shutdown called. Current window: {window}, renderer: {renderer}");
        if (renderer != nint.Zero)
        {
          SDL.DestroyRenderer(renderer);
          renderer = nint.Zero;
          Logger.Debug("Renderer destroyed.");
        }

        if (window != nint.Zero)
        {
          SDL.DestroyWindow(window);
          window = nint.Zero;
          Logger.Debug("Window destroyed.");
        }

        isWindowOpen = false;
        currentIconData = null;
        Logger.Debug("State reset.");
      }
    }

    /// <summary>
    /// Resets internal state variables of the Window module.
    /// </summary>
    internal static void ResetInternalState()
    {
      Logger.Debug("ResetInternalState called.");
      isWindowOpen = false;
      currentFullscreenType = FullscreenType.Desktop;
      currentIconData = null;
    }

    /// <summary>
    /// Helper to get desktop dimensions for a specific display ID.
    /// </summary>
    /// <param name="displayID">The SDL display ID.</param>
    /// <returns>Tuple of (Width, Height), or (0,0) on error.</returns>
    private static (int Width, int Height) GetDesktopDimensionsForDisplayID(uint displayID)
    {
      if (displayID == 0)
      {
        return (0, 0);
      }

      SDL.DisplayMode? mode = SDL.GetDesktopDisplayMode(displayID);
      if (mode == null)
      {
        Logger.Warn($"GetDesktopDimensionsForDisplayID: Failed to get desktop display mode for display {displayID}. SDL Error: {SDL.GetError()}");
        return (0, 0);
      }

      return (mode.Value.W, mode.Value.H);
    }
  }
}
