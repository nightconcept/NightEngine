// <copyright file="FileDropEventTest.cs" company="Night Circle">
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

using System.Runtime.InteropServices;

using Night;

using NightTest.Core;

using SDL3;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests the file drop event.
  /// </summary>
  public class FileDropEventTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "FileDropEventTest";

    /// <inheritdoc/>
    public override string Description => "Tests that the file drop event is triggered correctly.";

    /// <inheritdoc/>
    public override string SuccessMessage => "File drop event test passed.";

    /// <inheritdoc/>
    public override void Run()
    {
      const string testPath = "/path/to/some/file.txt";
      var testGame = new FileDropTestGame(testPath);

      // The test game will now handle the event and verify the path
      // We need to run the game loop for a short time to process the event
      Night.Framework.Run(testGame);

      Assert.Equal(TestStatus.Passed, testGame.CurrentStatus);
    }
  }
}
