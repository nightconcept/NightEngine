// <copyright file="SamplePlatformerGame.cs" company="Night Circle">
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Night;

using Night.Log.Sinks;

using SDL3;

namespace SampleGame;

/// <summary>
/// Main game class for the platformer sample.
/// Inherits from <see cref="Night.Game"/> for Night.Engine integration.
/// </summary>
public class SamplePlatformerGame : Night.Game
{
  private Player player;
  private List<Night.Rectangle> platforms;
  private Night.Sprite? platformSprite;

  // private static readonly ILogger Logger = LogManager.GetLogger(nameof(Game)); // Removed
  private Night.Rectangle goalPlatform;
  private bool goalReachedMessageShown = false; // To ensure message prints only once

  // Joystick input state
  private float joystickAxis0Value = 0.0f;
  private Night.JoystickHat joystickHat0Direction = Night.JoystickHat.Centered; // Fully qualified name
  private bool joystickAButtonPressed = false;
  private uint? inputProvidingJoystickId = null; // Store the ID of the joystick providing input (generic)

  // Gamepad specific input state
  private float gamepadLeftXValue = 0.0f;
  private bool gamepadAButtonPressed = false;
  private uint? gamepadProvidingJoystickId = null; // Store the ID of the joystick providing gamepad input

  /// <summary>
  /// Initializes a new instance of the <see cref="SamplePlatformerGame"/> class.
  /// </summary>
  public SamplePlatformerGame()
  {
    this.player = new Player();
    this.platforms = new List<Night.Rectangle>();
  }

  /// <summary>
  /// Loads game assets and initializes game state.
  /// Called once at the start of the game by the Night.Engine.
  /// </summary>
  public override void Load()
  {
    // _ = Window.SetMode(800, 600, SDL.WindowFlags.Resizable);
    // Window.SetTitle("Night Platformer Sample");
    // Window settings will now be driven by config.json (or defaults if not present/configured)
    this.player.Load();

    // Load platform sprite
    string baseDirectory = AppContext.BaseDirectory;
    string platformImageRelativePath = Path.Combine("assets", "images", "pixel_green.png");
    string platformImageFullPath = Path.Combine(baseDirectory, platformImageRelativePath);
    this.platformSprite = Graphics.NewImage(platformImageFullPath);
    if (this.platformSprite == null)
    {
      Console.WriteLine($"SamplePlatformerGame.Load: Failed to load platform sprite at '{platformImageFullPath}'. Platforms will not be drawn.");
    }

    // Initialize platforms (as per docs/epics/epic7-design.md)
    this.platforms.Add(new Night.Rectangle(50, 500, 700, 50));
    this.platforms.Add(new Night.Rectangle(200, 400, 150, 30));
    this.platforms.Add(new Night.Rectangle(450, 300, 100, 30));
    this.goalPlatform = new Night.Rectangle(600, 200, 100, 30);
    this.platforms.Add(this.goalPlatform);

    // Set the window icon (assuming icon is in assets/icon.ico relative to executable)
    // This path will be resolved by Night.Framework if specified in config.json via IconPath.
    // If not in config, or if this call is made after Framework has set from config,
    // this explicit call can override or set it if not in config.
    // For the sample, we'll rely on the config first, but this shows direct API usage.
    // If you want the SampleGame to ALWAYS use a specific icon regardless of config, call it here.
    // For now, we let config drive it. If you want to test direct SetIcon:
    string iconRelativePath = Path.Combine("assets", "icon.ico");
    string iconFullPath = Path.Combine(AppContext.BaseDirectory, iconRelativePath);
    _ = Window.SetIcon(iconFullPath);
    Console.WriteLine($"Attempted to set icon from SamplePlatformerGame.Load. Current icon: {Window.GetIcon()}");
  }

