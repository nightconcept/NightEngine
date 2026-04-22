// <copyright file="SystemOpenURLTests.cs" company="Night Circle">
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

namespace NightTest.Groups.SystemTests
{
  /// <summary>
  /// Manual test case for Night.System.OpenURL().
  /// </summary>
  public class SystemOpenURL_UserConfirmationTest : ManualTestCase
  {
    private const string TestUrl = "https://www.example.com";
    private bool urlOpenedAttempted = false;

    /// <inheritdoc/>
    public override string Name => "System.OpenURL.UserConfirmation";

    /// <inheritdoc/>
    public override string Description => "User must confirm that Night.System.OpenURL correctly opens a specified URL.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      this.Details = $"Attempting to open URL: {TestUrl}. Please observe if your browser opens it.";
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      if (!this.urlOpenedAttempted)
      {
        _ = Night.System.OpenURL(TestUrl);
        this.urlOpenedAttempted = true;
      }

      if (this.TestStopwatch.ElapsedMilliseconds > this.ManualTestPromptDelayMilliseconds)
      {
        this.RequestManualConfirmation($"Did the URL '{TestUrl}' open correctly in your browser or file explorer?");
      }
    }

    /// <inheritdoc/>
    protected override void Draw()
    {
      Night.Graphics.Clear(Night.Color.Black);

      // Night.Graphics.DrawString($"Testing Night.System.OpenURL...", 10, 10, Night.Color.White); // Commented out due to DrawString issues
      // Night.Graphics.DrawString($"Attempting to open: {TestUrl}", 10, 30, Night.Color.White); // Commented out due to DrawString issues

      // The base.Draw() method will handle drawing the prompt when active.
      base.Draw();
    }
  }
}
