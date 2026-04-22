// <copyright file="StepTest.cs" company="Night Circle">
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

using System.Collections.Generic;
using System.Linq;

using Night;

using NightTest.Core;

namespace NightTest.Groups.Timer
{
  /// <summary>
  /// Tests the Timer.Step() method.
  /// </summary>
  public class StepTest : GameTestCase
  {
    private int stepCount = 0;
    private List<double> stepDeltas = new List<double>();

    /// <inheritdoc/>
    public override string Name => "Timer.Step";

    /// <inheritdoc/>
    public override string Description => "Tests the Night.Timer.Step() method by calling it multiple times and observing delta values.";

    /// <inheritdoc/>
    protected override void Load()
    {
      this.stepCount = 0;
      this.stepDeltas.Clear();

      // Framework calls Timer.Initialize(), which sets LastStepTime.
      // Then Framework calls Timer.Step() before the first Update.
      // So, the first Night.Timer.Step() in Update here should give a small delta.
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      // The framework has already called Timer.Step() and the result is in `deltaTime` / Timer.GetDelta().
      // To test Timer.Step() somewhat independently, we can call it again.
      // Note: The *first* call to Timer.Step() in an application's lifetime (or after a long pause)
      // might have a larger delta if LastStepTime was zero or very old.
      // Timer.Initialize() sets LastStepTime, and framework calls Step() before first Update.
      double directStepDelta = Night.Timer.Step(); // Call it directly to get its current calculation
      this.stepDeltas.Add(directStepDelta);
      this.stepCount++; // Still need _stepCount for the number of direct calls.

      _ = this.CheckCompletionAfterDuration(
        201,
        successCondition: () => this.stepCount > 10,
        passDetails: () =>
        {
          double averageDirectStepDelta = this.stepDeltas.Count > 0 ? this.stepDeltas.Average() : 0.0;
          return $"Timer.Step() called {this.stepCount} times directly. Average delta from these calls: {averageDirectStepDelta:F6}. Test ran for >200ms.";
        },
        failDetailsTimeout: null,
        failDetailsCondition: () => "Timer.Step() test failed: Did not make >10 direct calls within 200ms.");
    }
  }
}