  /// <summary>
  /// Updates the game state.
  /// Called every frame by the Night.Engine.
  /// </summary>
  /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
  public override void Update(double deltaTime)
  {
    // Logger.Debug($"SamplePlatformerGame.Update: deltaTime={deltaTime:F5}");

    // Check if the input-providing joystick is still connected
    float finalHorizontalInput = 0.0f;
    Night.JoystickHat finalHatDirection = Night.JoystickHat.Centered;
    bool finalJumpPressed = false;

    // Prioritize gamepad input if available and from the same joystick
    if (this.gamepadProvidingJoystickId.HasValue)
    {
      Joystick? gamepadJoystick = Night.Joysticks.GetJoystickByInstanceId(this.gamepadProvidingJoystickId.Value);
      if (gamepadJoystick != null && gamepadJoystick.IsConnected() && gamepadJoystick.IsGamepad())
      {
        finalHorizontalInput = this.gamepadLeftXValue;
        finalJumpPressed = this.gamepadAButtonPressed;

        // Gamepad typically doesn't directly map to a single "hat" for player movement in this simple setup,
        // so we might still use raw joystick hat if needed, or ignore for gamepad.
        // For simplicity, if gamepad is active for axis/button, we might ignore raw hat.
        // Or, if player.Update needs hat, we could still get it from raw joystick state.
        // For now, let's assume gamepad axis/button overrides hat for player control.
        finalHatDirection = Night.JoystickHat.Centered; // Or decide how to integrate if player needs it

        // Log polled gamepad state for verification
        Console.WriteLine($"SampleGame.Update: Polled Gamepad ID {gamepadJoystick.GetId()}: LeftX: {gamepadJoystick.GetGamepadAxis(Night.GamepadAxis.LeftX):F4}, A Button: {gamepadJoystick.IsGamepadDown(Night.GamepadButton.A)}");
      }
      else
      {
        // Gamepad-providing joystick disconnected or no longer a gamepad
        this.gamepadLeftXValue = 0.0f;
        this.gamepadAButtonPressed = false;
        this.gamepadProvidingJoystickId = null;
      }
    }

    // If gamepad input wasn't used, or to supplement it (e.g., for hat), check raw joystick input
    if (this.inputProvidingJoystickId.HasValue && (!this.gamepadProvidingJoystickId.HasValue || this.gamepadProvidingJoystickId.Value != this.inputProvidingJoystickId.Value))
    {
      Joystick? rawJoystick = Night.Joysticks.GetJoystickByInstanceId(this.inputProvidingJoystickId.Value);
      if (rawJoystick != null && rawJoystick.IsConnected())
      {
        if (!this.gamepadProvidingJoystickId.HasValue)
        {
          finalHorizontalInput = this.joystickAxis0Value;
          finalJumpPressed = this.joystickAButtonPressed;
        }

        finalHatDirection = this.joystickHat0Direction; // Always take raw hat for now
      }
      else
      {
        // Raw input-providing joystick disconnected
        this.joystickAxis0Value = 0.0f;
        this.joystickHat0Direction = Night.JoystickHat.Centered;
        this.joystickAButtonPressed = false;
        this.inputProvidingJoystickId = null;
      }
    }

    // If both gamepadProvidingJoystickId and inputProvidingJoystickId are null, inputs remain 0/false/Centered.
    this.player.Update(deltaTime, this.platforms, finalHorizontalInput, finalHatDirection, finalJumpPressed);

    // Check if player reached the goal platform
    // Adjust playerBounds slightly for the goal check to ensure "touching" counts,
    // as player might be perfectly aligned on top.
    Night.Rectangle playerBoundsForGoalCheck = new Night.Rectangle((int)this.player.X, (int)this.player.Y, this.player.Width, this.player.Height + 1);
    if (CheckAABBCollision(playerBoundsForGoalCheck, this.goalPlatform) && !this.goalReachedMessageShown)
    {
      // Simple win condition: print a message.
      // A real game might change state, show a UI, etc.
      Console.WriteLine("Congratulations! Goal Reached!");
      this.goalReachedMessageShown = true; // Set flag so it doesn't print again

      // Optionally, could close the game or trigger another action:
      // Window.Close(); // Window class will be in Night.Framework
    }
  }

