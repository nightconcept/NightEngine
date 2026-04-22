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
  public static partial class Window
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

        ResetInternalState();
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
