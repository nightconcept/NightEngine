// <copyright file="GetTimeTest.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

namespace NightTest.Groups.Timer
{
  /// <summary>
  /// Tests the Timer.GetTime() method.
  /// </summary>
  public class GetTimeTest : GameTestCase
  {
    private double startTime = 0;
    private double endTime = 0;

    /// <inheritdoc/>
    public override string Name => "Timer.GetTime";

    /// <inheritdoc/>
    public override string Description => "Tests the Night.Timer.GetTime() method by measuring time passage.";

    /// <inheritdoc/>
    protected override void Load()
    {
      this.startTime = Night.Timer.GetTime();
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      // The IsDone check is handled by GameTestCase.Update before calling this.
      _ = this.CheckCompletionAfterDuration(
        500,
        successCondition: () =>
        {
          this.endTime = Night.Timer.GetTime();
          return true; // Condition for passing is simply reaching the duration
        },
        passDetails: () => // Use a lambda to construct details with captured values
        {
          double elapsed = this.endTime - this.startTime;
          return $"Timer.GetTime() test completed. Start: {this.startTime:F6}s, End: {this.endTime:F6}s. Elapsed: {elapsed:F6}s (Expected ~0.5s).";
        });
    }

    // Draw() override removed, will use empty GameTestCase.Draw()
  }
}