  /// <summary>
  /// Draws the game scene.
  /// Called every frame by the Night.Engine after Update.
  /// </summary>
  public override void Draw()
  {
    // Logger.Debug("SamplePlatformerGame.Draw START");
    Graphics.Clear(new Night.Color(135, 206, 235)); // Sky blue background

    // Draw platforms
    if (this.platformSprite != null)
    {
      foreach (var platform in this.platforms)
      {
        // Scale the 1x1 pixel sprite to the platform's dimensions
        Graphics.Draw(
            this.platformSprite,
            platform.X,
            platform.Y,
            0,
            platform.Width,
            platform.Height);
      }
    }

    this.player.Draw();

    // --- Graphics Shape Drawing Demonstration (Top-Left Corner) ---
    // All coordinates and sizes are adjusted to fit in a smaller area.
    // Base offset for the demo shapes
    int demoXOffset = 10;
    int demoYOffset = 10;
    int shapeSize = 20; // General size for smaller shapes
    int spacing = 5;    // Spacing between shapes

    // Rectangle Demo
    Graphics.SetColor(Night.Color.Red);
    Graphics.Rectangle(Night.DrawMode.Fill, demoXOffset, demoYOffset, shapeSize, shapeSize / 2); // Smaller Red Rectangle
    Graphics.SetColor(Night.Color.Black);
    Graphics.Rectangle(Night.DrawMode.Line, demoXOffset, demoYOffset, shapeSize, shapeSize / 2);

    demoXOffset += shapeSize + spacing; // Move right for next shape

    Graphics.SetColor(0, 0, 255, 128); // Semi-transparent Blue
    Graphics.Rectangle(Night.DrawMode.Line, demoXOffset, demoYOffset, shapeSize - 5, shapeSize + 5); // Adjusted Blue Rectangle

    demoXOffset += (shapeSize - 5) + spacing; // Move right

    // Circle Demo
    Graphics.SetColor(Night.Color.Green);
    Graphics.Circle(Night.DrawMode.Fill, demoXOffset + (shapeSize / 2), demoYOffset + (shapeSize / 2), shapeSize / 2); // Smaller Green Circle
    Graphics.SetColor(Night.Color.Black);
    Graphics.Circle(Night.DrawMode.Line, demoXOffset + (shapeSize / 2), demoYOffset + (shapeSize / 2), shapeSize / 2, 12); // 12 segments

    demoXOffset += shapeSize + spacing; // Move right

    Graphics.SetColor(Night.Color.Yellow);
    Graphics.Circle(Night.DrawMode.Line, demoXOffset + (shapeSize / 3), demoYOffset + (shapeSize / 3), shapeSize / 3, 6); // Smaller Hexagon

    // Reset X offset for a new "row" of shapes if needed, or continue right
    // For this demo, we'll just continue right and assume enough horizontal space for this small demo.
    // If more shapes were added, a new row would be demoYOffset += shapeSize + spacing; demoXOffset = 10;
    demoXOffset += (shapeSize / 3 * 2) + spacing; // Move right based on hexagon diameter

    // Line Demo
    Graphics.SetColor(Night.Color.Magenta);
    Graphics.Line(demoXOffset, demoYOffset, demoXOffset + shapeSize, demoYOffset + (shapeSize / 2)); // Smaller Magenta Line

    demoXOffset += shapeSize + spacing;

    Night.PointF[] linePoints = new Night.PointF[]
    {
      new Night.PointF(demoXOffset, demoYOffset),
      new Night.PointF(demoXOffset + (shapeSize / 3), demoYOffset + (shapeSize / 2)),
      new Night.PointF(demoXOffset + (shapeSize * 2 / 3), demoYOffset),
      new Night.PointF(demoXOffset + shapeSize, demoYOffset + (shapeSize / 2)),
    };
    Graphics.SetColor(Night.Color.Cyan);
    Graphics.Line(linePoints); // Smaller Polyline in Cyan

    demoXOffset += shapeSize + spacing;

    // Polygon Demo
    Night.PointF[] triangleVertices = new Night.PointF[]
    {
      new Night.PointF(demoXOffset + (shapeSize / 2), demoYOffset),
      new Night.PointF(demoXOffset + shapeSize, demoYOffset + shapeSize),
      new Night.PointF(demoXOffset, demoYOffset + shapeSize),
    };
    Graphics.SetColor(new Night.Color(255, 165, 0)); // Orange
    Graphics.Polygon(Night.DrawMode.Fill, triangleVertices); // Smaller Orange Triangle
    Graphics.SetColor(Night.Color.Black);
    Graphics.Polygon(Night.DrawMode.Line, triangleVertices);

    demoXOffset += shapeSize + spacing;

    Night.PointF[] pentagonVertices = new Night.PointF[]
    {
        new Night.PointF(demoXOffset + (shapeSize / 2), demoYOffset),
        new Night.PointF(demoXOffset + shapeSize, demoYOffset + (shapeSize / 3)),
        new Night.PointF(demoXOffset + (shapeSize * 2 / 3), demoYOffset + shapeSize),
        new Night.PointF(demoXOffset + (shapeSize / 3), demoYOffset + shapeSize),
        new Night.PointF(demoXOffset, demoYOffset + (shapeSize / 3)),
    };
    Graphics.SetColor(new Night.Color(75, 0, 130)); // Indigo
    Graphics.Polygon(Night.DrawMode.Line, pentagonVertices); // Smaller Pentagon

    // --- Test Large Filled Rectangle ---
    Graphics.SetColor(Night.Color.Blue);
    Graphics.Rectangle(Night.DrawMode.Fill, 300, 200, 200, 150); // Large Blue Filled Rectangle Test

    // --- End Test Large Filled Rectangle ---
  }

