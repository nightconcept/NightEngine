// <copyright file="FrameworkGetVersionTest.cs" company="Night Circle">
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

using Xunit;

namespace NightTest.Groups.Framework
{
  /// <summary>
  /// Tests the <see cref="Night.Framework.GetVersion()"/> method.
  /// </summary>
  public class Framework_GetVersionTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Framework.GetVersion";

    /// <inheritdoc/>
    public override string Description => "Tests that Night.Framework.GetVersion() returns the correct engine version.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Night.Framework.GetVersion() returned the correct version string.";

    /// <summary>
    /// Executes the test logic for GetVersion.
    /// </summary>
    public override void Run()
    {
      // Arrange
      string expectedVersion = VersionInfo.GetVersion();

      // Act
      string actualVersion = Night.Framework.GetVersion();

      // Assert
      Assert.Equal(expectedVersion, actualVersion);
    }
  }
}
