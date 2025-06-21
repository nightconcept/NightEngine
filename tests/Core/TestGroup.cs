// <copyright file="TestGroup.cs" company="Night Circle">
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

using Xunit;
using Xunit.Abstractions;

namespace NightTest.Core
{
  /// <summary>
  /// Base class for grouping xUnit tests for IGame test cases.
  /// </summary>
  public class TestGroup
  {
    private readonly ITestOutputHelper outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper for logging.</param>
    public TestGroup(ITestOutputHelper outputHelper)
    {
      this.outputHelper = outputHelper;

      // Clear any sinks from previous test runs and add our custom xUnit sink.
      LogManager.ClearSinks();
      LogManager.AddSink(new XUnitLogSink(this.outputHelper));
    }

    /// <summary>
    /// Run the IGame test case.
    /// </summary>
    /// <param name="testCase">The test case to run.</param>
    public void Run_GameTestCase(GameTestCase testCase)
    {
      Assert.NotNull(testCase);

      this.outputHelper.WriteLine($"Starting IGame test: {testCase.Name}");
      this.outputHelper.WriteLine($"  Description: {testCase.Description}");
      this.outputHelper.WriteLine($"  Type: {testCase.Type}");

      try
      {
        Night.Framework.Run(testCase);
      }
      catch (Exception ex)
      {
        // If Night.Framework.Run throws before testCase.Load() or if testCase itself is problematic early,
        // the Assert.NotNull would have already caught a null testCase argument.
        // If the exception happens *during* testCase execution, testCase is still the same valid object.
        this.outputHelper.WriteLine($"IGame test '{testCase.Name}' threw an unhandled exception: {ex.Message}\n{ex.StackTrace}");
        testCase.RecordFailure($"Unhandled exception: {ex.Message}", ex);
      }

      this.outputHelper.WriteLine($"IGame test '{testCase.Name}' completed.");
      this.outputHelper.WriteLine($"  Status: {testCase.CurrentStatus}");
      this.outputHelper.WriteLine($"  Details: {testCase.Details}");
      this.outputHelper.WriteLine($"  Duration: {testCase.TestStopwatch.ElapsedMilliseconds}ms");

      Assert.Equal(TestStatus.Passed, testCase.CurrentStatus);
    }

    /// <summary>
    /// Run the mod test case.
    /// </summary>
    /// <param name="testCase">The mod test case to run.</param>
    public void Run_ModTestCase(ModTestCase testCase)
    {
      Assert.NotNull(testCase);

      this.outputHelper.WriteLine($"Starting mod test: {testCase.Name}");
      this.outputHelper.WriteLine($"  Description: {testCase.Description}");
      this.outputHelper.WriteLine($"  Type: {testCase.Type}");

      testCase.PrepareForRun();
      try
      {
        testCase.Run();
        testCase.RecordSuccess(testCase.SuccessMessage);
      }
      catch (Exception ex)
      {
        this.outputHelper.WriteLine($"Test '{testCase.Name}' threw an exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        testCase.RecordFailure($"Test execution failed: {ex.Message}", ex);
        throw;
      }
      finally
      {
        testCase.FinalizeRun();
      }

      this.outputHelper.WriteLine($"Mod test '{testCase.Name}' completed.");
      this.outputHelper.WriteLine($"  Status: {testCase.CurrentStatus}");
      this.outputHelper.WriteLine($"  Details: {testCase.Details}");
      this.outputHelper.WriteLine($"  Duration: {testCase.TestStopwatch.ElapsedMilliseconds}ms");

      Assert.Equal(TestStatus.Passed, testCase.CurrentStatus);
    }
  }
}
