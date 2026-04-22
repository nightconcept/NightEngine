// <copyright file="SystemGetOSTest.cs" company="Night Circle">
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

using System.Runtime.InteropServices; // For RuntimeInformation

using NightTest.Core;

using SDL3;

using Xunit;

namespace NightTest.Groups.SystemTests
{
  /// <summary>
  /// Tests the <see cref="Night.System.GetOS()"/> method.
  /// </summary>
  public class SystemGetOS_ReturnsCorrectPlatformStringTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "System.GetOS.ReturnsCorrectPlatformString";

    /// <inheritdoc/>
    public override string Description => "Tests that Night.System.GetOS() returns a platform string " +
                                         "consistent with .NET's RuntimeInformation and LÖVE API conventions.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Night.System.GetOS() returned a platform string " +
                                             "consistent with .NET's RuntimeInformation and LÖVE API conventions.";

    /// <inheritdoc/>
    public override void Run()
    {
      // Arrange
      string expectedOsString = string.Empty;

      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        expectedOsString = "Windows";
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        expectedOsString = "OS X";
      }

      // TODO: Implement when Android and iOS get supported by Night.
      /*       else if (RuntimeInformation.IsOSPlatform(OSPlatform.Android))
            {
              expectedOsString = "Android";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.IOS))
            {
              expectedOsString = "iOS";
            } */
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        expectedOsString = "Linux";
      }

      // Act
      string actualOsString = Night.System.GetOS();

      // Assert
      Assert.Equal(expectedOsString, actualOsString);
    }
  }
}
