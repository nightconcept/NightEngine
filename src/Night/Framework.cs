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
  public static class Framework
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

      // Apply settings from command line arguments first, if provided.
      // This will handle LogManager setup, session logs, etc., based on CLI flags.
      cliArgs?.ApplySettings();

      inErrorState = false;
      IsInputInitialized = false;

      ConfigurationManager.LoadConfig();
      var windowConfig = ConfigurationManager.CurrentConfig.Window;

      bool isTestingEnvironment = IsTestingEnvironment();

      if ((cliArgs == null || !cliArgs.IsSilentMode) && !isTestingEnvironment)
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
        // Check if running in a testing environment (e.g., CI/CD, headless environments)
        // isTestingEnvironment is already determined above
        if (isTestingEnvironment)
        {
          // Redirect SDL logging to a null sink to prevent "Vulkan..." messages on CI
          sdlLogOutputFunction = (userdata, category, priority, message) => { };
          SDL.SetLogOutputFunction(sdlLogOutputFunction, nint.Zero);

          Logger.Info("Testing environment detected. Setting LogManager.MinLevel to Debug for diagnostics.");
          LogManager.MinLevel = LogLevel.Debug; // Verbose log output for CI tests
          Logger.Info("Testing environment detected. Setting SDL video driver to 'offscreen'.");

          // Use the "offscreen" video driver for headless testing environments
          // to ensure a valid rendering context without a visible window.
          _ = SDL.SetHint(SDL.Hints.VideoDriver, "offscreen");

          // Force software rendering unless hardware is explicitly requested for debugging.
          if (cliArgs == null || !cliArgs.ForceHardwareRender)
          {
            Logger.Info("Testing environment detected. Setting SDL render driver to 'software'.");

            // Force software rendering to avoid OpenGL/GLES initialization issues in headless environments
            _ = SDL.SetHint(SDL.Hints.RenderDriver, "software");
          }
          else
          {
            Logger.Info("Testing environment detected, but --force-graphics was specified. NOT forcing software rendering.");
          }
        }

        lock (SdlLock)
        {
          if (!isSdlInitialized)
          {
            Logger.Debug("Global isSdlInitialized is false. Attempting SDL.Init().");
            initializedSubsystemsFlags = SDL.InitFlags.Video | SDL.InitFlags.Events;
            if (!SDL.Init(initializedSubsystemsFlags))
            {
              Logger.Error($"SDL_Init failed: {SDL.GetError()}");
              return;
            }

            Logger.Info("SDL.Init() successful.");
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

          while (SDL.PollEvent(out SDL.Event e) && !inErrorState)
          {
            var eventType = (SDL.EventType)e.Type;
            Logger.Debug($"SDL Event polled: {eventType}");
            if (eventType == SDL.EventType.Quit)
            {
              Logger.Info("SDL_QUIT event received. Closing window.");
              Window.Close();
            }
            else if (eventType == SDL.EventType.KeyDown)
            {
              try
              {
                game.KeyPressed((KeySymbol)e.Key.Key, (KeyCode)e.Key.Scancode, e.Key.Repeat);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
            else if (eventType == SDL.EventType.KeyUp)
            {
              try
              {
                game.KeyReleased((KeySymbol)e.Key.Key, (KeyCode)e.Key.Scancode);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
            else if (eventType == SDL.EventType.MouseButtonDown)
            {
              try
              {
                game.MousePressed((int)e.Button.X, (int)e.Button.Y, (MouseButton)e.Button.Button, e.Button.Which == SDL.TouchMouseID, e.Button.Clicks);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
            else if (eventType == SDL.EventType.MouseButtonUp)
            {
              try
              {
                game.MouseReleased((int)e.Button.X, (int)e.Button.Y, (MouseButton)e.Button.Button, e.Button.Which == SDL.TouchMouseID, e.Button.Clicks);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
          }

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

    /// <summary>
    /// Detects if the application is running in a testing environment.
    /// This includes CI/CD systems, automated test runners, or when explicitly configured for testing.
    /// </summary>
    /// <returns>True if running in a testing environment, false otherwise.</returns>
    private static bool IsTestingEnvironment()
    {
      // Check for common CI/CD environment variables
      var ciEnvironmentVars = new[]
      {
        "CI", "CONTINUOUS_INTEGRATION", "GITHUB_ACTIONS", "GITLAB_CI",
        "JENKINS_URL", "BUILDKITE", "CIRCLECI", "TRAVIS", "APPVEYOR",
      };

      foreach (var envVar in ciEnvironmentVars)
      {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar)))
        {
          return true;
        }
      }

      // Check for test runner processes in the call stack or environment
      try
      {
        var processName = Process.GetCurrentProcess().ProcessName;
        if (processName.Contains("dotnet") || processName.Contains("testhost") ||
            processName.Contains("vstest") || processName.Contains("xunit"))
        {
          return true;
        }
      }
      catch
      {
        // Ignore any exceptions during process name detection
      }

      // Check for SDL_VIDEODRIVER environment variable already set to testing modes
      var videoDriver = Environment.GetEnvironmentVariable("SDL_VIDEODRIVER");
      if (!string.IsNullOrEmpty(videoDriver) &&
          (videoDriver.Equals("dummy", StringComparison.OrdinalIgnoreCase) ||
           videoDriver.Equals("offscreen", StringComparison.OrdinalIgnoreCase)))
      {
        return true;
      }

      return false;
    }

    private static string GetFormattedPlatformString()
    {
      string? osName;
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        osName = "Windows";
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        osName = "Linux";
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        osName = "macOS";
      }
      else
      {
        osName = RuntimeInformation.OSDescription;
      }

      return $"Platform: {osName} {RuntimeInformation.OSArchitecture}";
    }
  }
}
