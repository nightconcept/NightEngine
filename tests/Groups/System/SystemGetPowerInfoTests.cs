// <copyright file="SystemGetPowerInfoTests.cs" company="Night Circle">
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

using System; // For Enum.IsDefined

using Night; // For Night.System and Night.PowerState

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.SystemTests
{
  /// <summary>
  /// Tests the <see cref="Night.System.GetPowerInfo()"/> method.
  /// </summary>
  public class SystemGetPowerInfo_ReturnsValidDataTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "System.GetPowerInfo.ReturnsValidData";

    /// <inheritdoc/>
    public override string Description => "Tests that Night.System.GetPowerInfo() returns a valid power state, " +
                                         "percentage (0-100 or null), and seconds remaining (non-negative or null).";

    /// <inheritdoc/>
    public override string SuccessMessage => "Night.System.GetPowerInfo() returned data in the expected valid format.";

    /// <inheritdoc/>
    public override void Run()
    {
      // Arrange & Act
      (PowerState State, int? Percent, int? Seconds) powerInfo = Night.System.GetPowerInfo();

      // Assert
      Assert.True(Enum.IsDefined(typeof(PowerState), powerInfo.State), $"Invalid PowerState returned: {powerInfo.State}");

      if (powerInfo.Percent.HasValue)
      {
        Assert.InRange(powerInfo.Percent.Value, 0, 100);
      }

      if (powerInfo.Seconds.HasValue)
      {
        Assert.True(powerInfo.Seconds.Value >= 0, $"Expected seconds to be >= 0 or null, but got {powerInfo.Seconds.Value}.");
      }

      // Additional check to ensure that if state is NoBattery, percent and seconds are likely null.
      // This is a reasonable expectation but not a strict guarantee from SDL.
      // The primary assertions above cover the validity of the values.
      if (powerInfo.State == PowerState.NoBattery)
      {
        // It's common for Percent and Seconds to be null if NoBattery is reported.
        // No direct assertion here as SDL behavior can vary; the core validity is already checked.
      }

      // Similar consideration for Unknown state.
      if (powerInfo.State == PowerState.Unknown)
      {
        // It's common for Percent and Seconds to be null if Unknown is reported.
      }
    }
  }
}
