// <copyright file="System.cs" company="Night Circle">
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

using SDL3;

namespace Night
{
  /// <summary>
  /// Represents the basic state of the system's power supply.
  /// </summary>
  public enum PowerState
  {
    /// <summary>
    /// Cannot determine power status, or an error occurred.
    /// </summary>
    Unknown,

    /// <summary>
    /// Not plugged in, running on the battery.
    /// </summary>
    Battery,

    /// <summary>
    /// Plugged in, no battery available.
    /// </summary>
    NoBattery,

    /// <summary>
    /// Plugged in, charging battery.
    /// </summary>
    Charging,

    /// <summary>
    /// Plugged in, battery charged.
    /// </summary>
    Charged,
  }

  /// <summary>
  /// Provides access to system-level information and functions.
  /// </summary>
  public static class System
  {
    /// <summary>
    /// Gets the current text in the system's clipboard.
    /// </summary>
    /// <returns>Clipboard text as a string.</returns>
    public static string GetClipboardText()
    {
      return SDL.GetClipboardText();
    }

    /// <summary>
    /// Gets the current operating system.
    /// This function is similar to LÖVE's love.system.getOS().
    /// </summary>
    /// <returns>
    /// The current operating system: "OS X", "Windows", "Linux", "Android", or "iOS".
    /// Returns the raw platform string from SDL if the OS is not one of the above.
    /// </returns>
    public static string GetOS()
    {
      string sdlPlatform = SDL.GetPlatform();
      switch (sdlPlatform)
      {
        case "Windows":
          return "Windows";
        case "Mac OS X":
        case "macOS":
          return "OS X";
        case "Linux":
          return "Linux";
        case "Android":
          return "Android";
        case "iOS":
          return "iOS";
        default:
          // If SDL returns something unexpected, pass it through.
          // This helps in identifying new/unhandled platforms.
          return sdlPlatform;
      }
    }

    /// <summary>
    /// Gets information about the system's power supply.
    /// This function is similar to LÖVE's love.system.getPowerInfo().
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description><c>state</c>: The basic state of the power supply (<see cref="PowerState"/>).</description></item>
    /// <item><description><c>percent</c>: Percentage of battery life left (0-100), or <c>null</c> if not applicable/determinable.</description></item>
    /// <item><description><c>seconds</c>: Seconds of battery life left, or <c>null</c> if not applicable/determinable.</description></item>
    /// </list>
    /// </returns>
    public static (PowerState State, int? Percent, int? Seconds) GetPowerInfo()
    {
      SDL.PowerState sdlState = SDL.GetPowerInfo(out int seconds, out int percent);

      PowerState nightState;
      switch (sdlState)
      {
        case SDL.PowerState.OnBattery:
          nightState = PowerState.Battery;
          break;
        case SDL.PowerState.NoBattery:
          nightState = PowerState.NoBattery;
          break;
        case SDL.PowerState.Charging:
          nightState = PowerState.Charging;
          break;
        case SDL.PowerState.Charged:
          nightState = PowerState.Charged;
          break;
        case SDL.PowerState.Error:
        case SDL.PowerState.Unknown:
        default:
          nightState = PowerState.Unknown;
          break;
      }

      // SDL returns -1 for seconds/percent if not available or error
      int? nullablePercent = percent == -1 ? null : (int?)percent;
      int? nullableSeconds = seconds == -1 ? null : (int?)seconds;

      return (nightState, nullablePercent, nullableSeconds);
    }

    /// <summary>
    /// Gets the amount of logical processors in the system.
    /// </summary>
    /// <returns>Amount of logical processors.</returns>
    /// <remarks>
    /// The number includes the threads reported if technologies such as Intel's Hyper-threading are enabled.
    /// For example, on a 4-core CPU with Hyper-threading, this function will return 8.
    /// </remarks>
    public static int GetProcessorCount()
    {
      return SDL.GetNumLogicalCPUCores();
    }

    /// <summary>
    /// Opens a URL with the user's web or file browser.
    /// </summary>
    /// <param name="url">The URL to open. Must be formatted as a proper URL.</param>
    /// <returns>Whether the URL was opened successfully.</returns>
    public static bool OpenURL(string url)
    {
      return SDL.OpenURL(url);
    }

    /// <summary>
    /// Puts text in the system's clipboard.
    /// </summary>
    /// <param name="text">The new text to hold in the system's clipboard.</param>
    /// <returns>True if the operation was successful, false otherwise.</returns>
    public static bool SetClipboardText(string text)
    {
      return SDL.SetClipboardText(text);
    }
  }
}
