// <copyright file="FileDropTestGame.cs" company="Night Circle">
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

using Xunit;

namespace NightTest.Core
{
  /// <summary>
  /// A test game for verifying file drop events.
  /// </summary>
  public class FileDropTestGame : GameTestCase
  {
    private readonly string expectedPath;
    private string? actualPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDropTestGame"/> class.
    /// </summary>
    /// <param name="expectedPath">The expected path of the dropped file.</param>
    public FileDropTestGame(string expectedPath)
    {
      this.expectedPath = expectedPath;
    }

    /// <inheritdoc/>
    public override string Name => "FileDropEventTest";

    /// <inheritdoc/>
    public override string Description => "Tests that the file drop event is triggered correctly.";

    /// <inheritdoc/>
    public override void FileDropped(DroppedFile file)
    {
      this.actualPath = file.Path;
      Assert.Equal(this.expectedPath, this.actualPath);
      this.CurrentStatus = TestStatus.Passed;
      this.Details = $"File dropped with correct path: {this.actualPath}";
      this.EndTest();
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      // The test will be driven by the FileDropped event
      // We can add a timeout condition here if we want
      if (this.CheckCompletionAfterDuration(5000, () => this.actualPath != null, passDetails: () => $"File drop event received with path: {this.actualPath}", failDetailsTimeout: () => "Test failed: Timed out waiting for file drop event."))
      {
        return;
      }
    }
  }
}
