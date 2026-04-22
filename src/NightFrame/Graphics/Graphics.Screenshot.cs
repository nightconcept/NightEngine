// <copyright file="Graphics.Screenshot.cs" company="Night Circle">
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
using System.IO;
using System.Runtime.InteropServices;

using Night.Log;

using SDL3;

namespace Night
{
  public static partial class Graphics
  {
    private static readonly ILogger ScreenshotLogger = LogManager.GetLogger("Graphics.Screenshot");

    /// <summary>
    /// Captures the current renderer output and writes it to a PPM (P6) file.
    /// The output directory must exist. Caller is responsible for creating it.
    /// </summary>
    /// <param name="path">Destination file path.</param>
    /// <returns><c>true</c> if the screenshot was written successfully.</returns>
    public static bool Screenshot(string path)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        ScreenshotLogger.Error("Screenshot failed: renderer is null. Was Window.SetMode called successfully?");
        return false;
      }

      IntPtr surfacePtr = SDL.RenderReadPixels(rendererPtr, null);
      if (surfacePtr == IntPtr.Zero)
      {
        ScreenshotLogger.Error($"Screenshot failed: SDL.RenderReadPixels returned null. SDL Error: {SDL.GetError()}");
        return false;
      }

      try
      {
        SDL.Surface surface = Marshal.PtrToStructure<SDL.Surface>(surfacePtr);

        // Convert to RGB24 if needed so we always write 3 bytes per pixel.
        IntPtr workSurface = surfacePtr;
        bool converted = false;
        if (surface.Format != SDL.PixelFormat.RGB24)
        {
          IntPtr convertedPtr = SDL.ConvertSurface(surfacePtr, SDL.PixelFormat.RGB24);
          if (convertedPtr == IntPtr.Zero)
          {
            ScreenshotLogger.Error($"Screenshot failed: SDL.ConvertSurface returned null. SDL Error: {SDL.GetError()}");
            return false;
          }

          SDL.DestroySurface(surfacePtr);
          workSurface = convertedPtr;
          surface = Marshal.PtrToStructure<SDL.Surface>(workSurface);
          converted = true;
        }

        try
        {
          WritePpm(path, surface);
          ScreenshotLogger.Info($"Screenshot saved to '{path}' ({surface.Width}x{surface.Height}).");
          return true;
        }
        catch (Exception ex)
        {
          ScreenshotLogger.Error($"Screenshot failed writing PPM to '{path}': {ex.Message}");
          return false;
        }
        finally
        {
          SDL.DestroySurface(workSurface);
          _ = converted; // suppress unused warning if any
        }
      }
      catch (Exception ex)
      {
        ScreenshotLogger.Error($"Screenshot failed during surface processing: {ex.Message}");
        SDL.DestroySurface(surfacePtr);
        return false;
      }
    }

    private static void WritePpm(string path, SDL.Surface surface)
    {
      // PPM P6 format: binary RGB, top-row-first, no padding in output.
      // The SDL surface may have row padding (pitch > width * 3); strip it.
      using global::System.IO.FileStream fs = new global::System.IO.FileStream(path, global::System.IO.FileMode.Create, global::System.IO.FileAccess.Write);
      using global::System.IO.StreamWriter header = new global::System.IO.StreamWriter(fs, leaveOpen: true);
      header.NewLine = "\n";
      header.WriteLine("P6");
      header.WriteLine($"{surface.Width} {surface.Height}");
      header.WriteLine("255");
      header.Flush();

      int rowBytes = surface.Width * 3;
      for (int row = 0; row < surface.Height; row++)
      {
        IntPtr rowPtr = surface.Pixels + (row * surface.Pitch);
        byte[] rowData = new byte[rowBytes];
        Marshal.Copy(rowPtr, rowData, 0, rowBytes);
        fs.Write(rowData, 0, rowBytes);
      }
    }
  }
}