  /// <summary>
  /// Handles key press events.
  /// Called by Night.Engine when a key is pressed.
  /// </summary>
  /// <param name="key">The <see cref="Night.KeySymbol"/> of the pressed key.</param>
  /// <param name="scancode">The <see cref="Night.KeyCode"/> (physical key code) of the pressed key.</param>
  /// <param name="isRepeat">True if this is a repeat key event, false otherwise.</param>
  public override void KeyPressed(Night.KeySymbol key, Night.KeyCode scancode, bool isRepeat)
  {
    // Minimal key handling for now, primarily for closing the window.
    if (key == Night.KeySymbol.Escape)
    {
      Window.Close();
    }

    // Test error triggering
    if (key == Night.KeySymbol.E && !isRepeat)
    {
      throw new InvalidOperationException("Test error triggered by pressing 'E' in SamplePlatformerGame!");
    }

    // --- Night.Window Demo: Toggle Fullscreen ---
    if (key == Night.KeySymbol.F11)
    {
      var (isFullscreen, _) = Window.GetFullscreen();
      bool success = Window.SetFullscreen(!isFullscreen, Night.FullscreenType.Desktop);
      Console.WriteLine($"SetFullscreen to {!isFullscreen} (Desktop) attempt: {(success ? "Success" : "Failed")}");
      var newMode = Window.GetMode();
      Console.WriteLine($"New Window Mode: {newMode.Width}x{newMode.Height}, Fullscreen: {newMode.Fullscreen}, Type: {newMode.FullscreenType}, Borderless: {newMode.Borderless}");
    }

    if (key == Night.KeySymbol.F10)
    {
      var (isFullscreen, _) = Window.GetFullscreen();
      bool success = Window.SetFullscreen(!isFullscreen, Night.FullscreenType.Exclusive);
      Console.WriteLine($"SetFullscreen to {!isFullscreen} (Exclusive) attempt: {(success ? "Success" : "Failed")}");
      var newMode = Window.GetMode();
      Console.WriteLine($"New Window Mode: {newMode.Width}x{newMode.Height}, Fullscreen: {newMode.Fullscreen}, Type: {newMode.FullscreenType}, Borderless: {newMode.Borderless}");
    }
  }

  // KeyReleased, MousePressed, and MouseReleased are inherited from Night.Game (default empty implementations)
  // and do not need to be overridden here if no specific action is required.

