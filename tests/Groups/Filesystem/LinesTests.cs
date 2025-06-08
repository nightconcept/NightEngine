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

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests reading lines from a standard text file with multiple lines.
  /// </summary>
  public class FilesystemLines_ReadStandardFileTest : GameTestCase
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
    protected override void Load()
    {
      base.Load(); // Important to call base.Load() if it does any setup.
      try
      {
        File.WriteAllLines(TestFileName, this.expectedLines);
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Failed to create test file '{TestFileName}': {e.Message}";
        this.EndTest();
      }
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      try
      {
        if (this.CurrentStatus == TestStatus.Failed)
        {
          return; // EndTest was already called
        }

        var linesRead = Night.Filesystem.Lines(TestFileName).ToList();

        if (linesRead.Count != this.expectedLines.Count)
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Expected {this.expectedLines.Count} lines, but got {linesRead.Count}.";
        }
        else
        {
          bool contentMatches = true;
          for (int i = 0; i < this.expectedLines.Count; i++)
          {
            if (linesRead[i] != this.expectedLines[i])
            {
              contentMatches = false;
              this.CurrentStatus = TestStatus.Failed;
              this.Details = $"Line {i + 1} content mismatch. Expected: '{this.expectedLines[i]}', Got: '{linesRead[i]}'.";
              break;
            }
          }

          if (contentMatches)
          {
            this.CurrentStatus = TestStatus.Passed;
            this.Details = "Successfully read and verified all lines from the standard file.";
          }
        }
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Exception during file read or verification: {e.Message}";
      }
      finally
      {
        if (File.Exists(TestFileName))
        {
          try
          {
            File.Delete(TestFileName);
          }
          catch (System.Exception ex)
          {
            // Log or append to details if deletion fails, but don't overwrite primary test result.
            this.Details += $" | Warning: Failed to delete test file '{TestFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests reading lines from an empty text file.
  /// </summary>
  public class FilesystemLines_ReadEmptyFileTest : GameTestCase
  {
    private const string TestFileName = "test_empty.txt";

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ReadEmptyFile";

    /// <inheritdoc/>
    public override string Description => "Tests reading lines from an empty text file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      try
      {
        File.WriteAllText(TestFileName, string.Empty);
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Failed to create empty test file '{TestFileName}': {e.Message}";
        this.EndTest();
      }
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      try
      {
        if (this.CurrentStatus == TestStatus.Failed)
        {
          return;
        }

        var linesRead = Night.Filesystem.Lines(TestFileName).ToList();

        if (!linesRead.Any())
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully read an empty file, resulting in an empty enumerable.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Expected an empty enumerable, but got {linesRead.Count} lines.";
        }
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Exception during empty file read or verification: {e.Message}";
      }
      finally
      {
        if (File.Exists(TestFileName))
        {
          try
          {
            File.Delete(TestFileName);
          }
          catch (System.Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{TestFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests that Night.Filesystem.Lines throws FileNotFoundException for a non-existent file.
  /// </summary>
  public class FilesystemLines_FileNotFoundTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.FileNotFound";

    /// <inheritdoc/>
    public override string Description => "Tests that Night.Filesystem.Lines throws FileNotFoundException for a non-existent file.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      try
      {
        // Attempt to iterate. ToList() forces enumeration.
        _ = Night.Filesystem.Lines("this_file_should_definitely_not_exist.txt").ToList();

        // If we reach here, the expected exception was not thrown.
        this.CurrentStatus = TestStatus.Failed;
        this.Details = "FileNotFoundException was not thrown for a non-existent file.";
      }
      catch (FileNotFoundException)
      {
        this.CurrentStatus = TestStatus.Passed;
        this.Details = "Successfully caught FileNotFoundException for a non-existent file.";
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected exception occurred: {e.GetType().Name} - {e.Message}";
      }
      finally
      {
        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests reading lines from a text file containing only a single line.
  /// </summary>
  public class FilesystemLines_ReadSingleLineFileTest : GameTestCase
  {
    private const string TestFileName = "test_single_line.txt";
    private const string ExpectedLineContent = "This is the only line.";

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ReadSingleLineFile";

    /// <inheritdoc/>
    public override string Description => "Tests reading lines from a text file containing only a single line.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      try
      {
        File.WriteAllText(TestFileName, ExpectedLineContent);
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Failed to create single line test file '{TestFileName}': {e.Message}";
        this.EndTest();
      }
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      try
      {
        if (this.CurrentStatus == TestStatus.Failed)
        {
          return;
        }

        var linesRead = Night.Filesystem.Lines(TestFileName).ToList();

        if (linesRead.Count == 1)
        {
          if (linesRead[0] == ExpectedLineContent)
          {
            this.CurrentStatus = TestStatus.Passed;
            this.Details = "Successfully read and verified the single line from the file.";
          }
          else
          {
            this.CurrentStatus = TestStatus.Failed;
            this.Details = $"Content mismatch for the single line. Expected: '{ExpectedLineContent}', Got: '{linesRead[0]}'.";
          }
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Expected 1 line, but got {linesRead.Count} lines.";
        }
      }
      catch (System.Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Exception during single line file read or verification: {e.Message}";
      }
      finally
      {
        if (File.Exists(TestFileName))
        {
          try
          {
            File.Delete(TestFileName);
          }
          catch (System.Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{TestFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }
}
