// <copyright file="CLIGroup.cs" company="Night Circle">
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
  /// Test group for CLI-related ModTestCases.
  /// </summary>
  [Collection("SequentialTests")] // Recommended for ModTestCases as well if they modify global state (like LogManager)
  public class CLIGroup : TestGroup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CLIGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public CLIGroup(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Runs the test for CLI constructor default values.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_DefaultValuesTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_DefaultValuesTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor silent mode flags.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_SilentModeTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_SilentModeTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor --log-level argument.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_LogLevelTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_LogLevelTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor --debug flag.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_DebugModeTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_DebugModeTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor --session-log flag.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_SessionLogTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_SessionLogTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor --force-graphics flag.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_ForceHardwareRenderTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_ForceHardwareRenderTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor remaining arguments handling.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_RemainingArgsTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_RemainingArgsTest());
    }

    /// <summary>
    /// Runs the test for CLI constructor combined arguments parsing.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_Constructor_CombinedArgsTest()
    {
      this.Run_ModTestCase(new NightCLI_Constructor_CombinedArgsTest());
    }

    /// <summary>
    /// Runs the test for CLI ApplySettings method's effect on log level.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_ApplySettings_LogLevelTest()
    {
      this.Run_ModTestCase(new NightCLI_ApplySettings_LogLevelTest());
    }

    /// <summary>
    /// Runs the test for CLI ApplySettings method's effect in debug mode.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_ApplySettings_DebugModeTest()
    {
      this.Run_ModTestCase(new NightCLI_ApplySettings_DebugModeTest());
    }

    /// <summary>
    /// Runs the test for CLI ApplySettings method with session logging (no crash).
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_ApplySettings_SessionLogNoCrashTest()
    {
      this.Run_ModTestCase(new NightCLI_ApplySettings_SessionLogNoCrashTest());
    }

    /// <summary>
    /// Runs the test for CLI ApplySettings method with remaining/invalid arguments (no crash).
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FrameworkCLI_ApplySettings_RemainingArgsWarningTest()
    {
      this.Run_ModTestCase(new NightCLI_ApplySettings_RemainingArgsWarningTest());
    }
  }
}
