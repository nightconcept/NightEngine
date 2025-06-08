// <copyright file="ModulesConfig.cs" company="Night Circle">
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
  /// Configuration for enabling/disabling engine modules.
  /// </summary>
  public class ModulesConfig
  {
    /// <summary>Gets or sets a value indicating whether the Audio module is enabled.</summary>
    [JsonPropertyName("audio")]
    public bool Audio { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Data module is enabled.</summary>
    [JsonPropertyName("data")]
    public bool Data { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Event module is enabled.</summary>
    [JsonPropertyName("event")]
    public bool Event { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Font module is enabled.</summary>
    [JsonPropertyName("font")]
    public bool Font { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Graphics module is enabled.</summary>
    [JsonPropertyName("graphics")]
    public bool Graphics { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Image module is enabled.</summary>
    [JsonPropertyName("image")]
    public bool Image { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Joystick module is enabled.</summary>
    [JsonPropertyName("joystick")]
    public bool Joystick { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Keyboard module is enabled.</summary>
    [JsonPropertyName("keyboard")]
    public bool Keyboard { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Math module is enabled.</summary>
    [JsonPropertyName("math")]
    public bool Math { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Mouse module is enabled.</summary>
    [JsonPropertyName("mouse")]
    public bool Mouse { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Physics module is enabled.</summary>
    [JsonPropertyName("physics")]
    public bool Physics { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Sound module is enabled.</summary>
    [JsonPropertyName("sound")]
    public bool Sound { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the System module is enabled.</summary>
    [JsonPropertyName("system")]
    public bool System { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Timer module is enabled.</summary>
    [JsonPropertyName("timer")]
    public bool Timer { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Touch module is enabled.</summary>
    [JsonPropertyName("touch")]
    public bool Touch { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Video module is enabled.</summary>
    [JsonPropertyName("video")]
    public bool Video { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Window module is enabled.</summary>
    [JsonPropertyName("window")]
    public bool WindowModule { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the Thread module is enabled.</summary>
    [JsonPropertyName("thread")]
    public bool Thread { get; set; } = true;
  }
}
