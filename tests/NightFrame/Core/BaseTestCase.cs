// <copyright file="BaseTestCase.cs" company="Night Circle">
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

namespace NightTest.Core
{
  /// <summary>
  /// Abstract base class for all test cases, providing common functionality.
  /// Implements ITestCase.
  /// </summary>
  public abstract class BaseTestCase : ITestCase
  {
    // Public Properties

    /// <summary>
    /// Gets the stopwatch used to measure the duration of the test case.
    /// </summary>
    public Stopwatch TestStopwatch { get; } = new Stopwatch();

    /// <summary>
    /// Gets or sets the current status of the test case.
    /// Its value can be asserted by xUnit test methods.
    /// </summary>
    public TestStatus CurrentStatus { get; protected set; } = TestStatus.NotRun;

    /// <summary>
    /// Gets or sets details about the test execution, such as error messages or success information.
    /// Its value can be asserted by xUnit test methods.
    /// </summary>
    public string Details { get; protected set; } = "Test has not started.";

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public virtual TestType Type => TestType.Automated;

    /// <inheritdoc/>
    public abstract string Description { get; }

    /// <summary>
    /// Public method to record a test failure.
    /// </summary>
    /// <param name="failureDetails">Specific details about the failure.</param>
    /// <param name="ex">The exception that caused the failure, if any.</param>
    public void RecordFailure(string failureDetails, Exception? ex = null)
    {
      this.CurrentStatus = TestStatus.Failed;
      if (ex != null)
      {
        this.Details = $"{failureDetails} - Exception: {ex.GetType().Name}: {ex.Message}";
      }
      else
      {
        this.Details = failureDetails;
      }
    }

    /// <summary>
    /// Public method to record a test success.
    /// </summary>
    /// <param name="successDetails">Specific details about the success.</param>
    public void RecordSuccess(string successDetails)
    {
      this.CurrentStatus = TestStatus.Passed;
      this.Details = successDetails;
    }
  }
}
