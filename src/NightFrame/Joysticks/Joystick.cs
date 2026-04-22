// <copyright file="Joystick.cs" company="Night Circle">
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
using System.Runtime.InteropServices;
using System.Text;

using SDL3;

namespace Night
{
  /// <summary>
  /// Virtual gamepad axes.
  /// </summary>
  public enum GamepadAxis
  {
    /// <summary>
    /// The horizontal axis of the left analog stick.
    /// </summary>
    LeftX,

    /// <summary>
    /// The vertical axis of the left analog stick.
    /// </summary>
    LeftY,

    /// <summary>
    /// The horizontal axis of the right analog stick.
    /// </summary>
    RightX,

    /// <summary>
    /// The vertical axis of the right analog stick.
    /// </summary>
    RightY,

    /// <summary>
    /// The left trigger axis.
    /// </summary>
    TriggerLeft,

    /// <summary>
    /// The right trigger axis.
    /// </summary>
    TriggerRight,
  }

  /// <summary>
  /// Virtual gamepad buttons.
  /// </summary>
  public enum GamepadButton
  {
    /// <summary>
    /// The 'A' button (often cross or bottom face button, SDL_GAMEPAD_BUTTON_SOUTH).
    /// </summary>
    A,

    /// <summary>
    /// The 'B' button (often circle or right face button, SDL_GAMEPAD_BUTTON_EAST).
    /// </summary>
    B,

    /// <summary>
    /// The 'X' button (often square or left face button, SDL_GAMEPAD_BUTTON_WEST).
    /// </summary>
    X,

    /// <summary>
    /// The 'Y' button (often triangle or top face button, SDL_GAMEPAD_BUTTON_NORTH).
    /// </summary>
    Y,

    // Aliases for face buttons

    /// <summary>
    /// Alias for the 'A' button (South face button).
    /// </summary>
    South = A,

    /// <summary>
    /// Alias for the 'B' button (East face button).
    /// </summary>
    East = B,

    /// <summary>
    /// Alias for the 'X' button (West face button).
    /// </summary>
    West = X,

    /// <summary>
    /// Alias for the 'Y' button (North face button).
    /// </summary>
    North = Y,

    /// <summary>
    /// The 'Back' or 'Select' button.
    /// </summary>
    Back,

    /// <summary>
    /// The 'Guide' or 'Home' button (e.g., Xbox button).
    /// </summary>
    Guide,

    /// <summary>
    /// The 'Start' button.
    /// </summary>
    Start,

    /// <summary>
    /// The D-pad left button.
    /// </summary>
    DPLeft,

    /// <summary>
    /// The D-pad right button.
    /// </summary>
    DPRight,

    /// <summary>
    /// The D-pad up button.
    /// </summary>
    DPUp,

    /// <summary>
    /// The D-pad down button.
    /// </summary>
    DPDown,

    /// <summary>
    /// The left shoulder button (bumper).
    /// </summary>
    LeftShoulder,

    /// <summary>
    /// The right shoulder button (bumper).
    /// </summary>
    RightShoulder,

    /// <summary>
    /// The left analog stick click button.
    /// </summary>
    LeftStick,

    /// <summary>
    /// The right analog stick click button.
    /// </summary>
    RightStick,
  }

  /// <summary>
  /// Joystick hat positions.
  /// </summary>
  public enum JoystickHat : byte
  {
    /// <summary>
    /// Hat is centered. (SDL_HAT_CENTERED).
    /// </summary>
    Centered = 0x00,

    /// <summary>
    /// Hat is pressed up. (SDL_HAT_UP).
    /// </summary>
    Up = 0x01,

    /// <summary>
    /// Hat is pressed right. (SDL_HAT_RIGHT).
    /// </summary>
    Right = 0x02,

    /// <summary>
    /// Hat is pressed down. (SDL_HAT_DOWN).
    /// </summary>
    Down = 0x04,

    /// <summary>
    /// Hat is pressed left. (SDL_HAT_LEFT).
    /// </summary>
    Left = 0x08,

