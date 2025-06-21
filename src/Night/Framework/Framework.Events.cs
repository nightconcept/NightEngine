// <copyright file="Framework.Events.cs" company="Night Circle">
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

// using Night.Joysticks; // This was incorrect as Joysticks is a static class, types are in Night namespace.
using System.Runtime.InteropServices;

using SDL3;

namespace Night
{
  /// <summary>
  /// Provides the core framework functionalities, including the main game loop and event processing.
  /// This partial class specifically handles SDL event processing.
  /// </summary>
  public static partial class Framework
  {
    private static void ProcessSdlEvents(IGame game)
    {
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
        else if (eventType == SDL.EventType.JoystickAdded)
        {
          Logger.Info($"SDL_JOYSTICKADDED event: Joystick instance ID {e.JDevice.Which}");
          Joystick? newJoystick = Night.Joysticks.AddJoystick(e.JDevice.Which);
          if (newJoystick != null)
          {
            try
            {
              game.JoystickAdded(newJoystick);
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
          }
          else
          {
            Logger.Warn($"Failed to add joystick with instance ID {e.JDevice.Which} via Joysticks.AddJoystick.");
          }
        }
        else if (eventType == SDL.EventType.JoystickRemoved)
        {
          Logger.Info($"SDL_JOYSTICKREMOVED event: Joystick instance ID {e.JDevice.Which}");
          Joystick? removedJoystick = Night.Joysticks.RemoveJoystick(e.JDevice.Which);
          if (removedJoystick != null)
          {
            try
            {
              game.JoystickRemoved(removedJoystick);
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
            finally
            {
              removedJoystick.Dispose(); // Ensure joystick is disposed after event callback
            }
          }
          else
          {
            Logger.Warn($"Failed to remove joystick with instance ID {e.JDevice.Which} via Joysticks.RemoveJoystick (it might have already been removed or was never fully added).");
          }
        }
        else if (eventType == SDL.EventType.JoystickAxisMotion)
        {
          Logger.Debug($"SDL_JOYSTICKAXISMOTION event: Joystick {e.JAxis.Which}, Axis {e.JAxis.Axis}, Value {e.JAxis.Value}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.JAxis.Which);
          if (joystick != null)
          {
            try
            {
              float normalizedValue = NormalizeSdlAxisValue(e.JAxis.Value);
              game.JoystickAxis(joystick, (int)e.JAxis.Axis, normalizedValue);
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
          }
          else
          {
            Logger.Warn($"Received JoystickAxisMotion for unknown joystick instance ID {e.JAxis.Which}");
          }
        }
        else if (eventType == SDL.EventType.JoystickButtonDown)
        {
          Logger.Debug($"SDL_JOYSTICKBUTTONDOWN event: Joystick {e.JButton.Which}, Button {e.JButton.Button}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.JButton.Which);
          if (joystick != null)
          {
            try
            {
              game.JoystickPressed(joystick, (int)e.JButton.Button);
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
          }
          else
          {
            Logger.Warn($"Received JoystickButtonDown for unknown joystick instance ID {e.JButton.Which}");
          }
        }
        else if (eventType == SDL.EventType.JoystickButtonUp)
        {
          Logger.Debug($"SDL_JOYSTICKBUTTONUP event: Joystick {e.JButton.Which}, Button {e.JButton.Button}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.JButton.Which);
          if (joystick != null)
          {
            try
            {
              game.JoystickReleased(joystick, (int)e.JButton.Button);
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
          }
          else
          {
            Logger.Warn($"Received JoystickButtonUp for unknown joystick instance ID {e.JButton.Which}");
          }
        }
        else if (eventType == SDL.EventType.JoystickHatMotion)
        {
          Logger.Debug($"SDL_JOYSTICKHATMOTION event: Joystick {e.JHat.Which}, Hat {e.JHat.Hat}, Value {(JoystickHat)e.JHat.Value}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.JHat.Which);
          if (joystick != null)
          {
            try
            {
              game.JoystickHat(joystick, (int)e.JHat.Hat, (JoystickHat)e.JHat.Value);
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
          }
          else
          {
            Logger.Warn($"Received JoystickHatMotion for unknown joystick instance ID {e.JHat.Which}");
          }
        }
        else if (eventType == SDL.EventType.GamepadAxisMotion)
        {
          Logger.Debug($"SDL_GAMEPADAXISMOTION event: Joystick {e.GAxis.Which}, Axis {(SDL.GamepadAxis)e.GAxis.Axis}, Value {e.GAxis.Value}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.GAxis.Which);
          if (joystick != null)
          {
            if (joystick.IsGamepad())
            {
              try
              {
                float normalizedValue = NormalizeSdlAxisValue(e.GAxis.Value);
                Night.GamepadAxis nightAxis = MapSdlGamepadAxisToNight((SDL.GamepadAxis)e.GAxis.Axis);
                game.GamepadAxis(joystick, nightAxis, normalizedValue);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
          }
          else
          {
            Logger.Warn($"Received GamepadAxisMotion for unknown joystick instance ID {e.GAxis.Which}");
          }
        }
        else if (eventType == SDL.EventType.GamepadButtonDown)
        {
          Logger.Debug($"SDL_GAMEPADBUTTONDOWN event: Joystick {e.GButton.Which}, Button {(SDL.GamepadButton)e.GButton.Button}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.GButton.Which);
          if (joystick != null)
          {
            if (joystick.IsGamepad())
            {
              try
              {
                Night.GamepadButton nightButton = MapSdlGamepadButtonToNight((SDL.GamepadButton)e.GButton.Button);
                game.GamepadPressed(joystick, nightButton);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
          }
          else
          {
            Logger.Warn($"Received GamepadButtonDown for unknown joystick instance ID {e.GButton.Which}");
          }
        }
        else if (eventType == SDL.EventType.GamepadButtonUp)
        {
          Logger.Debug($"SDL_GAMEPADBUTTONUP event: Joystick {e.GButton.Which}, Button {(SDL.GamepadButton)e.GButton.Button}");
          Joystick? joystick = Night.Joysticks.GetJoystickByInstanceId(e.GButton.Which);
          if (joystick != null)
          {
            if (joystick.IsGamepad())
            {
              try
              {
                Night.GamepadButton nightButton = MapSdlGamepadButtonToNight((SDL.GamepadButton)e.GButton.Button);
                game.GamepadReleased(joystick, nightButton);
              }
              catch (Exception exUser)
              {
                HandleGameException(exUser, game);
              }
            }
          }
          else
          {
            Logger.Warn($"Received GamepadButtonUp for unknown joystick instance ID {e.GButton.Which}");
          }
        }
        else if (eventType == SDL.EventType.DropFile)
        {
          Logger.Debug($"SDL_DROPFILE event: Window {e.Drop.WindowID}");
          if (e.Drop.Data != IntPtr.Zero)
          {
            try
            {
              var path = Marshal.PtrToStringUTF8(e.Drop.Data);
              if (path != null)
              {
                game.FileDropped(new DroppedFile(path));
              }
            }
            catch (Exception exUser)
            {
              HandleGameException(exUser, game);
            }
            finally
            {
              // TODO: Free the data associated with the drop event.
              // The SDL documentation for SDL_DropEvent states that the `file` member
              // (e.Drop.Data) must be freed by the caller using SDL_free.
              // The current version of the SDL3-CS wrapper does not expose a public
              // method to call SDL_free. This will result in a small memory leak
              // for each dropped file. This should be addressed when the wrapper is updated.
              // SDL.free(e.Drop.Data);
            }
          }
        }

        // Other Gamepad event handling will be added here in later phases
      }
    }

    private static float NormalizeSdlAxisValue(short value)
    {
      // SDL axis values range from -32768 (SDL_JOYSTICK_AXIS_MIN) to 32767 (SDL_JOYSTICK_AXIS_MAX).
      // We want to normalize this to -1.0f to 1.0f.
      if (value == 0)
      {
        return 0.0f;
      }
      else if (value == -32768)
      {
        return -1.0f;
      }

      // For positive values, max is 32767. For negative, min is -32768 (already handled).
      // So, for positive values, divide by 32767.0f.
      // For negative values (excluding -32768), divide by 32768.0f to maintain symmetry around 0.
      return value < 0 ? value / 32768.0f : value / 32767.0f;
    }

    private static Night.GamepadAxis MapSdlGamepadAxisToNight(SDL.GamepadAxis sdlAxis)
    {
      switch (sdlAxis)
      {
        case SDL.GamepadAxis.LeftX:
          return Night.GamepadAxis.LeftX;
        case SDL.GamepadAxis.LeftY:
          return Night.GamepadAxis.LeftY;
        case SDL.GamepadAxis.RightX:
          return Night.GamepadAxis.RightX;
        case SDL.GamepadAxis.RightY:
          return Night.GamepadAxis.RightY;
        case SDL.GamepadAxis.LeftTrigger:
          return Night.GamepadAxis.TriggerLeft;
        case SDL.GamepadAxis.RightTrigger:
          return Night.GamepadAxis.TriggerRight;
        default:
          Logger.Warn($"Unknown SDL.GamepadAxis: {sdlAxis}. Defaulting to LeftX.");
          return Night.GamepadAxis.LeftX; // Or throw an exception
      }
    }

    private static Night.GamepadButton MapSdlGamepadButtonToNight(SDL.GamepadButton sdlButton)
    {
      switch (sdlButton)
      {
        case SDL.GamepadButton.South: // A
          return Night.GamepadButton.A;
        case SDL.GamepadButton.East: // B
          return Night.GamepadButton.B;
        case SDL.GamepadButton.West: // X
          return Night.GamepadButton.X;
        case SDL.GamepadButton.North: // Y
          return Night.GamepadButton.Y;
        case SDL.GamepadButton.Back:
          return Night.GamepadButton.Back;
        case SDL.GamepadButton.Guide:
          return Night.GamepadButton.Guide;
        case SDL.GamepadButton.Start:
          return Night.GamepadButton.Start;
        case SDL.GamepadButton.LeftStick:
          return Night.GamepadButton.LeftStick;
        case SDL.GamepadButton.RightStick:
          return Night.GamepadButton.RightStick;
        case SDL.GamepadButton.LeftShoulder:
          return Night.GamepadButton.LeftShoulder;
        case SDL.GamepadButton.RightShoulder:
          return Night.GamepadButton.RightShoulder;
        case SDL.GamepadButton.DPadUp:
          return Night.GamepadButton.DPUp;
        case SDL.GamepadButton.DPadDown:
          return Night.GamepadButton.DPDown;
        case SDL.GamepadButton.DPadLeft:
          return Night.GamepadButton.DPLeft;
        case SDL.GamepadButton.DPadRight:
          return Night.GamepadButton.DPRight;

        // SDL.GamepadButton.Misc1, Paddle1, Paddle2, Paddle3, Paddle4, Touchpad are not in Night.GamepadButton
        default:
          Logger.Warn($"Unknown SDL.GamepadButton: {sdlButton}. Defaulting to A.");
          return Night.GamepadButton.A; // Or throw an exception
      }
    }
  }
}
