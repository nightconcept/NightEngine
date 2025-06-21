// <copyright file="ManualTestCase.cs" company="Night Circle">
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

using Night;

namespace NightTest.Core
{
  /// <summary>
  /// Abstract base class for manual test cases, providing common UI and interaction logic.
  /// Inherits from GameTestCase.
  /// </summary>
  public abstract class ManualTestCase : GameTestCase
  {
    // Constants
    private const int ButtonWidth = 120;
    private const int ButtonHeight = 50;
    private const int ButtonPadding = 20;
    private static readonly Color PassButtonColor = new Color(0, 180, 0); // Green
    private static readonly Color FailButtonColor = new Color(200, 0, 0); // Red
    private static readonly Color ButtonBorderColor = Color.White;

    // Private Fields

    /// <summary>
    /// The current UI mode for manual input.
    /// </summary>
    private ManualInputUIMode currentManualInputUIMode = ManualInputUIMode.None;

    /// <summary>
    /// A value indicating whether the confirmation prompt is currently active.
    /// </summary>
    private bool confirmationPromptActive;

    private Rectangle passButtonRect;
    private Rectangle failButtonRect;

    /// <summary>
    /// Defines the UI mode for manual test input confirmation.
    /// </summary>
    protected enum ManualInputUIMode
    {
      /// <summary>
      /// No manual input UI is active.
      /// </summary>
      None,

      /// <summary>
      /// The test is awaiting user confirmation via the UI (Pass/Fail buttons).
      /// </summary>
      AwaitingConfirmation,
    }

    /// <inheritdoc/>
    public override TestType Type => TestType.Manual;

