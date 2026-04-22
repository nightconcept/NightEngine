// <copyright file="SDLTests.cs" company="Night Circle">
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

using System.Text.RegularExpressions;

using Night;

using NightTest.Core;

namespace NightTest.Groups.SDL
{
  /// <summary>
  /// Tests that <see cref="NightSDL.GetVersion"/> returns a correctly formatted version string.
  /// </summary>
  public class NightSDL_GetVersionTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "NightSDL.GetVersion Format";

    /// <inheritdoc/>
    public override string Description => "Tests if NightSDL.GetVersion() returns a string in 'major.minor.patch' format.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      string version = NightSDL.GetVersion();
      bool isMatch = Regex.IsMatch(version, @"^\d+\.\d+\.\d+$");

      if (isMatch)
      {
        this.Details = $"Version string '{version}' is correctly formatted.";
        this.CurrentStatus = TestStatus.Passed;
      }
      else
      {
        this.Details = $"Version string '{version}' is NOT correctly formatted. Expected format: 'major.minor.patch'.";
        this.CurrentStatus = TestStatus.Failed;
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests that <see cref="NightSDL.GetError"/> returns an empty string when no SDL error has occurred.
  /// </summary>
  public class NightSDL_GetErrorTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "NightSDL.GetError No Error";

    /// <inheritdoc/>
    public override string Description => "Tests if NightSDL.GetError() returns an empty string when no SDL error is set.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      // The try-catch-finally for general exceptions and ensuring EndTest() is called
      // has been moved to GameTestCase.IGame.Update().

      // Ensure CurrentStatus is Running if not already set by Load,
      // though Load should have set it.
      // This specific check can remain if there's a concern it might still be NotRun
      // when Update is called, though the GameTestCase.Load() should prevent this.
      // For now, let's assume GameTestCase.Load() correctly sets it to Running.
      // if (this.CurrentStatus == TestStatus.NotRun)
      // {
      //     this.CurrentStatus = TestStatus.Running;
      //     this.Details = "Test execution started in Update...";
      // }
      _ = SDL3.SDL.ClearError(); // Ensure no pre-existing error
      string error = NightSDL.GetError();

      if (string.IsNullOrEmpty(error))
      {
        this.Details = "NightSDL.GetError() returned an empty string as expected.";
        this.CurrentStatus = TestStatus.Passed;
      }
      else
      {
        this.Details = $"NightSDL.GetError() returned '{error}' but an empty string was expected after SDL.ClearError().";
        this.CurrentStatus = TestStatus.Failed;
      }

      this.EndTest();
    }
  }
}
