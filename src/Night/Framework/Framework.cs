// <copyright file="Framework.cs" company="Night Circle">
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Night;
using Night.Log;

using SDL3;

namespace Night
{
  /// <summary>
  /// Manages the main game loop and coordination of game states.
  /// Provides the main entry point to run a game.
  /// </summary>
  public static partial class Framework
  {
    /// <summary>
    /// Gets the version of the Night Engine as a string.
    /// </summary>
    /// <returns>The version of the Night Engine as a string.</returns>
    public static string GetVersion()
    {
      return VersionInfo.GetVersion();
    }

    private static void HandleGameException(Exception e, IGame? gameInstance)
    {
      Logger.Error($"HandleGameException: Error: {e.Message}", e);
      inErrorState = true;
      var customHandler = Night.Error.GetHandler();
      if (customHandler != null)
      {
        try
        {
          customHandler(e);
          if (Window.IsOpen())
          {
            Window.Close();
          }
        }
        catch (Exception exHandler)
        {
          Logger.Fatal($"CRITICAL: Exception in custom error handler: {exHandler.ToString()}", exHandler);
          Logger.Error($"Original game error: {e.ToString()}", e);
          if (Window.IsOpen())
          {
            Window.Close();
          }
        }
      }
      else
      {
        DefaultErrorHandler(e, gameInstance);
      }
    }

    private static void DefaultErrorHandler(Exception e, IGame? gameInstance)
    {
      Logger.Error("--- Night Engine: Default Error Handler ---");
      Logger.Error($"An error occurred in the game: {e.GetType().Name}", e);
      Logger.Error($"Message: {e.Message}");
      Logger.Error("Stack Trace:");
      Logger.Error(e.StackTrace ?? "No stack trace available");
      Logger.Error("-------------------------------------------");

      bool canDrawError = false;
      try
      {
        if (!Window.IsOpen() || (Window.RendererPtr == nint.Zero))
        {
          Logger.Warn("(DefaultErrorHandler): Window or Graphics not initialized. Attempting to set mode...");
          if (Window.SetMode(800, 600, SDL.WindowFlags.Resizable))
          {
            Logger.Info("(DefaultErrorHandler): Window mode set to 800x600.");
            canDrawError = Window.RendererPtr != nint.Zero;
          }
          else
          {
            Logger.Error($"(DefaultErrorHandler): Failed to set window mode. SDL Error: {SDL.GetError()}");
          }
        }
        else
        {
          canDrawError = true;
        }

        if (IsInputInitialized)
        {
          Mouse.SetVisible(true);
          Mouse.SetGrabbed(false);
          Mouse.SetRelativeMode(false);
        }
      }
      catch (Exception resetEx)
      {
        Logger.Error($"(DefaultErrorHandler): Exception during state reset: {resetEx.ToString()}", resetEx);
        canDrawError = false;
      }

      if (canDrawError)
      {
        try
        {
          string fullErrorText = $"Error: {e.Message}\n\n{e.StackTrace}";
          Window.SetTitle($"Error - {gameInstance?.GetType().Name ?? "Night Game"}");
          bool runningErrorLoop = true;
          while (runningErrorLoop && Window.IsOpen())
          {
            while (SDL.PollEvent(out SDL.Event ev))
            {
              if (ev.Type == (uint)SDL.EventType.Quit)
              {
                runningErrorLoop = false;
                Window.Close();
                break;
              }

              if (ev.Type == (uint)SDL.EventType.KeyDown)
              {
                if (ev.Key.Key == SDL.Keycode.Escape)
                {
                  runningErrorLoop = false;
                  Window.Close();
                  break;
                }
              }
            }

            if (!runningErrorLoop)
            {
              break;
            }

            _ = SDL.SetRenderDrawColor(Window.RendererPtr, 30, 30, 30, 255);
            _ = SDL.RenderClear(Window.RendererPtr);
            _ = SDL.RenderPresent(Window.RendererPtr);
            SDL.Delay(16);
          }
        }
        catch (Exception drawEx)
        {
          Logger.Error($"(DefaultErrorHandler): Exception during error display loop: {drawEx.ToString()}", drawEx);
        }
      }

      if (Window.IsOpen())
      {
        Window.Close();
      }
    }

    private static string GetFormattedPlatformString()
    {
      string platformSpecificInfo;

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        platformSpecificInfo = "Windows";
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        global::System.Version osVersion = global::System.Environment.OSVersion.Version; // e.g., 15.4.1
        string versionString = osVersion.ToString(3); // Ensures Major.Minor.Patch format

        string? marketingName = GetMacOsMarketingName(osVersion.Major);
        if (!string.IsNullOrEmpty(marketingName))
        {
          platformSpecificInfo = $"macOS {marketingName} {versionString}";
        }
        else
        {
          // Fallback if marketing name not found
          platformSpecificInfo = $"macOS {versionString}";
        }
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        string? distroInfo = GetLinuxDistroInfoInternal();
        string? kernelVersion = GetLinuxKernelVersionInternal();
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(distroInfo))
        {
          parts.Add(distroInfo);
        }
        else
        {
          parts.Add("Linux"); // Fallback if distro info is not available
        }