    /// <summary>
    /// Hat is pressed up and right. (SDL_HAT_RIGHTUP).
    /// </summary>
    RightUp = Right | Up, // 0x03

    /// <summary>
    /// Hat is pressed down and right. (SDL_HAT_RIGHTDOWN).
    /// </summary>
    RightDown = Right | Down, // 0x06

    /// <summary>
    /// Hat is pressed up and left. (SDL_HAT_LEFTUP).
    /// </summary>
    LeftUp = Left | Up, // 0x09

    /// <summary>
    /// Hat is pressed down and left. (SDL_HAT_LEFTDOWN).
    /// </summary>
    LeftDown = Left | Down, // 0x0C
  }

  /// <summary>
  /// Types of Joystick inputs.
  /// </summary>
  public enum JoystickInputType
  {
    /// <summary>
    /// The input is an axis.
    /// </summary>
    Axis,

    /// <summary>
    /// The input is a button.
    /// </summary>
    Button,

    /// <summary>
    /// The input is a hat.
    /// </summary>
    Hat,
  }

  /// <summary>
  /// OS-independent device info of the joystick.
  /// </summary>
  public struct DeviceInfo
  {
    /// <summary>
    /// The vendor ID of the joystick.
    /// </summary>
    public int Vendor;

    /// <summary>
    /// The product ID of the joystick.
    /// </summary>
    public int Product;

    /// <summary>
    /// The version number of the joystick.
    /// </summary>
    public int Version;

    /// <summary>
    /// The GUID of the joystick.
    /// </summary>
    public string Guid;
  }

  /// <summary>
  /// Represents the vibration motor strengths.
  /// </summary>
  public struct VibrationStrength
  {
    /// <summary>
    /// The strength of the left vibration motor (0-1).
    /// </summary>
    public float Left;

    /// <summary>
    /// The strength of the right vibration motor (0-1).
    /// </summary>
    public float Right;
  }

  /// <summary>
  /// Represents the result of a gamepad mapping query.
  /// </summary>
  public struct GamepadMappingResult
  {
    /// <summary>
    /// Indicates whether a valid mapping exists.
    /// </summary>
    public bool IsValid;

    /// <summary>
    /// The type of the input (axis, button, or hat).
    /// </summary>
    public JoystickInputType Type;

    /// <summary>
    /// The index of the input (e.g., axis index, button index).
    /// </summary>
    public int InputIndex;

    /// <summary>
    /// The hat value, only relevant if <see cref="Type"/> is <see cref="JoystickInputType.Hat"/>.
    /// </summary>
    public JoystickHat HatValue;
  }

  /// <summary>
  /// Represents a physical joystick.
  /// </summary>
  public class Joystick : IDisposable
  {
    private readonly uint instanceId;
    private readonly IntPtr joystickDevicePtr;
    private IntPtr gamepadDevicePtr = IntPtr.Zero;
    private bool disposed = false;
    private bool isConnected = true; // Assume connected on creation, updated by events

