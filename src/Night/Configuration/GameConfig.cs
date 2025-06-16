// <copyright file="GameConfig.cs" company="Night Circle">
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

using System.Text.Json.Serialization;

namespace Night
{
  /// <summary>
  /// Represents the overall game configuration settings, typically loaded from a config.json file.
  /// </summary>
  public class GameConfig
  {
    /// <summary>
    /// Gets or sets the identity of the game. This is used for the save directory.
    /// </summary>
    [JsonPropertyName("identity")]
    public string? Identity { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the game identity should be appended to the save directory path.
    /// </summary>
    [JsonPropertyName("appendidentity")]
    public bool AppendIdentity { get; set; } = false;

    /// <summary>
    /// Gets or sets the Night Engine version this game targets.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = VersionInfo.GetVersion();

    /// <summary>
    /// Gets or sets a value indicating whether a console window should be attached (Windows only, currently placeholder).
    /// </summary>
    [JsonPropertyName("console")]
    public bool Console { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the accelerometer should be used as a joystick.
    /// </summary>
    [JsonPropertyName("accelerometerjoystick")]
    public bool AccelerometerJoystick { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to request external storage access (Android only).
    /// </summary>
    [JsonPropertyName("externalstorage")]
    public bool ExternalStorage { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether gamma correction should be enabled.
    /// </summary>
    [JsonPropertyName("gammacorrect")]
    public bool GammaCorrect { get; set; } = false;

    /// <summary>
    /// Gets or sets the audio module configuration.
    /// </summary>
    [JsonPropertyName("audio")]
    public AudioConfig Audio { get; set; } = new AudioConfig();

    /// <summary>
    /// Gets or sets the window module configuration.
    /// </summary>
    [JsonPropertyName("window")]
    public WindowConfig Window { get; set; } = new WindowConfig();

    /// <summary>
    /// Gets or sets the configuration for enabling/disabling engine modules.
    /// </summary>
    [JsonPropertyName("modules")]
    public ModulesConfig Modules { get; set; } = new ModulesConfig();
  }
}
