// <copyright file="Player.cs" company="Night Circle">
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

namespace SampleGame
{
  /// <summary>
  /// Represents the player character in the game.
  /// Handles player movement, physics, and rendering.
  /// </summary>
  public class Player
  {
    private const float HorizontalSpeed = 200f; // Pixels per second
    private const float JumpStrength = -450f;  // Initial upward velocity
    private const float Gravity = 1000f;       // Pixels per second squared

    private float velocityX;
    private float velocityY;
    private bool isGrounded;
    private Night.Sprite? playerSprite; // To hold the blue rectangle sprite

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    public Player()
    {
      this.isGrounded = false; // Start in the air or assume Load sets initial grounded state
    }

    /// <summary>
    /// Gets the player's current X-coordinate (left edge).
    /// </summary>
    public float X { get; private set; }

    /// <summary>
    /// Gets the player's current Y-coordinate (top edge).
    /// </summary>
    public float Y { get; private set; }

    /// <summary>
    /// Gets the width of the player.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the player.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Loads player assets and initializes player state (position, size, sprite).
    /// </summary>
    public void Load()
    {
      this.Width = 32;
      this.Height = 64;

      // Initial position: Centered horizontally, resting on the first platform (Y=500, H=50).
      // Player's top-left Y = PlatformTopY - PlayerHeight = 500 - 64 = 436.
      // Player's top-left X = (WindowWidth / 2) - (PlayerWidth / 2) = (800 / 2) - (32 / 2) = 400 - 16 = 384.
      // Assuming window width is 800 as per Game.cs Load()
      this.X = 384f;
      this.Y = 436f; // This will be adjusted by gravity until grounded on a floor/platform

      this.velocityX = 0f;
      this.velocityY = 0f;

      // Attempt to load a pre-made blue image for the player.
      // This file needs to exist: "assets/images/player_sprite_blue_32x64.png"
      // Or a 1x1 blue pixel: "assets/images/pixel_blue.png" which we then scale.
      // For simplicity with current Draw method (no tinting), let's assume a 32x64 blue sprite.
      // If we used a 1x1 pixel_blue.png, scaleX would be Width, scaleY would be Height.
      string baseDirectory = AppContext.BaseDirectory;

      // Using a specific asset name as discussed due to lack of tinting.
      // This asset would be a 32x64 solid blue rectangle image.
      string imageRelativePath = Path.Combine("assets", "images", "player_sprite_blue_32x64.png");
      string imageFullPath = Path.Combine(baseDirectory, imageRelativePath);

      this.playerSprite = Graphics.NewImage(imageFullPath);
      if (this.playerSprite == null)
      {
        Console.WriteLine($"Player.Load: Failed to load player sprite at '{imageFullPath}'. A blue rectangle will not be drawn.");
      }
      else
      {
      }

      this.isGrounded = false; // Player starts potentially in the air and falls to ground.
    }

