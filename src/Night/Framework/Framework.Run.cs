// <copyright file="Framework.Run.cs" company="Night Circle">
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
    private const int MaxDeltaHistorySamples = 60; // Store up to 1 second of deltas at 60fps

    private static readonly object SdlLock = new object(); // Thread synchronization for SDL operations
    private static readonly ILogger Logger = LogManager.GetLogger("Framework");
    private static bool isSdlInitialized = false; // Tracks if SDL is currently active globally
    private static SDL.InitFlags initializedSubsystemsFlags = 0;

    // Delegate to hold a reference to the log output function to prevent garbage collection.
    private static SDL.LogOutputFunction? sdlLogOutputFunction;

    private static int frameCount = 0;
    private static double fpsTimeAccumulator = 0.0;
    private static List<double> deltaHistory = new List<double>();

    private static bool inErrorState = false;

    /// <summary>
    /// Gets a value indicating whether a flag indicating whether the core SDL systems, particularly for input,
    /// have been successfully initialized by this Framework's Run method.
    /// </summary>
    public static bool IsInputInitialized { get; internal set; } = false;

    /// <summary>
    /// Runs the game instance.
    /// The game loop will internally call Load, Update, and Draw methods
    /// on the provided game logic.
    /// This method will initialize and shut down required SDL subsystems.
    /// </summary>
    /// <param name="game">The game interface to run. Must implement <see cref="Night.IGame"/>.</param>
    /// <param name="cliArgs">The parsed command-line arguments. Optional; if null, default settings are used.</param>
    public static void Run(IGame game, CLI? cliArgs = null)
    {
      if (game == null)
      {
        Logger.Error("gameLogic cannot be null.");
        return;
      }

      cliArgs?.ApplySettings();

      inErrorState = false;
      IsInputInitialized = false;

      ConfigurationManager.LoadConfig();
      var windowConfig = ConfigurationManager.CurrentConfig.Window;

      if (cliArgs == null || !cliArgs.IsSilentMode)
      {
        string nightVersionString = VersionInfo.GetVersion();
        string sdlVersionString = NightSDL.GetVersion();
        Console.WriteLine($"Night Engine: v{nightVersionString}");
        Console.WriteLine($"SDL: v{sdlVersionString}");
        Console.WriteLine(GetFormattedPlatformString());
        Console.WriteLine($"Framework: {RuntimeInformation.FrameworkDescription}");
      }

      bool sdlSuccessfullyInitializedThisRun = false;

      try
      {
        var videoDriver = Environment.GetEnvironmentVariable("SDL_VIDEODRIVER");
        bool isHeadlessEnv = string.Equals(videoDriver, "dummy", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(videoDriver, "offscreen", StringComparison.OrdinalIgnoreCase);

        if (isHeadlessEnv)
        {
          // Configure for a headless run.
          Logger.Info($"Headless mode explicitly requested via SDL_VIDEODRIVER='{videoDriver}'.");
          sdlLogOutputFunction = (userdata, category, priority, message) => { };
          SDL.SetLogOutputFunction(sdlLogOutputFunction, nint.Zero);
          _ = SDL.SetHint(SDL.Hints.VideoDriver, videoDriver!);
          _ = SDL.SetHint(SDL.Hints.RenderDriver, "software");
          LogManager.MinLevel = LogLevel.Debug;
        }
        else
        {
          // We are in a headed mode (either by default, or forced)
          Logger.Info("Headed mode detected. Using default drivers.");
        }

        lock (SdlLock)
        {
          if (!isSdlInitialized)
          {
            Logger.Debug("Global isSdlInitialized is false. Attempting SDL.Init().");
            initializedSubsystemsFlags = SDL.InitFlags.Video | SDL.InitFlags.Events | SDL.InitFlags.Joystick | SDL.InitFlags.Gamepad;
            if (!SDL.Init(initializedSubsystemsFlags))
            {
              string sdlError = SDL.GetError();
              Logger.Error($"SDL_Init failed: {sdlError}");

              // Special handling for macOS headed mode when video init fails
              if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !isHeadlessEnv)
              {
                Logger.Info("macOS headed mode video init failed. Attempting workarounds...");

                // Clean up and try with different configuration
                SDL.Quit();

                // Try removing all hints and letting SDL auto-detect
                _ = SDL.SetHint(SDL.Hints.VideoDriver, string.Empty);
                _ = SDL.SetHint(SDL.Hints.MacBackgroundApp, "0");

                Logger.Debug("Retrying SDL.Init() with auto-detection for macOS headed mode.");
                if (!SDL.Init(initializedSubsystemsFlags))
                {
                  string secondError = SDL.GetError();
                  Logger.Error($"SDL_Init auto-detection also failed: {secondError}");
                  Logger.Error("macOS manual testing requires:");
                  Logger.Error("1. Screen Recording permission for your terminal/IDE in System Preferences");
                  Logger.Error("2. Running from a GUI application with proper entitlements");
                  Logger.Error("3. Consider running: SDL_VIDEODRIVER=dummy mise man-test (for headless testing)");
                  return;
                }

                Logger.Info("SDL.Init() successful with auto-detection on macOS.");
              }

              // Only fallback when user explicitly requested headless mode but it failed
              else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                  isHeadlessEnv &&
                  sdlError.Contains("No available video device"))
              {
                Logger.Info("macOS headless mode failed. Retrying with explicit dummy driver configuration.");

                // Clean up and retry with more explicit dummy driver setup
                SDL.Quit();
                _ = SDL.SetHint(SDL.Hints.VideoDriver, "dummy");
                _ = SDL.SetHint(SDL.Hints.RenderDriver, "software");

                Logger.Debug("Retrying SDL.Init() with explicit dummy driver configuration for macOS.");
                if (!SDL.Init(initializedSubsystemsFlags))
                {
                  Logger.Error($"SDL_Init explicit dummy fallback also failed: {SDL.GetError()}");
                  return;
                }

                Logger.Info("SDL.Init() successful with explicit dummy driver on macOS.");
              }
              else
              {
                return;
              }
            }
            else
            {
              Logger.Info("SDL.Init() successful.");
            }

            // Now that SDL is initialized, we can check available drivers
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !isHeadlessEnv)
            {
              try
              {
                int numDrivers = SDL.GetNumVideoDrivers();
                Logger.Debug($"Available video drivers: {numDrivers}");
                for (int i = 0; i < numDrivers; i++)
                {
                  string driver = SDL.GetVideoDriver(i);
                  Logger.Debug($"  Driver {i}: {driver}");
                }

                string? currentDriver = SDL.GetCurrentVideoDriver();
                Logger.Info($"Successfully initialized with video driver: {currentDriver ?? "unknown"}");
              }
              catch (Exception ex)
              {
                Logger.Warn($"Could not enumerate video drivers: {ex.Message}");
              }
            }

            isSdlInitialized = true;
            sdlSuccessfullyInitializedThisRun = true;
          }
          else
          {
            Logger.Debug("Global isSdlInitialized is true. Skipping SDL.Init().");
          }
        }

        IsInputInitialized = (initializedSubsystemsFlags & SDL.InitFlags.Events) == SDL.InitFlags.Events;
        Logger.Debug($"IsInputInitialized set to {IsInputInitialized}.");

        SDL.WindowFlags sdlFlags = 0;
        if (windowConfig.Resizable)
        {
          sdlFlags |= SDL.WindowFlags.Resizable;
        }

        if (windowConfig.Borderless)
        {
          sdlFlags |= SDL.WindowFlags.Borderless;
        }

        if (windowConfig.HighDPI)
        {
          sdlFlags |= SDL.WindowFlags.HighPixelDensity;
        }

        Logger.Debug($"Calling Window.SetMode with Width={windowConfig.Width}, Height={windowConfig.Height}, Flags={sdlFlags}");

        bool modeSet = Window.SetMode(windowConfig.Width, windowConfig.Height, sdlFlags);
        Logger.Debug($"Window.SetMode returned {modeSet}.");

        if (!modeSet)
        {
          Logger.Error($"Window.SetMode returned false. Window.Handle: {Window.Handle}, Window.IsOpen(): {Window.IsOpen()}. SDL Error: {SDL.GetError()}");
          return;
        }

        Window.SetTitle(windowConfig.Title ?? "Night Game");
        Logger.Info($"Window title set to '{Window.GetMode().Title}'. IsOpen: {Window.IsOpen()}");

        if (!Window.IsOpen())
        {
          // This condition implies modeSet was true, but IsOpen is now false.
          Logger.Warn($"Window.IsOpen() is false AFTER modeSet was true and title was set. Window.Handle: {Window.Handle}. SDL Error: {SDL.GetError()}");
        }
        else
        {
          Logger.Debug($"Window.IsOpen() is true after SetMode and SetTitle. Window.Handle: {Window.Handle}");
        }

        if (windowConfig.Fullscreen)
        {
          Logger.Debug($"Attempting to set fullscreen: {windowConfig.FullscreenType}.");
          FullscreenType fsType = windowConfig.FullscreenType.ToLowerInvariant() == "exclusive"
                                    ? FullscreenType.Exclusive
                                    : FullscreenType.Desktop;
          if (!Window.SetFullscreen(true, fsType))
          {
            Logger.Warn($"Failed to set initial fullscreen mode from configuration: {SDL.GetError()}");
          }
          else
          {
            Logger.Debug($"SetFullscreen successful.");
          }
        }

        if (Window.RendererPtr != nint.Zero)
        {
          Logger.Debug($"Attempting to set VSync: {windowConfig.VSync}.");
          if (!SDL.SetRenderVSync(Window.RendererPtr, windowConfig.VSync ? 1 : 0))
          {
            Logger.Warn($"Failed to set initial VSync mode from configuration: {SDL.GetError()}");
          }
          else
          {
            Logger.Debug($"SetRenderVSync successful.");
          }
        }

        if (windowConfig.X.HasValue && windowConfig.Y.HasValue && Window.Handle != nint.Zero)
        {
          Logger.Debug($"Setting window position to X={windowConfig.X.Value}, Y={windowConfig.Y.Value}.");
          _ = SDL.SetWindowPosition(Window.Handle, windowConfig.X.Value, windowConfig.Y.Value);
        }

        if (!string.IsNullOrEmpty(windowConfig.IconPath) && Window.Handle != nint.Zero)
        {
          string iconFullPath = windowConfig.IconPath;
          if (!Path.IsPathRooted(iconFullPath))
          {
            iconFullPath = Path.Combine(AppContext.BaseDirectory, iconFullPath);
          }

          Logger.Debug($"Setting window icon from '{iconFullPath}'.");
          if (!Window.SetIcon(iconFullPath))
          {
            Logger.Warn($"Failed to set window icon from configuration: '{iconFullPath}'. Check path and image format.");
          }
          else
          {
            Logger.Debug($"Window icon set successfully.");
          }
        }

        Logger.Info($"Proceeding to game.Load(). Window.IsOpen(): {Window.IsOpen()}, Window.Handle: {Window.Handle}");
        try
        {
          game.Load();
          Logger.Info($"game.Load() completed. Window.IsOpen(): {Window.IsOpen()}, Window.Handle: {Window.Handle}");
          if (!Window.IsOpen())
          {
            Logger.Error($"Window is NOT open after game.Load() completed. SDL Error: {SDL.GetError()}");
          }
        }
        catch (Exception e)
        {
          Logger.Error($"Exception during game.Load(): {e.Message}", e);
          HandleGameException(e, game);
          if (inErrorState)
          {
            return;
          }
        }

        if (!Window.IsOpen())
        {
          Logger.Fatal($"CRITICAL CHECK - Window is not open after game.Load() for {game.GetType().FullName}. Exiting game loop early. SDL Error if relevant: {SDL.GetError()}");
          return;
        }

        Logger.Info($"Starting main loop. Window.IsOpen(): {Window.IsOpen()}");
        Night.Timer.Initialize();
        frameCount = 0;
        fpsTimeAccumulator = 0.0;
        deltaHistory.Clear();
        var loopCount = 0;

        while (Window.IsOpen() && !inErrorState)
        {
          loopCount++;
          double deltaTime = Night.Timer.Step();
          frameCount++;
          fpsTimeAccumulator += deltaTime;
          if (fpsTimeAccumulator >= 1.0)
          {
            Night.Timer.CurrentFPS = frameCount;
            frameCount = 0;
            fpsTimeAccumulator -= 1.0;
          }

          deltaHistory.Add(deltaTime);
          if (deltaHistory.Count > MaxDeltaHistorySamples)
          {
            deltaHistory.RemoveAt(0);
          }

          if (deltaHistory.Count > 0)
          {
            Night.Timer.CurrentAverageDelta = deltaHistory.Average();
          }

          ProcessSdlEvents(game); // Call to the new method in Framework.Events.cs

          if (inErrorState)
          {
            break;
          }

          if (!inErrorState)
          {
            try
            {
              Logger.Debug($"Loop {loopCount}: Calling game.Update()");
              game.Update((float)deltaTime);
              Logger.Debug($"Loop {loopCount}: game.Update() returned");
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
              if (inErrorState)
              {
                break;
              }
            }
          }

          if (!inErrorState)
          {
            try
            {
              Logger.Debug($"Loop {loopCount}: Calling game.Draw() and Graphics.Present()");
              game.Draw();
              Night.Graphics.Present();
              Logger.Debug($"Loop {loopCount}: game.Draw() and Graphics.Present() returned");
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
              if (inErrorState)
              {
                break;
              }
            }
          }
        }

        Logger.Info($"Main loop ended. Window.IsOpen(): {Window.IsOpen()}, inErrorState: {inErrorState}, LoopCount: {loopCount}");
      }
      catch (Exception ex)
      {
        Logger.Fatal($"An UNEXPECTED FRAMEWORK error occurred: {ex.ToString()}", ex);
        HandleGameException(ex, null);
      }
      finally
      {
        Logger.Debug($"Entering finally block. sdlSuccessfullyInitializedThisRun: {sdlSuccessfullyInitializedThisRun}, isSdlInitialized (static): {isSdlInitialized}");
        Window.Shutdown();
        Night.Joysticks.ClearJoysticks(); // Clear joystick resources

        lock (SdlLock)
        {
          if (sdlSuccessfullyInitializedThisRun)
          {
            Logger.Info($"SDL was initialized this run. Quitting SDL subsystems and SDL.");
            if (initializedSubsystemsFlags != 0)
            {
              SDL.QuitSubSystem(initializedSubsystemsFlags);
              Logger.Debug($"QuitSubSystem({initializedSubsystemsFlags}) called.");
              initializedSubsystemsFlags = 0;
            }

            SDL.Quit();
            Logger.Info($"SDL.Quit() called.");
            isSdlInitialized = false;
          }
          else
          {
            Logger.Debug($"SDL was not initialized this run or Init failed. Skipping SDL.Quit(). Global isSdlInitialized: {isSdlInitialized}");
          }
        }

        IsInputInitialized = false;
        Logger.Debug($"Exiting finally block. IsInputInitialized: {IsInputInitialized}, isSdlInitialized (static): {isSdlInitialized}");
      }
    }
  }
}
