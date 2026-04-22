// <copyright file="JoystickConnectionEventsTest.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

namespace NightTest.Groups.Joysticks
{
  /// <summary>
  /// Manual test case for verifying joystick connection and disconnection events.
  /// </summary>
  public class JoystickConnectionEventsTest : ManualTestCase
  {
    private TestState currentState = TestState.InitialPromptConnect;
    private string instructionText = string.Empty;
    private List<string> consoleMessages = new List<string>();
    private Joystick? lastConnectedJoystick;
    private Joystick? lastDisconnectedJoystick;

    private enum TestState
    {
      InitialPromptConnect,
      WaitingForConnect,
      ConnectVerifiedPromptDisconnect,
      WaitingForDisconnect,
      DisconnectVerifiedPromptPassFail,
      TestComplete,
    }

    /// <inheritdoc/>
    public override string Name => "Joysticks.ConnectionEvents";

    /// <inheritdoc/>
    public override string Description => "Manually tests JoystickAdded and JoystickRemoved events. " +
                                         "User will be prompted to connect and disconnect a joystick and verify console output.";

    /// <inheritdoc/>
    public override void JoystickAdded(Joystick joystick)
    {
      base.JoystickAdded(joystick);
      string msg = $"EVENT: Joystick ADDED - ID: {joystick.GetId()}, Name: '{joystick.GetName()}'";
      Console.WriteLine(msg);
      this.consoleMessages.Add(msg);
      this.lastConnectedJoystick = joystick;

      if (this.currentState == TestState.WaitingForConnect)
      {
        this.currentState = TestState.ConnectVerifiedPromptDisconnect;
        this.UpdateInstructionText();
      }
    }

    /// <inheritdoc/>
    public override void JoystickRemoved(Joystick joystick)
    {
      base.JoystickRemoved(joystick);

      // Note: joystick.IsConnected() will be false here.
      string msg = $"EVENT: Joystick REMOVED - ID: {joystick.GetId()}, Name: '{joystick.GetName()}'";
      Console.WriteLine(msg);
      this.consoleMessages.Add(msg);
      this.lastDisconnectedJoystick = joystick;

      if (this.currentState == TestState.WaitingForDisconnect)
      {
        this.currentState = TestState.DisconnectVerifiedPromptPassFail;
        this.UpdateInstructionText();
      }
    }

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      this.currentState = TestState.InitialPromptConnect;
      this.UpdateInstructionText();
      this.consoleMessages.Clear();
      this.lastConnectedJoystick = null;
      this.lastDisconnectedJoystick = null;

      // Ensure the window is a reasonable size for instructions
      _ = Window.SetMode(800, 600, 0); // Added flags argument
      Window.SetTitle(this.Name);
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone || this.currentState == TestState.TestComplete)
      {
        return;
      }

      // This test relies on user actions (plugging/unplugging joystick)
      // and then confirming observations. The main logic is in event handlers
      // and the final RequestManualConfirmation.

      // If we are in a state waiting for user to confirm via UI buttons
      if (this.currentState == TestState.DisconnectVerifiedPromptPassFail)
      {
        // RequestManualConfirmation handles its own timing and activation logic.
        // It will only show the prompt once after ManualTestPromptDelayMilliseconds.
        this.RequestManualConfirmation("Did the console correctly log joystick ADD and REMOVE events with details? Check counts and names.");
      }
    }

    /// <inheritdoc/>
    protected override void Draw()
    {
      Night.Graphics.Clear(new Color(30, 30, 30)); // Dark grey background

      // Instruction text will be shown in the console.
      // The Pass/Fail buttons are drawn by the ManualTestCase base class.
      // No additional in-window text rendering will be done here as Night.Graphics.Print/DrawString is not available.
    }

    /// <inheritdoc/>
    protected override void EndTest()
    {
      this.currentState = TestState.TestComplete;
      this.UpdateInstructionText(); // Update text to show final status
      base.EndTest();
    }

    private void UpdateInstructionText()
    {
      switch (this.currentState)
      {
        case TestState.InitialPromptConnect:
          this.instructionText = "Please CONNECT a joystick/gamepad now.\n" +
                                 "The test will proceed automatically upon detection.";
          this.currentState = TestState.WaitingForConnect; // Move to waiting state
          break;
        case TestState.WaitingForConnect:
          // This state is mostly for internal logic; instruction was set in InitialPromptConnect
          this.instructionText = "WAITING for joystick connection...";
          break;
        case TestState.ConnectVerifiedPromptDisconnect:
          this.instructionText = $"Joystick '{this.lastConnectedJoystick?.GetName()}' (ID: {this.lastConnectedJoystick?.GetId()}) connected.\n" +
                                 "VERIFY console output for ADDED event details.\n\n" +
                                 "Now, please DISCONNECT the joystick.\n" +
                                 "The test will proceed automatically upon detection.";
          this.currentState = TestState.WaitingForDisconnect; // Move to waiting state
          break;
        case TestState.WaitingForDisconnect:
          this.instructionText = $"WAITING for joystick '{this.lastConnectedJoystick?.GetName()}' to be disconnected...";
          break;
        case TestState.DisconnectVerifiedPromptPassFail:
          this.instructionText = $"Joystick '{this.lastDisconnectedJoystick?.GetName()}' (ID: {this.lastDisconnectedJoystick?.GetId()}) disconnected.\n" +
                                 "VERIFY console output for REMOVED event details.\n\n" +
                                 "If all console logs were correct, click PASS. Otherwise, click FAIL.";

          // The RequestManualConfirmation will be called in Update()
          break;
        case TestState.TestComplete:
          this.instructionText = "Test complete. Status: " + this.CurrentStatus;
          break;
        default:
          this.instructionText = "Unknown test state.";
          break;
      }

      Console.WriteLine(this.instructionText);
    }
  }
}