    /// <summary>
    /// Gets the console prompt message displayed during manual confirmation.
    /// </summary>
    protected string ManualConfirmationConsolePrompt { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the timeout in milliseconds for manual tests awaiting confirmation. Defaults to 30 seconds.
    /// </summary>
    protected double ManualTestTimeoutMilliseconds { get; } = 30000;

    /// <summary>
    /// Gets the suggested delay in milliseconds before a manual test prompt is shown. Defaults to 200ms.
    /// Derived classes can use this value to time their call to RequestManualConfirmation.
    /// </summary>
    protected double ManualTestPromptDelayMilliseconds { get; } = 200;

    /// <inheritdoc/>
    public override void KeyPressed(KeySymbol key, KeyCode scancode, bool isRepeat)
    {
      base.KeyPressed(key, scancode, isRepeat);
      if (this.IsDone || isRepeat)
      {
        return;
      }

      // Handling for ESC key to fail manual tests during confirmation
      if (this.Type == TestType.Manual && this.currentManualInputUIMode == ManualInputUIMode.AwaitingConfirmation && scancode == KeyCode.Escape)
      {
        Console.WriteLine($"MANUAL TEST '{this.Name}': FAILED by user pressing ESCAPE.");
        this.CurrentStatus = TestStatus.Failed;
        this.Details = this.ManualConfirmationConsolePrompt + " - User pressed ESCAPE to fail.";
        this.EndTest();
        return; // Test is over
      }
    }

    /// <inheritdoc/>
    public override void MousePressed(int x, int y, MouseButton button, bool istouch, int presses)
    {
      base.MousePressed(x, y, button, istouch, presses);
      if (this.IsDone)
      {
        return;
      }

      if (this.currentManualInputUIMode == ManualInputUIMode.AwaitingConfirmation && button == MouseButton.Left && !istouch)
      {
        if (this.passButtonRect.Width > 0 && // Ensure buttons are initialized
            x >= this.passButtonRect.X && x <= this.passButtonRect.X + this.passButtonRect.Width &&
            y >= this.passButtonRect.Y && y <= this.passButtonRect.Y + this.passButtonRect.Height)
        {
          Console.WriteLine($"MANUAL TEST '{this.Name}': PASSED by user click.");
          this.CurrentStatus = TestStatus.Passed;
          this.Details = this.ManualConfirmationConsolePrompt + " - User confirmed: PASSED.";
          this.currentManualInputUIMode = ManualInputUIMode.None;
          this.EndTest();
        }
        else if (this.failButtonRect.Width > 0 && // Ensure buttons are initialized
                 x >= this.failButtonRect.X && x <= this.failButtonRect.X + this.failButtonRect.Width &&
                 y >= this.failButtonRect.Y && y <= this.failButtonRect.Y + this.failButtonRect.Height)
        {
          Console.WriteLine($"MANUAL TEST '{this.Name}': FAILED by user click.");
          this.CurrentStatus = TestStatus.Failed;
          this.Details = this.ManualConfirmationConsolePrompt + " - User confirmed: FAILED.";
          this.currentManualInputUIMode = ManualInputUIMode.None;
          this.EndTest();
        }
      }
    }

    /// <summary>
    /// Overrides the internal load hook from <see cref="GameTestCase"/>
    /// to inject manual test-specific initialization logic
    /// before allowing the concrete test's <see cref="GameTestCase.Load()"/> method to run.
    /// This method is sealed to ensure this control flow.
    /// </summary>
    protected sealed override void InternalLoad()
    {
      // Perform ManualTestCase specific initialization
      this.currentManualInputUIMode = ManualInputUIMode.None;
      this.ManualConfirmationConsolePrompt = string.Empty;
      this.confirmationPromptActive = false;

      // Call the base InternalLoad, which will in turn call the concrete test's Load() method.
      base.InternalLoad();
    }

    /// <summary>
    /// Overrides the internal update hook from <see cref="GameTestCase"/>
    /// to inject manual test-specific logic, such as timeout checks,
    /// before allowing the concrete test's <see cref="GameTestCase.Update(double)"/> method to run.
    /// This method is sealed to ensure this control flow.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last frame.</param>
    protected sealed override void InternalUpdate(double deltaTime)
    {
      // Handle timeout for manual tests
      if (this.Type == TestType.Manual && this.currentManualInputUIMode == ManualInputUIMode.AwaitingConfirmation)
      {
        if (this.TestStopwatch.ElapsedMilliseconds > this.ManualTestTimeoutMilliseconds)
        {
          Console.WriteLine($"MANUAL TEST '{this.Name}': Timed out after {this.ManualTestTimeoutMilliseconds / 1000} seconds.");
          this.CurrentStatus = TestStatus.Failed;
          this.Details = this.ManualConfirmationConsolePrompt + " - Test timed out.";
          this.EndTest();
          return; // Test is over
        }
      }

      // If not timed out or otherwise completed, call the base InternalUpdate,
      // which will in turn call the concrete test's Update() method.
      if (!this.IsDone)
      {
        base.InternalUpdate(deltaTime);
      }
    }

    /// <summary>
    /// Overrides the internal draw hook from <see cref="GameTestCase"/>
    /// to allow the concrete test's <see cref="GameTestCase.Draw()"/> method to run first,
    /// then draws manual test-specific UI elements (like Pass/Fail buttons),
    /// and finally calls <see cref="Night.Graphics.Present()"/>.
    /// This method is sealed to ensure this control flow.
    /// </summary>
    protected sealed override void InternalDraw()
    {
      // First, call the base InternalDraw, which will execute the concrete test's Draw() method.
      base.InternalDraw();

      // Then, draw ManualTestCase specific UI elements.
      if (this.currentManualInputUIMode == ManualInputUIMode.AwaitingConfirmation)
      {
        // Ensure button rects are calculated if window is valid
        if (this.passButtonRect.Width == 0 && Window.IsOpen())
        {
          var windowMode = Window.GetMode();
          if (windowMode.Width > 0 && windowMode.Height > 0)
          {
            this.CalculateButtonPositions(windowMode.Width, windowMode.Height);
          }
        }

        if (this.passButtonRect.Width > 0)
        {
          // Draw Pass Button (Green)
          Graphics.SetColor(PassButtonColor);
          Graphics.Rectangle(DrawMode.Fill, this.passButtonRect.X, this.passButtonRect.Y, this.passButtonRect.Width, this.passButtonRect.Height);
          Graphics.SetColor(ButtonBorderColor); // Border
          Graphics.Rectangle(DrawMode.Line, this.passButtonRect.X, this.passButtonRect.Y, this.passButtonRect.Width, this.passButtonRect.Height);

          // Draw Fail Button (Red)
          Graphics.SetColor(FailButtonColor);
          Graphics.Rectangle(DrawMode.Fill, this.failButtonRect.X, this.failButtonRect.Y, this.failButtonRect.Width, this.failButtonRect.Height);
          Graphics.SetColor(ButtonBorderColor); // Border
          Graphics.Rectangle(DrawMode.Line, this.failButtonRect.X, this.failButtonRect.Y, this.failButtonRect.Width, this.failButtonRect.Height);
        }
      }

      // Finally, present the graphics for all manual tests.
      Night.Graphics.Present();
    }

    // Note: The public override Draw() is removed as its logic is now in InternalDraw().

    /// <summary>
    /// Initiates a manual confirmation step for the test.
    /// Displays a prompt in the console and renders Pass/Fail buttons in the game window.
    /// </summary>
    /// <param name="consolePrompt">The question or instruction to display in the console for the user.</param>
    protected void RequestManualConfirmation(string consolePrompt)
    {
      if (this.Type != TestType.Manual)
      {
        Console.WriteLine($"Warning: RequestManualConfirmation called for a non-manual test: {this.Name}. Ignoring.");
        return;
      }

      if (!this.confirmationPromptActive && this.TestStopwatch.ElapsedMilliseconds > this.ManualTestPromptDelayMilliseconds)
      {
        this.confirmationPromptActive = true;
        this.ManualConfirmationConsolePrompt = consolePrompt;
        this.currentManualInputUIMode = ManualInputUIMode.AwaitingConfirmation;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n--- MANUAL CONFIRMATION REQUIRED for test: '{this.Name}' ---");
        Console.ResetColor();
        Console.WriteLine(this.ManualConfirmationConsolePrompt);
        Console.WriteLine("Please observe the game window. Click the GREEN box to PASS, or the RED box to FAIL.");
        Console.WriteLine("(Alternatively, press ESCAPE to fail and quit this specific test.)");

        // Attempt to calculate button positions immediately if window is available
        if (Window.IsOpen())
        {
          var windowMode = Window.GetMode();
          if (windowMode.Width > 0 && windowMode.Height > 0)
          {
            this.CalculateButtonPositions(windowMode.Width, windowMode.Height);
          }
        }
      }
    }

    /// <inheritdoc/>
    protected override void EndTest()
    {
      if (this.IsDone)
      {
        return;
      }

      // If a manual test is quit externally (e.g. ESC key in test case) before confirmation,
      // and status hasn't been set by button click, mark as failed.
      if (this.Type == TestType.Manual && this.currentManualInputUIMode == ManualInputUIMode.AwaitingConfirmation && this.CurrentStatus == TestStatus.NotRun)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = this.ManualConfirmationConsolePrompt + " - Test quit prematurely by user before confirmation.";
        Console.WriteLine($"MANUAL TEST '{this.Name}': Test quit prematurely. Marked as FAILED.");
      }

      this.currentManualInputUIMode = ManualInputUIMode.None; // Ensure this is reset before base call

      base.EndTest();
    }

    // Private Methods
    private void CalculateButtonPositions(int windowWidth, int windowHeight)
    {
      int totalButtonsWidth = (ButtonWidth * 2) + ButtonPadding;
      int startX = (windowWidth - totalButtonsWidth) / 2;
      if (startX < ButtonPadding)
      {
        startX = ButtonPadding; // Ensure buttons are not off-screen left
      }

      int buttonY = windowHeight - ButtonHeight - ButtonPadding;
      if (buttonY < ButtonPadding)
      {
        buttonY = ButtonPadding; // Ensure buttons are not off-screen bottom
      }

      this.passButtonRect = new Rectangle(startX, buttonY, ButtonWidth, ButtonHeight);
      this.failButtonRect = new Rectangle(startX + ButtonWidth + ButtonPadding, buttonY, ButtonWidth, ButtonHeight);
    }
  }
}