    /// <summary>
    /// Updates the player's state, including handling input, physics, and collisions.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame, in seconds.</param>
    /// <param name="platforms">A list of <see cref="Night.Rectangle"/> objects representing solid platforms.</param>
    /// <param name="joystickAxisValue">The current value of the joystick's horizontal axis (e.g., left stick X).</param>
    /// <param name="hatDirection">The current direction of the joystick's hat (e.g., D-pad).</param>
    /// <param name="joystickAButtonPressed">True if the joystick 'A' button is currently pressed.</param>
    public void Update(double deltaTime, List<Night.Rectangle> platforms, float joystickAxisValue, Night.JoystickHat hatDirection, bool joystickAButtonPressed)
    {
      float dt = (float)deltaTime;
      const float joystickDeadzone = 0.2f;

      // Logger.Debug(
      //     $"Player.Update START: dt={dt.ToString("F5", CultureInfo.InvariantCulture)}, " +
      //     $"X={this.X.ToString("F2", CultureInfo.InvariantCulture)}, Y={this.Y.ToString("F2", CultureInfo.InvariantCulture)}, " +
      //     $"vX={this.velocityX.ToString("F2", CultureInfo.InvariantCulture)}, vY={this.velocityY.ToString("F2", CultureInfo.InvariantCulture)}, " +
      //     $"Grounded={this.isGrounded}");

      // 1. Handle Input & Apply Jump Impulse
      this.velocityX = 0;

      // Joystick Hat (D-Pad) input - highest priority for horizontal movement
      if ((hatDirection & Night.JoystickHat.Left) != 0)
      {
        this.velocityX = -HorizontalSpeed;
      }
      else if ((hatDirection & Night.JoystickHat.Right) != 0)
      {
        this.velocityX = HorizontalSpeed;
      }

      // Joystick Axis input - next priority if D-Pad is not active
      else if (Math.Abs(joystickAxisValue) > joystickDeadzone)
      {
        this.velocityX = joystickAxisValue * HorizontalSpeed;
      }

      // Keyboard input - lowest priority if no joystick input for horizontal movement
      else
      {
        if (Keyboard.IsDown(KeyCode.Left) || Keyboard.IsDown(KeyCode.A))
        {
          this.velocityX = -HorizontalSpeed;
        }

        if (Keyboard.IsDown(KeyCode.Right) || Keyboard.IsDown(KeyCode.D))
        {
          // If left was also pressed, this will override. If only right, it sets.
          // If both, right takes precedence here due to order.
          this.velocityX = HorizontalSpeed;
        }
      }

      // Jump input
      bool tryingToJump = joystickAButtonPressed || Keyboard.IsDown(KeyCode.Space);
      if (tryingToJump && this.isGrounded)
      {
        this.velocityY = JumpStrength;
        this.isGrounded = false; // Explicitly set to false when jump starts
      }

      // 2. Apply Gravity
      if (!this.isGrounded)
      {
        this.velocityY += Gravity * dt;
      }

      // 3. Horizontal Movement and Collision
      this.X += this.velocityX * dt;
      Night.Rectangle playerBoundingBox = new Night.Rectangle((int)this.X, (int)this.Y, this.Width, this.Height);

      foreach (var platform in platforms)
      {
        // Update playerBoundingBox X for current position before check
        playerBoundingBox.X = (int)this.X;

        // Keep Y fixed for horizontal check pass
        playerBoundingBox.Y = (int)this.Y;

        if (CheckAABBCollision(new Night.Rectangle((int)this.X, (int)this.Y, this.Width, this.Height), platform))
        {
          // Moving right, collided with left edge of platform
          if (this.velocityX > 0)
          {
            this.X = platform.X - this.Width;
          }

          // Moving left, collided with right edge of platform
          else if (this.velocityX < 0)
          {
            this.X = platform.X + platform.Width;
          }

          // Stop horizontal movement on collision
          this.velocityX = 0;
        }
      }

      // 4. Vertical Movement and Collision
      this.Y += this.velocityY * dt;

      // Update playerBoundingBox for vertical check using potentially corrected X and new Y
      playerBoundingBox.X = (int)this.X;
      playerBoundingBox.Y = (int)this.Y;

      // Before checking collisions, assume player is not grounded unless a collision proves otherwise.
      // This flag will be set if any interaction during the platform loop results in grounding.
      bool newIsGroundedThisFrame = false;

      foreach (var platform in platforms)
      {
        // Horizontal overlap check (using player's float X for precision against integer platform.X)
        bool horizontalOverlap = this.X + this.Width > platform.X && this.X < platform.X + platform.Width;

        if (horizontalOverlap)
        {
          float playerFloatTop = this.Y;
          float playerFloatBottom = this.Y + this.Height;

          float platformTop = platform.Y;
          float platformBottom = platform.Y + platform.Height;

          // Player is moving downwards
          if (this.velocityY > 0)
          {
            // Check if player's bottom has landed on or passed through the platform's top surface,
            // and the player's top was above the platform's top (i.e., not starting from inside/below).
            if (playerFloatBottom >= platformTop && playerFloatTop < platformTop)
            {
              // Snap player's bottom to platform's top
              this.Y = platformTop - this.Height;
              this.velocityY = 0f;
              newIsGroundedThisFrame = true;
            }
          }

          // Player is moving upwards
          else if (this.velocityY < 0)
          {
            // Check if player's top has hit or passed through the platform's bottom surface,
            // and the player's bottom was below the platform's bottom (i.e., not starting from inside/above).
            if (playerFloatTop <= platformBottom && playerFloatBottom > platformBottom)
            {
              // Snap player's top to platform's bottom
              this.Y = platformBottom;
              this.velocityY = 0f;

              // Hitting head does not make player grounded
            }
          }

          // Additional check for stable grounding if player is (almost) stationary vertically.
          // This handles cases where player is already on the platform, slid onto it, or just landed.
          // It's important this runs even if _velocityY became 0 in this frame due to landing.
          // If effectively stationary vertically
          if (Math.Abs(this.velocityY) < 0.1f)
          {
            // Check if player's bottom is at or very slightly through the platform's top,
            // and player's head is above the platform's top.
            // The (platformTop + 1.0f) allows for a small 1px penetration to still count as grounded.
            if (playerFloatBottom >= platformTop && playerFloatBottom < (platformTop + 1.0f) && playerFloatTop < platformTop)
            {
              // Snap firmly
              this.Y = platformTop - this.Height;

              // Ensure velocity is zeroed
              this.velocityY = 0f;
              newIsGroundedThisFrame = true;
            }
          }
        }
      }

      this.isGrounded = newIsGroundedThisFrame;

      // Logger.Debug(
      //     $"Player.Update END: X={this.X.ToString("F2", CultureInfo.InvariantCulture)}, Y={this.Y.ToString("F2", CultureInfo.InvariantCulture)}, " +
      //     $"vX={this.velocityX.ToString("F2", CultureInfo.InvariantCulture)}, vY={this.velocityY.ToString("F2", CultureInfo.InvariantCulture)}, " +
      //     $"Grounded={this.isGrounded}");

      // Prevent player from going off-screen left/right (simple boundary)
      // These values should ideally come from Window.GetWidth/Height if game resizes
      float gameWindowWidth = 800;
      if (this.X < 0)
      {
        this.X = 0;
      }

      if (this.X + this.Width > gameWindowWidth)
      {
        this.X = gameWindowWidth - this.Width;
      }

      // Top boundary (optional, could allow jumping off screen top)
      // if (Y < 0) { Y = 0; if (_velocityY < 0) _velocityY = 0; }
    }

