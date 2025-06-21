// <copyright file="GetFPSTest.cs" company="Night Circle">
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
  /// Tests the Timer.GetFPS() method.
  /// </summary>
  public class GetFPSTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Timer.GetFPS";

    /// <inheritdoc/>
    public override string Description => "Tests the Night.Timer.GetFPS() method by observing its value over a short period.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      int finalFps = 0; // To capture FPS in the success condition

      _ = this.CheckCompletionAfterDuration(
        201, // > 200ms
        successCondition: () =>
        {
          if (this.CurrentFrameCount > 10)
          {
            finalFps = Night.Timer.GetFPS(); // Get FPS when conditions are met
            return true;
          }

          return false;
        },
        passDetails: () => $"Timer.GetFPS() test observed. Last reported FPS: {finalFps}. Test ran for >200ms and >10 frames.",
        failDetailsTimeout: null,
        failDetailsCondition: () => "Timer.GetFPS() test failed: Did not exceed 10 frames within 200ms.");
    }
  }
}
