// <copyright file="GameConfigTest.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Configuration
{
  /// <summary>
  /// Tests for Night.Configuration.GameConfig.
  /// </summary>
  public class ConfigurationGameConfig_GetSet : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.GameConfig";

    /// <inheritdoc/>
    public override string Description => "Tests getters and setters.";

    /// <inheritdoc/>
    public override string SuccessMessage => "GameConfig getters and setters passed successfully.";

    /// <inheritdoc/>
    public override void Run()
    {
      GameConfig config = new GameConfig();
      config.Identity = "TestGame";
      Assert.Equal("TestGame", config.Identity);

      config.AppendIdentity = true;
      Assert.True(config.AppendIdentity);

      config.Version = "1.0.0";
      Assert.Equal("1.0.0", config.Version);

      config.Console = true;
      Assert.True(config.Console);

      config.AccelerometerJoystick = false;
      Assert.False(config.AccelerometerJoystick);

      config.ExternalStorage = true;
      Assert.True(config.ExternalStorage);

      config.GammaCorrect = false;
      Assert.False(config.GammaCorrect);

      config.Audio = new AudioConfig
      {
        MixWithSystem = true,
      };
      Assert.True(config.Audio.MixWithSystem);

      config.Window = new WindowConfig
      {
        Title = "Test Window",
        IconPath = "icon.png",
        Width = 1024,
        Height = 768,
        X = 100,
        Y = 200,
        MinWidth = 800,
        MinHeight = 600,
        Resizable = true,
        Borderless = true,
        Fullscreen = false,
        FullscreenType = "exclusive",
        VSync = true,
        HighDPI = true,
        MSAA = 4,
        Depth = 24,
        Stencil = 8,
        Display = 0,
        UseDPIScale = false,
      };
      Assert.Equal("Test Window", config.Window.Title);
      Assert.Equal("icon.png", config.Window.IconPath);
      Assert.Equal(1024, config.Window.Width);
      Assert.Equal(768, config.Window.Height);
      Assert.Equal(100, config.Window.X);
      Assert.Equal(200, config.Window.Y);
      Assert.Equal(800, config.Window.MinWidth);
      Assert.Equal(600, config.Window.MinHeight);
      Assert.True(config.Window.Resizable);
      Assert.True(config.Window.Borderless);
      Assert.False(config.Window.Fullscreen);
      Assert.Equal("exclusive", config.Window.FullscreenType);
      Assert.True(config.Window.VSync);
      Assert.True(config.Window.HighDPI);
      Assert.Equal(4, config.Window.MSAA);
      Assert.Equal(24, config.Window.Depth);
      Assert.Equal(8, config.Window.Stencil);
      Assert.Equal(0, config.Window.Display);
      Assert.False(config.Window.UseDPIScale);

      config.Modules = new ModulesConfig
      {
        Audio = false,
        Data = false,
        Event = false,
        Font = false,
        Graphics = false,
        Image = false,
        Joystick = false,
        Keyboard = false,
        Math = false,
        Mouse = false,
        Physics = false,
        Sound = false,
        System = false,
        Timer = false,
        Touch = false,
        Video = false,
        WindowModule = false,
        Thread = false,
      };
    }
  }
}
