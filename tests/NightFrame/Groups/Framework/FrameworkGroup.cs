// <copyright file="FrameworkGroup.cs" company="Night Circle">
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

namespace NightTest.Groups.Framework
{
  /// <summary>
  /// Test group for Framework-related ModTestCases.
  /// </summary>
  [Collection("SequentialTests")] // Recommended for ModTestCases as well if they modify global state (like LogManager)
  public class FrameworkGroup : TestGroup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FrameworkGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public FrameworkGroup(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Runs ModTestCases for the CLI feature within the Framework module.
    /// This includes tests for CLI constructor arguments (default values, silent mode, log level, debug mode, session log, remaining args, combined args)
    /// and ApplySettings method (log level, debug mode, session log, remaining args warning).
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_ModTests()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_DefaultValuesTest());
      this.Run_ModTestCase(new NightCLI_Constructor_SilentModeTest());
      this.Run_ModTestCase(new NightCLI_Constructor_LogLevelTest());
      this.Run_ModTestCase(new NightCLI_Constructor_DebugModeTest());
      this.Run_ModTestCase(new NightCLI_Constructor_SessionLogTest());
      this.Run_ModTestCase(new NightCLI_Constructor_RemainingArgsTest());
      this.Run_ModTestCase(new NightCLI_Constructor_CombinedArgsTest());
      this.Run_ModTestCase(new NightCLI_ApplySettings_LogLevelTest());
      this.Run_ModTestCase(new NightCLI_ApplySettings_DebugModeTest());
      this.Run_ModTestCase(new NightCLI_ApplySettings_SessionLogNoCrashTest());
      this.Run_ModTestCase(new NightCLI_ApplySettings_RemainingArgsWarningTest());
    }

    /// <summary>
    /// Runs ModTestCases for the Framework.GetVersion method.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkGetVersion_ModTests()
    {
      this.Run_ModTestCase(new Framework_GetVersionTest());
    }

    /// <summary>
    /// Runs ModTestCases for the Framework.Run method.
    /// This includes tests for handling null IGame instances.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkRun_ModTests()
    {
      this.Run_ModTestCase(new FrameworkRun_NullIGame());
    }
  }
}
