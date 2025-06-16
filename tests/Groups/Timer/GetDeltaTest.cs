// <copyright file="GetDeltaTest.cs" company="Night Circle">
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
  /// Tests the Timer.GetDelta() method.
  /// </summary>
  public class GetDeltaTest : GameTestCase
  {
    private List<float> deltas = new List<float>();

    /// <inheritdoc/>
    public override string Name => "Timer.GetDelta";

    /// <inheritdoc/>
    public override string Description => "Tests the Night.Timer.GetDelta() method by collecting delta values.";

    /// <inheritdoc/>
    protected override void Load()
    {
      this.deltas.Clear();
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      this.deltas.Add(Night.Timer.GetDelta()); // Collect delta each frame this update is called

      _ = this.CheckCompletionAfterDuration(
        201, // > 200ms
        successCondition: () => this.CurrentFrameCount > 10,
        passDetails: () =>
        {
          float averageDelta = this.deltas.Count > 0 ? this.deltas.Average() : 0f;
          return $"Timer.GetDelta() test collected {this.deltas.Count} values. Average delta from Timer.GetDelta(): {averageDelta:F6}. Test ran for >200ms and >10 frames.";
        },
        failDetailsTimeout: null,
        failDetailsCondition: () => "Timer.GetDelta() test failed: Did not exceed 10 frames collecting deltas within 200ms.");
    }
  }
}