        if (!string.IsNullOrEmpty(kernelVersion))
        {
          parts.Add($"(Kernel {kernelVersion})");
        }

        platformSpecificInfo = string.Join(" ", parts); // SA1513 for line 229 (closing 'if')
      }
      else
      {
        platformSpecificInfo = RuntimeInformation.OSDescription;
      }

      return $"Platform: {platformSpecificInfo} {RuntimeInformation.OSArchitecture}";
    }

    private static string? GetMacOsMarketingName(int majorVersion)
    {
      // This list should be updated as new macOS versions are released.
      return majorVersion switch
      {
        15 => "Sequoia",
        14 => "Sonoma",
        13 => "Ventura",
        12 => "Monterey",
        11 => "Big Sur",

        // Older versions can be added if necessary
        _ => null, // No specific marketing name known for this major version
      };
    }

    private static string? GetLinuxDistroInfoInternal()
    {
      const string osReleasePath = "/etc/os-release";
      try
      {
        if (File.Exists(osReleasePath))
        {
          var lines = File.ReadAllLines(osReleasePath);
          string? prettyName = null;
          string? name = null;
          string? version = null;

          foreach (var line in lines)
          {
            if (line.StartsWith("PRETTY_NAME=", StringComparison.Ordinal))
            {
              prettyName = line.Substring("PRETTY_NAME=".Length).Trim('"');
              break; // PRETTY_NAME is preferred
            }
            else if (line.StartsWith("NAME=", StringComparison.Ordinal))
            {
              name = line.Substring("NAME=".Length).Trim('"');
            }
            else if (line.StartsWith("VERSION=", StringComparison.Ordinal))
            {
              version = line.Substring("VERSION=".Length).Trim('"');
            }
          }

          if (!string.IsNullOrEmpty(prettyName))
          {
            return prettyName;
          }
          else if (!string.IsNullOrEmpty(name))
          {
            return string.IsNullOrEmpty(version) ? name : $"{name} {version}";
          }

          Logger.Debug($"Could not parse relevant fields (PRETTY_NAME, NAME, VERSION) from '{osReleasePath}'."); // SA1513 for line 302 (closing 'else if')
        }
        else
        {
          Logger.Debug($"Linux distribution information file '{osReleasePath}' not found.");
        }
      }
      catch (IOException ex)
      {
        Logger.Warn($"IO error reading '{osReleasePath}': {ex.Message}");
      }
      catch (UnauthorizedAccessException ex)
      {
        Logger.Warn($"Permission denied reading '{osReleasePath}': {ex.Message}");
      }

      // Catch-all for other unexpected errors // SA1108
      catch (Exception ex)
      {
        Logger.Warn($"Failed to read or parse '{osReleasePath}': {ex.Message}");
      }

      return null;
    }

    private static string? GetLinuxKernelVersionInternal()
    {
      try
      {
        var startInfo = new ProcessStartInfo
        {
          FileName = "uname",
          Arguments = "-r",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
          RedirectStandardError = true, // SA1413: Added trailing comma
        }; // SA1108: Moved comment "// Capture errors as well" from previous line

        using (var process = Process.Start(startInfo))
        {
          if (process == null)
          {
            Logger.Warn("Failed to start 'uname' process (Process.Start returned null).");
            return null;
          }

          string kernelVersion = process.StandardOutput.ReadToEnd().Trim();
          string errorOutput = process.StandardError.ReadToEnd().Trim();
          process.WaitForExit();

          if (process.ExitCode != 0)
          {
            Logger.Warn($"'uname -r' exited with code {process.ExitCode}. Error: '{errorOutput}'.");
            return null;
          }

          if (!string.IsNullOrEmpty(errorOutput) && string.IsNullOrEmpty(kernelVersion))
          {
            Logger.Warn($"'uname -r' produced error output: '{errorOutput}'."); // IDE0055: Fixed indentation (15 -> 12)
          }

          return string.IsNullOrEmpty(kernelVersion) ? null : kernelVersion; // SA1513 for line 361 (closing 'if')
        }
      }

      // Typically "file not found" or permission issues // SA1108
      catch (Win32Exception ex)
      {
        Logger.Warn($"Failed to execute 'uname -r'. Is 'uname' in PATH and executable? Error: {ex.Message}"); // IDE0055: Fixed indentation (10 -> 8)
      }

      // e.g. if StandardOutput/Error not redirected // SA1108
      catch (InvalidOperationException ex)
      {
        Logger.Warn($"Invalid operation while trying to run 'uname -r': {ex.Message}"); // IDE0055: Fixed indentation (10 -> 8)
      }

      // Catch other potential exceptions // SA1108
      catch (Exception ex)
      {
        Logger.Warn($"An unexpected error occurred while executing 'uname -r': {ex.Message}");
      }

      return null;
    }
  }
}
