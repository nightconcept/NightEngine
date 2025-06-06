// <copyright file="Platformer.cs" company="Night Circle">
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

using Night;

using SDL3;

namespace SampleGame;

/// <summary>
/// A sample platformer game implementation using the Night engine.
/// Implements the <see cref="IGame"/> interface for Night.Engine integration.
/// </summary>
public class Platformer : IGame
{
  private Player player;
  private List<Night.Rectangle> platforms;
  private Sprite? platformSprite;
  private Night.Rectangle goalPlatform;
  private bool goalReachedMessageShown = false;

  /// <summary>
  /// Initializes a new instance of the <see cref="Platformer"/> class.
  /// </summary>
  public Platformer()
  {
    this.player = new Player();
    this.platforms = new List<Night.Rectangle>();
  }

  /// <summary>
  /// Loads game assets and initializes game state for the platformer.
  /// Called once at the start of the game by the Night.Engine.
  /// </summary>
  public void Load()
  {
    _ = Night.Window.SetMode(800, 600, SDL.WindowFlags.Resizable);
    Night.Window.SetTitle("Night Platformer Sample");

    this.player.Load();

    string baseDirectory = AppContext.BaseDirectory;
    string platformImageRelativePath = Path.Combine("assets", "images", "pixel_green.png");
    string platformImageFullPath = Path.Combine(baseDirectory, platformImageRelativePath);
    this.platformSprite = Graphics.NewImage(platformImageFullPath);
    if (this.platformSprite == null)
    {
      Console.WriteLine($"Game.Load: Failed to load platform sprite at '{platformImageFullPath}'. Platforms will not be drawn.");
    }

    this.platforms.Add(new Night.Rectangle(50, 500, 700, 50));
    this.platforms.Add(new Night.Rectangle(200, 400, 150, 30));
    this.platforms.Add(new Night.Rectangle(450, 300, 100, 30));
    this.goalPlatform = new Night.Rectangle(600, 200, 100, 30);
    this.platforms.Add(this.goalPlatform);
  }

  /// <summary>
  /// Updates the platformer game state.
  /// Called every frame by the Night.Engine.
  /// </summary>
  /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
  public void Update(double deltaTime)
  {
    this.player.Update(deltaTime, this.platforms);

    Night.Rectangle playerBoundsForGoalCheck = new Night.Rectangle((int)this.player.X, (int)this.player.Y, this.player.Width, this.player.Height + 1);
    if (CheckAABBCollision(playerBoundsForGoalCheck, this.goalPlatform) && !this.goalReachedMessageShown)
    {
      // Simple win condition: print a message.
      Console.WriteLine("Congratulations! Goal Reached!");
      this.goalReachedMessageShown = true;
    }
  }

  /// <summary>
  /// Draws the platformer game scene.
  /// Called every frame by the Night.Engine after Update.
  /// </summary>
  public void Draw()
  {
    Night.Graphics.Clear(new Night.Color(135, 206, 235)); // Sky blue background

    // Draw platforms
    if (this.platformSprite != null)
    {
      foreach (var platform in this.platforms)
      {
        // Scale the 1x1 pixel sprite to the platform's dimensions
        Graphics.Draw(
            sprite: this.platformSprite,
            x: platform.X,
            y: platform.Y,
            rotation: 0,
            scaleX: platform.Width,
            scaleY: platform.Height);
      }
    }

    this.player.Draw();

    // Player and Level drawing logic will go here in later tasks.
  }

  /// <summary>
  /// Handles key press events for the platformer game.
  /// Called by Night.Engine when a key is pressed.
  /// </summary>
  /// <param name="key">The <see cref="Night.KeySymbol"/> of the pressed key.</param>
  /// <param name="scancode">The <see cref="Night.KeyCode"/> (physical key code) of the pressed key.</param>
  /// <param name="isRepeat">True if this is a repeat key event, false otherwise.</param>
  public void KeyPressed(KeySymbol key, KeyCode scancode, bool isRepeat)
  {
    // Minimal key handling for now, primarily for closing the window.
    // System.Console.WriteLine($"SampleGame: KeyPressed - KeySymbol: {key}, Scancode: {scancode}, IsRepeat: {isRepeat}");
    if (key == KeySymbol.Escape)
    {
      Console.WriteLine("SampleGame: Escape key pressed, closing window.");
      Window.Close();
    }

    // Player input (movement, jump) will be handled in Player.Update using Night.Keyboard.IsDown().
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
