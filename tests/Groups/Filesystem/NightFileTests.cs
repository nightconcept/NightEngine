// <copyright file="NightFileTests.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for the Night.NightFile class.
  /// </summary>
  public class NightFile_Dispose_DoesNotThrowTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "NightFile.Dispose.DoesNotThrow";

    /// <inheritdoc/>
    public override string Description => "Tests that NightFile.Dispose() can be called without throwing an exception.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      try
      {
        using (var nightFile = new NightFile())
        {
          // Dispose is called automatically by using statement.
        }

        // If we reach here, Dispose() was called and did not throw.
        this.CurrentStatus = TestStatus.Passed;
        this.Details = "NightFile.Dispose() called successfully without exceptions.";
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"NightFile.Dispose() threw an unexpected exception: {e.Message}";
      }
      finally
      {
        this.EndTest();
      }
    }
  }
}