    /// <summary>
    /// Initializes a new instance of the <see cref="Joystick"/> class.
    /// Joystick instances are typically obtained via methods in the <c>Night.Joysticks.Joysticks</c> class.
    /// </summary>
    /// <param name="instanceId">The SDL instance ID of the joystick.</param>
    /// <exception cref="InvalidOperationException">Thrown if the joystick cannot be opened.</exception>
    internal Joystick(uint instanceId)
    {
      this.instanceId = instanceId;
      this.joystickDevicePtr = SDL.OpenJoystick(instanceId);
      if (this.joystickDevicePtr == IntPtr.Zero)
      {
        throw new InvalidOperationException($"Failed to open joystick with ID {instanceId}: {SDL.GetError()}");
      }

      if (SDL.IsGamepad(instanceId))
      {
        this.gamepadDevicePtr = SDL.OpenGamepad(instanceId);
        if (this.gamepadDevicePtr == IntPtr.Zero)
        {
          // Log warning, but don't fail construction. It's still a valid joystick.
          // Night.Log.LogManager.GetLogger("Joystick").Warn($"Joystick {instanceId} is a gamepad, but failed to open as gamepad: {SDL.GetError()}");
        }
      }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Joystick"/> class.
    /// </summary>
    ~Joystick()
    {
      this.Dispose(false);
    }

    /// <summary>
    /// Gets the SDL instance ID of this joystick.
    /// </summary>
    internal uint InstanceId => this.instanceId;

    /// <summary>
    /// Gets the button, axis or hat that a virtual gamepad axis is bound to.
    /// (This is a complex LÖVE feature and is currently not fully implemented.)
    /// </summary>
    /// <param name="axis">The virtual gamepad axis to check.</param>
    /// <returns>A GamepadMappingResult indicating the physical input. Currently returns IsValid = false.</returns>
    public static GamepadMappingResult GetGamepadMapping(GamepadAxis axis)
    {
      _ = axis; // Parameter is required for overload and future implementation.

      // SDL_GetGamepadBindings might be useful here but is complex to parse.
      // For now, returning not valid.
      return new GamepadMappingResult { IsValid = false };
    }

    /// <summary>
    /// Gets the button, axis or hat that a virtual gamepad button is bound to.
    /// (This is a complex LÖVE feature and is currently not fully implemented.)
    /// </summary>
    /// <param name="button">The virtual gamepad button to check.</param>
    /// <returns>A GamepadMappingResult indicating the physical input. Currently returns IsValid = false.</returns>
    public static GamepadMappingResult GetGamepadMapping(GamepadButton button)
    {
      _ = button; // Parameter is required for overload and future implementation.

      // SDL_GetGamepadBindings might be useful here but is complex to parse.
      // For now, returning not valid.
      return new GamepadMappingResult { IsValid = false };
    }

    /// <summary>
    /// Gets the direction of each axis.
    /// </summary>
    /// <returns>An array of floats, one for each axis direction, or an empty array if disposed or an error occurs.</returns>
    public float[] GetAxes()
    {
      if (this.disposed)
      {
        return Array.Empty<float>();
      }

      int axisCount = SDL.GetNumJoystickAxes(this.joystickDevicePtr);
      if (axisCount < 0)
      {
        return Array.Empty<float>();
      }

      float[] axes = new float[axisCount];
      for (int i = 0; i < axisCount; i++)
      {
        axes[i] = this.GetAxis(i);
      }

      return axes;
    }

    /// <summary>
    /// Gets the direction of an axis.
    /// </summary>
    /// <param name="axisIndex">The index of the axis.</param>
    /// <returns>The direction of the specified axis (-1.0 to 1.0), or 0.0f if disposed.</returns>
    public float GetAxis(int axisIndex)
    {
      if (this.disposed)
      {
        return 0.0f;
      }

      short rawValue = SDL.GetJoystickAxis(this.joystickDevicePtr, axisIndex);
      return NormalizeAxisValue(rawValue);
    }

    /// <summary>
    /// Gets the number of axes on the joystick.
    /// </summary>
    /// <returns>The number of axes, or 0 if disposed or an error occurs.</returns>
    public int GetAxisCount()
    {
      if (this.disposed)
      {
        return 0;
      }

      int count = SDL.GetNumJoystickAxes(this.joystickDevicePtr);
      return count < 0 ? 0 : count;
    }

    /// <summary>
    /// Gets the number of buttons on the joystick.
    /// </summary>
    /// <returns>The number of buttons, or 0 if disposed or an error occurs.</returns>
    public int GetButtonCount()
    {
      if (this.disposed)
      {
        return 0;
      }

      int count = SDL.GetNumJoystickButtons(this.joystickDevicePtr);
      return count < 0 ? 0 : count;
    }

    /// <summary>
    /// Gets the OS-independent device info of the joystick.
    /// </summary>
    /// <returns>The device information.</returns>
    public DeviceInfo GetDeviceInfo()
    {
      if (this.disposed)
      {
        return new DeviceInfo { Guid = string.Empty };
      }

      SDL.GUID sdlGuid = SDL.GetJoystickGUIDForID(this.instanceId);
      SDL.GetJoystickGUIDInfo(sdlGuid, out short vendor, out short product, out short version, out _);
      byte[] guidBuffer = new byte[37]; // SDL_GUID_STRING_LENGTH is 36 + null terminator
      SDL.GUIDToString(sdlGuid, ref guidBuffer, guidBuffer.Length);
      string guidString = Encoding.UTF8.GetString(guidBuffer).Split('\0')[0];

      return new DeviceInfo
      {
        Vendor = vendor,
        Product = product,
        Version = version,
        Guid = guidString,
      };
    }

    /// <summary>
    /// Gets a stable GUID unique to the type of the physical joystick.
    /// </summary>
    /// <returns>The joystick's GUID as a string, or an empty string if disposed.</returns>
    public string GetGuid()
    {
      if (this.disposed)
      {
        return string.Empty;
      }

      SDL.GUID sdlGuid = SDL.GetJoystickGUIDForID(this.instanceId);
      byte[] guidBuffer = new byte[37]; // SDL_GUID_STRING_LENGTH is 36 + null terminator
      SDL.GUIDToString(sdlGuid, ref guidBuffer, guidBuffer.Length);
      return Encoding.UTF8.GetString(guidBuffer).Split('\0')[0];
    }

    /// <summary>
    /// Gets the direction of a virtual gamepad axis.
    /// </summary>
    /// <param name="axis">The virtual gamepad axis.</param>
    /// <returns>The direction of the gamepad axis (-1.0 to 1.0), or 0.0f if not a gamepad or disposed.</returns>
    public float GetGamepadAxis(GamepadAxis axis)
    {
      if (this.disposed || this.gamepadDevicePtr == IntPtr.Zero)
      {
        return 0.0f;
      }

      SDL.GamepadAxis sdlAxis = MapNightGamepadAxisToSdl(axis);
      short rawValue = SDL.GetGamepadAxis(this.gamepadDevicePtr, sdlAxis);
      return NormalizeAxisValue(rawValue);
    }

    /// <summary>
    /// Gets the full gamepad mapping string of this Joystick, if it's recognized as a gamepad.
    /// </summary>
    /// <returns>The gamepad mapping string, or null if not a gamepad or disposed.</returns>
    public string? GetGamepadMappingString()
    {
      if (this.disposed || this.gamepadDevicePtr == IntPtr.Zero)
      {
        return null;
      }

      // SDL.GetJoystickMappingForID or similar not found in current bindings.
      return null;
    }

    /// <summary>
    /// Gets the direction of a hat.
    /// </summary>
    /// <param name="hatIndex">The index of the hat.</param>
    /// <returns>The direction of the specified hat, or JoystickHat.Centered if disposed or an error occurs.</returns>
    public JoystickHat GetHat(int hatIndex)
    {
      if (this.disposed)
      {
        return JoystickHat.Centered;
      }

      // SDL.GetJoystickHat returns an enum like SDL.JoystickHat or SDL.Hat
      // We need to cast it to byte to match our enum definition based on SDL_HAT_* values
      byte sdlHatValue = (byte)SDL.GetJoystickHat(this.joystickDevicePtr, hatIndex);
      return (JoystickHat)sdlHatValue;
    }

    /// <summary>
    /// Gets the number of hats on the joystick.
    /// </summary>
    /// <returns>The number of hats, or 0 if disposed or an error occurs.</returns>
    public int GetHatCount()
    {
      if (this.disposed)
      {
        return 0;
      }

      int count = SDL.GetNumJoystickHats(this.joystickDevicePtr);
      return count < 0 ? 0 : count;
    }

    /// <summary>
    /// Gets the joystick's unique instance identifier.
    /// </summary>
    /// <returns>The joystick's ID (matches SDL_JoystickID).</returns>
    public uint GetId()
    {
      return this.instanceId;
    }

    /// <summary>
    /// Gets the name of the joystick.
    /// </summary>
    /// <returns>The joystick's name, or an empty string if disposed.</returns>
    public string GetName()
    {
      if (this.disposed)
      {
        return string.Empty;
      }

      return SDL.GetJoystickName(this.joystickDevicePtr) ?? string.Empty;
    }

    /// <summary>
    /// Gets the current vibration motor strengths on a Joystick with rumble support.
    /// </summary>
    /// <returns>The current vibration strengths (0-1). Returns (0,0) if not supported or disposed.</returns>
    public VibrationStrength GetVibration()
    {
      if (this.disposed || !this.IsVibrationSupported())
      {
        return new VibrationStrength { Left = 0f, Right = 0f };
      }

      // SDL_GetJoystickRumble is for current state, not supported in SDL3-CS directly for get.
      // LÖVE's getVibration implies it knows the last set state. We'll have to store it.
      // For now, this is a simplification. A more complete impl would store last set values.
      // SDL3 doesn't have a direct "get current rumble state" function.
      // This function in LÖVE might return the last values set by love.joystick.setVibration.
      // We will need to add private fields to store _currentLeftVibration and _currentRightVibration
      // and update them in SetVibration. For now, returning 0.
      return new VibrationStrength { Left = 0f, Right = 0f }; // Placeholder
    }

    /// <summary>
    /// Gets whether the Joystick is connected.
    /// This is managed by the Joysticks class based on add/remove events.
    /// </summary>
    /// <returns>True if the joystick is considered connected, false otherwise.</returns>
    public bool IsConnected()
    {
      return !this.disposed && this.isConnected;
    }

    /// <summary>
    /// Checks if a specific joystick button is pressed.
    /// </summary>
    /// <param name="buttonIndex">The index of the button.</param>
    /// <returns>True if the button is pressed, false otherwise or if disposed.</returns>
    public bool IsDown(int buttonIndex)
    {
      if (this.disposed)
      {
        return false;
      }

      return SDL.GetJoystickButton(this.joystickDevicePtr, buttonIndex);
    }

    /// <summary>
    /// Checks if the Joystick is recognized as a gamepad by SDL.
    /// </summary>
    /// <returns>True if it's a gamepad and successfully opened as one, false otherwise or if disposed.</returns>
    public bool IsGamepad()
    {
      return !this.disposed && this.gamepadDevicePtr != IntPtr.Zero;
    }

    /// <summary>
    /// Checks if a virtual gamepad button is pressed.
    /// </summary>
    /// <param name="button">The virtual gamepad button.</param>
    /// <returns>True if the button is pressed, false otherwise, if not a gamepad, or if disposed.</returns>
    public bool IsGamepadDown(GamepadButton button)
    {
      if (this.disposed || this.gamepadDevicePtr == IntPtr.Zero)
      {
        return false;
      }

      SDL.GamepadButton sdlButton = MapNightGamepadButtonToSdl(button);
      return SDL.GetGamepadButton(this.gamepadDevicePtr, sdlButton);
    }

    /// <summary>
    /// Checks if the Joystick supports vibration (rumble).
    /// </summary>
    /// <returns>True if vibration is supported, false otherwise or if disposed.</returns>
    public bool IsVibrationSupported()
    {
      if (this.disposed)
      {
        return false;
      }

      // SDL.JoystickHasRumble or similar not found in current bindings.
      return false;
    }

    /// <summary>
    /// Sets the vibration motor strengths on the Joystick.
    /// </summary>
    /// <param name="left">Strength of the left motor (0.0 to 1.0).</param>
    /// <param name="right">Strength of the right motor (0.0 to 1.0).</param>
    /// <param name="durationSeconds">Duration of the rumble in seconds. LÖVE's API implies continuous until changed; use a long duration or 0 for infinite if SDL supports.</param>
    public void SetVibration(float left, float right, float durationSeconds = 1.0f)
    {
      if (this.disposed || !this.IsVibrationSupported())
      {
        return;
      }

      ushort leftStrength = (ushort)(Math.Clamp(left, 0f, 1f) * 65535);
      ushort rightStrength = (ushort)(Math.Clamp(right, 0f, 1f) * 65535);
      _ = (uint)(Math.Max(0, durationSeconds) * 1000);

      // SDL_RumbleJoystick: 0 duration means play for 0ms (i.e. stop).
      // To make it "continuous until changed" like LÖVE, we might need to manage this.
      // For now, a call to SetVibration(0,0) will stop it.
      // If LÖVE implies it stays on, a very large duration could be used,
      // but SDL_RumbleJoystick might not support "infinite".
      // Let's use the provided duration, or a default if not specified by LÖVE's direct equivalent.
      // LÖVE's love.joystick.setVibration() does not take duration. It's a state.
      // So, if left/right are > 0, we rumble for a "long time", if 0,0 we stop.
      if (leftStrength == 0 && rightStrength == 0)
      {
        _ = SDL.RumbleJoystick(this.joystickDevicePtr, 0, 0, 0); // Stop rumble
      }
      else
      {
        // Use a long duration to simulate continuous rumble until next SetVibration call
        _ = SDL.RumbleJoystick(this.joystickDevicePtr, (short)leftStrength, (short)rightStrength, 30000); // 30 seconds, effectively "on"
      }
    }

    /// <summary>
    /// Releases resources used by the Joystick object.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Sets the connected state. Internal use by Joysticks class.
    /// </summary>
    /// <param name="connected">True if connected, false otherwise.</param>
    internal void SetConnectedState(bool connected)
    {
      this.isConnected = connected;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          // Dispose managed state (managed objects).
        }

        // Free unmanaged resources (unmanaged objects) and override a finalizer below.
        if (this.gamepadDevicePtr != IntPtr.Zero)
        {
          SDL.CloseGamepad(this.gamepadDevicePtr);
          this.gamepadDevicePtr = IntPtr.Zero;
        }

        if (this.joystickDevicePtr != IntPtr.Zero)
        {
          SDL.CloseJoystick(this.joystickDevicePtr);

          // _joystickDevicePtr is readonly, so cannot set to IntPtr.Zero here.
          // This is fine as _disposed flag handles access.
        }

        this.disposed = true;
        this.isConnected = false;
      }
    }