  /// <summary>
  /// Called when a joystick is connected.
  /// </summary>
  /// <param name="joystick">The joystick that was connected.</param>
  public override void JoystickAdded(Joystick joystick)
  {
    Console.WriteLine($"SampleGame: Joystick Added! ID: {joystick.GetId()}, Name: '{joystick.GetName()}'");
    Console.WriteLine($"SampleGame: Total Joysticks: {Night.Joysticks.GetJoystickCount()}");
    var joysticks = Night.Joysticks.GetJoysticks();
    Console.WriteLine($"SampleGame: Night.Joysticks.GetJoysticks().Count: {joysticks.Count}");
    foreach (var j in joysticks)
    {
      Console.WriteLine($"  - Joystick ID: {j.GetId()}, Name: '{j.GetName()}', Connected: {j.IsConnected()}");
    }
  }

  /// <summary>
  /// Called when a joystick is disconnected.
  /// </summary>
  /// <param name="joystick">The joystick that was disconnected.</param>
  public override void JoystickRemoved(Joystick joystick)
  {
    // Note: joystick.IsConnected() will likely be false here as Joysticks.RemoveJoystick sets it.
    Console.WriteLine($"SampleGame: Joystick Removed! ID: {joystick.GetId()}, Name: '{joystick.GetName()}', WasConnected: {joystick.IsConnected()}");
    if (this.inputProvidingJoystickId.HasValue && this.inputProvidingJoystickId.Value == joystick.GetId())
    {
      // The joystick that was providing input has been removed, reset stored values.
      this.joystickAxis0Value = 0.0f;
      this.joystickHat0Direction = Night.JoystickHat.Centered; // Fully qualified name
      this.joystickAButtonPressed = false;
      this.inputProvidingJoystickId = null;
      Console.WriteLine($"SampleGame: Raw input-providing joystick (ID: {joystick.GetId()}) was removed. Resetting its input state.");
    }

    if (this.gamepadProvidingJoystickId.HasValue && this.gamepadProvidingJoystickId.Value == joystick.GetId())
    {
      this.gamepadLeftXValue = 0.0f;
      this.gamepadAButtonPressed = false;
      this.gamepadProvidingJoystickId = null;
      Console.WriteLine($"SampleGame: Gamepad input-providing joystick (ID: {joystick.GetId()}) was removed. Resetting its input state.");
    }

    Console.WriteLine($"SampleGame: Total Joysticks after removal: {Night.Joysticks.GetJoystickCount()}");
    var joysticks = Night.Joysticks.GetJoysticks();
    Console.WriteLine($"SampleGame: Night.Joysticks.GetJoysticks().Count after removal: {joysticks.Count}");
    foreach (var j in joysticks)
    {
      Console.WriteLine($"  - Remaining Joystick ID: {j.GetId()}, Name: '{j.GetName()}', Connected: {j.IsConnected()}");
    }
  }

  /// <summary>
  /// Called when a joystick axis moves.
  /// </summary>
  /// <param name="joystick">The joystick whose axis moved.</param>
  /// <param name="axis">The index of the axis.</param>
  /// <param name="value">The new value of the axis (-1.0 to 1.0).</param>
  public override void JoystickAxis(Joystick joystick, int axis, float value)
  {
    Console.WriteLine($"SampleGame: Joystick Axis! ID: {joystick.GetId()}, Axis: {axis}, Value: {value:F4}");

    // Typically left stick X-axis
    if (axis == 0)
    {
      this.joystickAxis0Value = value;
      this.inputProvidingJoystickId = (uint)joystick.GetId(); // Record which joystick is providing this input, cast to uint
    }
  }

  /// <summary>
  /// Called when a joystick button is pressed.
  /// </summary>
  /// <param name="joystick">The joystick whose button was pressed.</param>
  /// <param name="button">The index of the button.</param>
  public override void JoystickPressed(Joystick joystick, int button)
  {
    Console.WriteLine($"SampleGame: Joystick Pressed! ID: {joystick.GetId()}, Button: {button}");

    // Assuming 'A' button (South) corresponds to raw button index 0 for many controllers,
    // or if we had a mapping to Night.GamepadButton.A, we'd check that.
    // For raw joystick, we'll assume button 0 is a common primary action button.
    // This part will be more robust in Phase 4 with GamepadPressed.
    // Assuming raw button 0 is 'A'/South for testing P3
    if (button == 0)
    {
      this.joystickAButtonPressed = true;
      this.inputProvidingJoystickId = joystick.GetId();
    }
  }

