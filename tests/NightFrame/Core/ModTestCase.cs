// <copyright file="ModTestCase.cs" company="Night Circle">
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
  /// Abstract mod test base class for test cases to reduce boilerplate.
  /// Implements ITestCase.
  /// Mod tests are closer in nature to unit tests and do not rely on a
  /// IGame instance to run.
  /// </summary>
  public abstract class ModTestCase : BaseTestCase
  {
    // Public Properties

    /// <summary>
    /// Gets the success message for the test.
    /// This message is used by the test runner if the Run() method completes without exceptions.
    /// </summary>
    public abstract string SuccessMessage { get; }

    /// <summary>
    /// When implemented by a derived class, contains the specific test logic and assertions.
    /// This method will be called by the test runner (e.g., TestGroup) and should not
    /// contain try-catch blocks for assertion failures or calls to RecordSuccess/RecordFailure.
    /// </summary>
    public abstract void Run();

    /// <summary>
    /// Public method to record a test success, typically called by an xUnit wrapper when an exception occurs.
    /// </summary>
    /// <param name="successDetails">Specific details about the success.</param>
    public new void RecordSuccess(string successDetails)
    {
      this.CurrentStatus = TestStatus.Passed;
      this.Details = successDetails;
    }

    /// <summary>
    /// Sets the initial state of the test before execution by the test runner.
    /// </summary>
    internal void PrepareForRun()
    {
      this.TestStopwatch.Restart();
      this.Details = "Test is preparing to run.";
    }

    /// <summary>
    /// Finalizes the test run, typically called by the test runner.
    /// </summary>
    internal void FinalizeRun()
    {
      this.TestStopwatch.Stop();
    }
  }
}
