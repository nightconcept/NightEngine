// <copyright file="DroppedFileTests.cs" company="Night Circle">
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

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.DroppedFile.
  /// </summary>
  public class DroppedFile_PathCorrectness : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "DroppedFile.PathCorrectness";

    /// <inheritdoc/>
    public override string Description => "Tests that the Path property of DroppedFile is correctly set.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully verified DroppedFile path handling.";

    /// <inheritdoc/>
    public override void Run()
    {
      const string testPath = "/path/to/some/file.txt";
      var droppedFile = new DroppedFile(testPath);

      Assert.Equal(testPath, droppedFile.Path);
    }
  }
}