    /// <summary>
    /// Draws the player on the screen.
    /// </summary>
    public void Draw()
    {
      // Logger.Debug($"Player.Draw: X={this.X.ToString("F2", CultureInfo.InvariantCulture)}, Y={this.Y.ToString("F2", CultureInfo.InvariantCulture)}");
      if (this.playerSprite != null)
      {
        // If player_sprite_blue_32x64.png is exactly 32x64, scaleX and scaleY are 1.
        // If it was a 1x1 pixel_blue.png, then scaleX=Width, scaleY=Height.
        // Assuming the loaded sprite is already the correct size (32x64):
        Graphics.Draw(this.playerSprite, this.X, this.Y);
      }
      else
      {
        // Fallback: Could draw a placeholder or nothing if sprite failed to load.
        // For now, player is invisible if sprite load fails.
        // To draw a white rectangle as a fallback (if we had a white pixel sprite):
        // if (_fallbackWhitePixelSprite != null) {
        //    Graphics.Draw(_fallbackWhitePixelSprite, X, Y, 0, Width, Height);
        // }
      }
    }

    private static bool CheckAABBCollision(Night.Rectangle rect1, Night.Rectangle rect2)
    {
      // True if the rectangles are overlapping
      return rect1.X < rect2.X + rect2.Width &&
             rect1.X + rect1.Width > rect2.X &&
             rect1.Y < rect2.Y + rect2.Height &&
             rect1.Y + rect1.Height > rect2.Y;
    }
  }
}
