// <copyright file="SystemGroup.cs" company="Night Circle">
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

using NightTest.Core;

using Xunit;
using Xunit.Abstractions;

namespace NightTest.Groups.SystemTests
{
  /// <summary>
  /// Test group for Night.System module.
  /// </summary>
  [Collection("SequentialTests")] // As per guidelines, though likely not strictly needed for only ModTestCases
  public class SystemGroup : TestGroup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper.</param>
    public SystemGroup(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Runs module tests for the Night.System functionality.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_System_ModTests()
    {
      this.Run_ModTestCase(new SystemGetOS_ReturnsCorrectPlatformStringTest());
      this.Run_ModTestCase(new SystemGetProcessorCount_ReturnsPositiveValueTest());
      this.Run_ModTestCase(new SystemGetPowerInfo_ReturnsValidDataTest());
    }
  }
}
