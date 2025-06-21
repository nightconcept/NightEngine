// <copyright file="LinesTests.cs" company="Night Circle">
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

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests reading lines from a standard text file with multiple lines.
  /// </summary>
  public class FilesystemLines_ReadStandardFileTest : ModTestCase
  {
    private const string TestFileName = "test_standard.txt";
    private readonly List<string> expectedLines = new()
    {
      "Line 1",
      "Line 2 with some different content.",
      "A third line.",
    };

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ReadStandardFile";

    /// <inheritdoc/>
    public override string Description => "Tests reading lines from a standard text file with multiple lines.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully read and verified all lines from the standard file.";

    /// <inheritdoc/>
    public override void Run()
    {
      try
      {
        File.WriteAllLines(TestFileName, this.expectedLines);
        var linesRead = Night.Filesystem.Lines(TestFileName).ToList();
        Assert.Equal(this.expectedLines, linesRead);
      }
      finally
      {
        if (File.Exists(TestFileName))
        {
          File.Delete(TestFileName);
        }
      }
    }
  }

  /// <summary>
  /// Tests reading lines from an empty text file.
  /// </summary>
  public class FilesystemLines_ReadEmptyFileTest : ModTestCase
  {
    private const string TestFileName = "test_empty.txt";

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ReadEmptyFile";

    /// <inheritdoc/>
    public override string Description => "Tests reading lines from an empty text file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully read an empty file, resulting in an empty enumerable.";

    /// <inheritdoc/>
    public override void Run()
    {
      try
      {
        File.WriteAllText(TestFileName, string.Empty);
        var linesRead = Night.Filesystem.Lines(TestFileName).ToList();
        Assert.Empty(linesRead);
      }
      finally
      {
        if (File.Exists(TestFileName))
        {
          File.Delete(TestFileName);
        }
      }
    }
  }

  /// <summary>
  /// Tests that Night.Filesystem.Lines throws FileNotFoundException for a non-existent file.
  /// </summary>
  public class FilesystemLines_FileNotFoundTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.FileNotFound";

    /// <inheritdoc/>
    public override string Description => "Tests that Night.Filesystem.Lines throws FileNotFoundException for a non-existent file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully caught FileNotFoundException for a non-existent file.";

    /// <inheritdoc/>
    public override void Run()
    {
      // Attempt to iterate. ToList() forces enumeration.
      _ = Assert.Throws<FileNotFoundException>(() => Night.Filesystem.Lines("this_file_should_definitely_not_exist.txt").ToList());
    }
  }

  /// <summary>
  /// Tests reading lines from a text file containing only a single line.
  /// </summary>
  public class FilesystemLines_ReadSingleLineFileTest : ModTestCase
  {
    private const string TestFileName = "test_single_line.txt";
    private const string ExpectedLineContent = "This is the only line.";

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ReadSingleLineFile";

    /// <inheritdoc/>
    public override string Description => "Tests reading lines from a text file containing only a single line.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully read and verified the single line from the file.";

    /// <inheritdoc/>
    public override void Run()
    {
      try
      {
        File.WriteAllText(TestFileName, ExpectedLineContent);
        var linesRead = Night.Filesystem.Lines(TestFileName).ToList();
        _ = Assert.Single(linesRead);
        Assert.Equal(ExpectedLineContent, linesRead[0]);
      }
      finally
      {
        if (File.Exists(TestFileName))
        {
          File.Delete(TestFileName);
        }
      }
    }
  }
}
