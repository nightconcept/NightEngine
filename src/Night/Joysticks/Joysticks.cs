// <copyright file="Joysticks.cs" company="Night Circle">
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
using System.Linq;

namespace Night
{
  /// <summary>
  /// Provides functionality for managing and querying joysticks.
  /// Corresponds to LÖVE's `love.joystick` module.
  /// </summary>
  public static class Joysticks
  {
    private static readonly Dictionary<uint, Joystick> ActiveJoysticks = new Dictionary<uint, Joystick>();

    /// <summary>
    /// Gets a list of connected Joysticks.
    /// Corresponds to `love.joystick.getJoysticks()`.
    /// </summary>
    /// <returns>A list of currently connected <see cref="Joystick"/> objects.</returns>
    public static List<Joystick> GetJoysticks()
    {
      // Return a copy to prevent external modification of the internal list
      return ActiveJoysticks.Values.ToList();
    }

    /// <summary>
    /// Gets the number of connected joysticks.
    /// Corresponds to `love.joystick.getJoystickCount()`.
    /// (Renamed from `love.joystick.getNumJoysticks` in LÖVE).
    /// </summary>
    /// <returns>The number of connected joysticks.</returns>
    public static int GetJoystickCount()
    {
      return ActiveJoysticks.Count;
    }

    /// <summary>
    /// Gets an active joystick by its SDL instance ID.
    /// </summary>
    /// <param name="instanceId">The SDL instance ID of the joystick.</param>
    /// <returns>The <see cref="Joystick"/> instance if found and active, otherwise null.</returns>
    public static Joystick? GetJoystickByInstanceId(uint instanceId)
    {
      _ = ActiveJoysticks.TryGetValue(instanceId, out Joystick? joystickInstance);
      return joystickInstance;
    }

    /// <summary>
    /// Adds a joystick to the active list when an SDL_EVENT_JOYSTICK_ADDED event occurs.
    /// </summary>
    /// <param name="instanceId">The SDL instance ID of the joystick to add.</param>
    /// <returns>The newly created and added <see cref="Joystick"/> instance, or null if it failed to open.</returns>
    internal static Joystick? AddJoystick(uint instanceId)
    {
      if (ActiveJoysticks.ContainsKey(instanceId))
      {
        // Already exists, perhaps an erroneous event or already handled.
        // Night.Log.LogManager.GetLogger("Joysticks").Warn($"Joystick with instance ID {instanceId} already exists in ActiveJoysticks.");
        return ActiveJoysticks[instanceId];
      }

      try
      {
        Joystick newJoystick = new Joystick(instanceId);
        ActiveJoysticks[instanceId] = newJoystick;

        // Night.Log.LogManager.GetLogger("Joysticks").Info($"Joystick added: ID {newJoystick.GetId()}, Name '{newJoystick.GetName()}', InstanceID {instanceId}");
        return newJoystick;
      }
      catch (InvalidOperationException)
      {
        // Night.Log.LogManager.GetLogger("Joysticks").Error($"Failed to add joystick with instance ID {instanceId}: {ex.Message}");
        return null;
      }
    }

    /// <summary>
    /// Removes a joystick from the active list when an SDL_EVENT_JOYSTICK_REMOVED event occurs.
    /// The Joystick object is returned so it can be passed to the event callback before disposal.
    /// </summary>
    /// <param name="instanceId">The SDL instance ID of the joystick to remove.</param>
    /// <returns>The removed <see cref="Joystick"/> instance if found, otherwise null.</returns>
    internal static Joystick? RemoveJoystick(uint instanceId)
    {
      if (ActiveJoysticks.TryGetValue(instanceId, out Joystick? joystickInstance))
      {
        _ = ActiveJoysticks.Remove(instanceId);
        joystickInstance.SetConnectedState(false); // Mark as disconnected

        // Night.Log.LogManager.GetLogger("Joysticks").Info($"Joystick removed: ID {joystickInstance.GetId()}, Name '{joystickInstance.GetName()}', InstanceID {instanceId}");
        return joystickInstance;
      }
      else
      {
        // Night.Log.LogManager.GetLogger("Joysticks").Warn($"Attempted to remove joystick with instance ID {instanceId}, but it was not found in ActiveJoysticks.");
        return null;
      }
    }

    /// <summary>
    /// Clears all active joysticks. Called during framework shutdown.
    /// </summary>
    internal static void ClearJoysticks()
    {
      foreach (var joystick in ActiveJoysticks.Values)
      {
        joystick.Dispose();
      }

      ActiveJoysticks.Clear();

      // Night.Log.LogManager.GetLogger("Joysticks").Info("All active joysticks cleared and disposed.");
    }
  }
}