    private static float NormalizeAxisValue(short rawValue)
    {
      if (rawValue == 0)
      {
        return 0.0f;
      }
      else if (rawValue > 0)
      {
        return rawValue / 32767.0f;
      }
      else
      {
        // rawValue < 0
        return rawValue / 32768.0f;
      }
    }

    private static SDL.GamepadAxis MapNightGamepadAxisToSdl(GamepadAxis axis)
    {
      return axis switch
      {
        GamepadAxis.LeftX => SDL.GamepadAxis.LeftX,
        GamepadAxis.LeftY => SDL.GamepadAxis.LeftY,
        GamepadAxis.RightX => SDL.GamepadAxis.RightX,
        GamepadAxis.RightY => SDL.GamepadAxis.RightY,
        GamepadAxis.TriggerLeft => SDL.GamepadAxis.LeftTrigger,
        GamepadAxis.TriggerRight => SDL.GamepadAxis.RightTrigger,
        _ => SDL.GamepadAxis.Invalid,
      };
    }

    private static SDL.GamepadButton MapNightGamepadButtonToSdl(GamepadButton button)
    {
      return button switch
      {
        GamepadButton.A => SDL.GamepadButton.South,
        GamepadButton.B => SDL.GamepadButton.East,
        GamepadButton.X => SDL.GamepadButton.West,
        GamepadButton.Y => SDL.GamepadButton.North,
        GamepadButton.Back => SDL.GamepadButton.Back,
        GamepadButton.Guide => SDL.GamepadButton.Guide,
        GamepadButton.Start => SDL.GamepadButton.Start,
        GamepadButton.LeftStick => SDL.GamepadButton.LeftStick,
        GamepadButton.RightStick => SDL.GamepadButton.RightStick,
        GamepadButton.LeftShoulder => SDL.GamepadButton.LeftShoulder,
        GamepadButton.RightShoulder => SDL.GamepadButton.RightShoulder,
        GamepadButton.DPUp => SDL.GamepadButton.DPadUp,
        GamepadButton.DPDown => SDL.GamepadButton.DPadDown,
        GamepadButton.DPLeft => SDL.GamepadButton.DPadLeft,
        GamepadButton.DPRight => SDL.GamepadButton.DPadRight,
        _ => SDL.GamepadButton.Invalid,
      };
    }
  }
}