  /// <summary>
  /// Called when a joystick button is released.
  /// </summary>
  /// <param name="joystick">The joystick whose button was released.</param>
  /// <param name="button">The index of the button.</param>
  public override void JoystickReleased(Joystick joystick, int button)
  {
    Console.WriteLine($"SampleGame: Joystick Released! ID: {joystick.GetId()}, Button: {button}");

    // Assuming raw button 0 is 'A'/South
    if (button == 0)
    {
      this.joystickAButtonPressed = false;

      // We don't reset _inputProvidingJoystickId here, as other inputs might still be active from this joystick.
      // It will be reset if the joystick is disconnected or if another joystick provides input.
    }
  }

  /// <summary>
  /// Called when a joystick hat direction changes.
  /// </summary>
  /// <param name="joystick">The joystick whose hat changed.</param>
  /// <param name="hat">The index of the hat.</param>
  /// <param name="direction">The new direction of the hat.</param>
  public override void JoystickHat(Joystick joystick, int hat, JoystickHat direction)
  {
    Console.WriteLine($"SampleGame: Joystick Hat! ID: {joystick.GetId()}, Hat: {hat}, Direction: {direction}");

    // Typically the first D-Pad/Hat
    if (hat == 0)
    {
      this.joystickHat0Direction = direction;
      this.inputProvidingJoystickId = (uint)joystick.GetId(); // Record which joystick is providing this input, cast to uint
    }
  }

  /// <summary>
  /// Called when a virtual gamepad axis is moved.
  /// </summary>
  /// <param name="joystick">The joystick whose virtual gamepad axis moved.</param>
  /// <param name="axis">The virtual gamepad axis.</param>
  /// <param name="value">The new value of the virtual gamepad axis (-1.0 to 1.0).</param>
  public override void GamepadAxis(Joystick joystick, Night.GamepadAxis axis, float value)
  {
    Console.WriteLine($"SampleGame: Gamepad Axis! ID: {joystick.GetId()}, Axis: {axis}, Value: {value:F4}");
    if (axis == Night.GamepadAxis.LeftX)
    {
      this.gamepadLeftXValue = value;
      this.gamepadProvidingJoystickId = joystick.GetId();
    }
  }

  /// <summary>
  /// Called when a virtual gamepad button is pressed.
  /// </summary>
  /// <param name="joystick">The joystick whose virtual gamepad button was pressed.</param>
  /// <param name="button">The virtual gamepad button.</param>
  public override void GamepadPressed(Joystick joystick, Night.GamepadButton button)
  {
    Console.WriteLine($"SampleGame: Gamepad Pressed! ID: {joystick.GetId()}, Button: {button}");
    if (button == Night.GamepadButton.A || button == Night.GamepadButton.South)
    {
      this.gamepadAButtonPressed = true;
      this.gamepadProvidingJoystickId = joystick.GetId();
    }
  }

  /// <summary>
  /// Called when a virtual gamepad button is released.
  /// </summary>
  /// <param name="joystick">The joystick whose virtual gamepad button was released.</param>
  /// <param name="button">The virtual gamepad button.</param>
  public override void GamepadReleased(Joystick joystick, Night.GamepadButton button)
  {
    Console.WriteLine($"SampleGame: Gamepad Released! ID: {joystick.GetId()}, Button: {button}");
    if (button == Night.GamepadButton.A || button == Night.GamepadButton.South)
    {
      this.gamepadAButtonPressed = false;

      // Do not reset gamepadProvidingJoystickId here, other gamepad inputs might be active.
    }
  }

  // Helper for collision detection (AABB)
  private static bool CheckAABBCollision(Night.Rectangle rect1, Night.Rectangle rect2)
  {
    // True if the rectangles are overlapping
    return rect1.X < rect2.X + rect2.Width &&
           rect1.X + rect1.Width > rect2.X &&
           rect1.Y < rect2.Y + rect2.Height &&
           rect1.Y + rect1.Height > rect2.Y;
  }
}
