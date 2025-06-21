// <copyright file="SleepTest.cs" company="Night Circle">
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
using System.Diagnostics;

using Night;

using NightTest.Core;

namespace NightTest.Groups.Timer
{
  /// <summary>
  /// Tests the Timer.Sleep() method.
  /// </summary>
  public class SleepTest : GameTestCase
  {
    private const double SleepDurationSeconds = 0.25; // Sleep for 250ms

    // Using TestStopwatch from base class for overall test duration
    // Need a separate stopwatch for measuring sleep itself
    private Stopwatch internalStopwatch = new Stopwatch();

    /// <inheritdoc/>
    public override string Name => "Timer.Sleep";

    /// <inheritdoc/>
    public override string Description => $"Tests the Night.Timer.Sleep() method by sleeping for {SleepDurationSeconds}s.";

    /// <inheritdoc/>
    protected override void Load()
    {
      this.internalStopwatch.Reset();
      this.internalStopwatch.Start();
      Night.Timer.Sleep(SleepDurationSeconds);
      this.internalStopwatch.Stop();

      double elapsedMs = this.internalStopwatch.ElapsedMilliseconds;

      if (elapsedMs >= SleepDurationSeconds * 1000 * 0.9 && elapsedMs <= SleepDurationSeconds * 1000 * 1.6)
      {
        this.CurrentStatus = TestStatus.Passed;
        this.Details = $"Timer.Sleep({SleepDurationSeconds}) executed. Measured duration: {elapsedMs}ms. Expected ~{SleepDurationSeconds * 1000}ms.";
      }
      else
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Timer.Sleep({SleepDurationSeconds}) executed. Measured duration: {elapsedMs}ms. Expected ~{SleepDurationSeconds * 1000}ms. Deviation too large.";
      }

      // Since this test completes its logic here, call EndTest immediately.
      // TestStopwatch started by IGame.Load will correctly measure the duration of this Load phase.
      this.EndTest();
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (!this.IsDone)
      {
        this.Details = "Test did not complete in Load as expected.";
        this.CurrentStatus = TestStatus.Failed;
        this.EndTest(); // Ensure it quits if it somehow reaches here and isn't done.
      }
    }
  }

  /// <summary>
  /// Tests the Timer.Sleep(seconds) method where time sleep is less than 0.
  /// </summary>
  public class SleepTest_EarlyReturn : GameTestCase
  {
    private const double SleepDurationSeconds = -1; // Invalid time
    private const double ExpectedSleepDurationMs = 0.001; // Expect no sleep

    // Using TestStopwatch from base class for overall test duration
    // Need a separate stopwatch for measuring sleep itself
    private Stopwatch internalStopwatch = new Stopwatch();

    /// <inheritdoc/>
    public override string Name => "Timer.Sleep";

    /// <inheritdoc/>
    public override string Description => $"Tests the Night.Timer.Sleep() method by sleeping for {SleepDurationSeconds}s.";

    /// <inheritdoc/>
    protected override void Load()
    {
      this.internalStopwatch.Reset();
      this.internalStopwatch.Start();
      Night.Timer.Sleep(SleepDurationSeconds);
      this.internalStopwatch.Stop();

      double elapsedMs = this.internalStopwatch.ElapsedMilliseconds;

      if (elapsedMs <= ExpectedSleepDurationMs)
      {
        this.CurrentStatus = TestStatus.Passed;
        this.Details = $"Timer.Sleep({SleepDurationSeconds}) executed. Measured duration: {elapsedMs}ms. Expected less than {SleepDurationSeconds * 1000}ms.";
      }
      else
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Timer.Sleep({SleepDurationSeconds}) executed. Measured duration: {elapsedMs}ms. Expected less than {SleepDurationSeconds * 1000}ms.";
      }

      // Since this test completes its logic here, call EndTest immediately.
      // TestStopwatch started by IGame.Load will correctly measure the duration of this Load phase.
      this.EndTest();
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (!this.IsDone)
      {
        this.Details = "Test did not complete in Load as expected.";
        this.CurrentStatus = TestStatus.Failed;
        this.EndTest(); // Ensure it quits if it somehow reaches here and isn't done.
      }
    }
  }
}
