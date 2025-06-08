// <copyright file="ReadWriteTests.cs" company="Night Circle">
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
using System.IO;
using System.Linq;
using System.Text;

using Night;

using NightTest.Core;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.Filesystem.ReadBytes().
  /// </summary>
  public class FilesystemReadBytes_ReadExistingFileTest : GameTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_readbytes_file.bin");
    private readonly byte[] expectedContent = { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x4E, 0x69, 0x67, 0x68, 0x74 }; // "Hello Night"

    /// <inheritdoc/>
    public override string Name => "Filesystem.ReadBytes.ReadExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests ReadBytes for an existing binary file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      try
      {
        File.WriteAllBytes(this.testFileName, this.expectedContent);
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Setup failed: Could not create test file '{this.testFileName}'. {e.Message}";
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

        byte[] actualContent = Night.Filesystem.ReadBytes(this.testFileName);

        if (actualContent != null && actualContent.SequenceEqual(this.expectedContent))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully read bytes from an existing file.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "ReadBytes content did not match expected content or was null.";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          try
          {
            File.Delete(this.testFileName);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{this.testFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests for Night.Filesystem.ReadText().
  /// </summary>
  public class FilesystemReadText_ReadExistingFileTest : GameTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_readtext_file.txt");
    private readonly string expectedContent = "Hello Night Text!";

    /// <inheritdoc/>
    public override string Name => "Filesystem.ReadText.ReadExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests ReadText for an existing text file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      try
      {
        File.WriteAllText(this.testFileName, this.expectedContent);
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Setup failed: Could not create test file '{this.testFileName}'. {e.Message}";
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

        string actualContent = Night.Filesystem.ReadText(this.testFileName);

        if (actualContent == this.expectedContent)
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully read text from an existing file.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"ReadText content did not match. Expected: '{this.expectedContent}', Got: '{actualContent}'.";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          try
          {
            File.Delete(this.testFileName);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{this.testFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests Night.Filesystem.ReadBytes for a non-existent file.
  /// </summary>
  public class FilesystemReadBytes_FileNotFoundTest : GameTestCase
  {
    private readonly string nonExistentFile = Path.Combine(Path.GetTempPath(), "night_test_readbytes_nonexistent.bin");

    /// <inheritdoc/>
    public override string Name => "Filesystem.ReadBytes.FileNotFound";

    /// <inheritdoc/>
    public override string Description => "Tests ReadBytes throws FileNotFoundException for a non-existent file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      if (File.Exists(this.nonExistentFile))
      {
        File.Delete(this.nonExistentFile); // Ensure it doesn't exist
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
        _ = Night.Filesystem.ReadBytes(this.nonExistentFile);
        this.CurrentStatus = TestStatus.Failed;
        this.Details = "FileNotFoundException was not thrown for ReadBytes.";
      }
      catch (FileNotFoundException)
      {
        this.CurrentStatus = TestStatus.Passed;
        this.Details = "Successfully caught FileNotFoundException for ReadBytes.";
      }
      catch (Exception e)
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
  /// Tests Night.Filesystem.ReadText for a non-existent file.
  /// </summary>
  public class FilesystemReadText_FileNotFoundTest : GameTestCase
  {
    private readonly string nonExistentFile = Path.Combine(Path.GetTempPath(), "night_test_readtext_nonexistent.txt");

    /// <inheritdoc/>
    public override string Name => "Filesystem.ReadText.FileNotFound";

    /// <inheritdoc/>
    public override string Description => "Tests ReadText throws FileNotFoundException for a non-existent file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      if (File.Exists(this.nonExistentFile))
      {
        File.Delete(this.nonExistentFile); // Ensure it doesn't exist
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
        _ = Night.Filesystem.ReadText(this.nonExistentFile);
        this.CurrentStatus = TestStatus.Failed;
        this.Details = "FileNotFoundException was not thrown for ReadText.";
      }
      catch (FileNotFoundException)
      {
        this.CurrentStatus = TestStatus.Passed;
        this.Details = "Successfully caught FileNotFoundException for ReadText.";
      }
      catch (Exception e)
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
  /// Tests for Night.Filesystem.Append().
  /// </summary>
  public class FilesystemAppend_AppendToNewFileTest : GameTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_append_new.txt");
    private readonly byte[] dataToAppend = Encoding.UTF8.GetBytes("First line.");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.NewFile";

    /// <inheritdoc/>
    public override string Description => "Tests Append to a new file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      if (File.Exists(this.testFileName))
      {
        File.Delete(this.testFileName);
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
        Night.Filesystem.Append(this.testFileName, this.dataToAppend);
        byte[] fileContent = File.ReadAllBytes(this.testFileName);

        if (fileContent.SequenceEqual(this.dataToAppend))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully appended data to a new file.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "Appended content did not match expected content.";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          try
          {
            File.Delete(this.testFileName);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{this.testFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests appending data to an existing file.
  /// </summary>
  public class FilesystemAppend_AppendToExistingFileTest : GameTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_append_existing.txt");
    private readonly byte[] initialData = Encoding.UTF8.GetBytes("Initial content. ");
    private readonly byte[] dataToAppend = Encoding.UTF8.GetBytes("Appended data.");
    private byte[] expectedData = Array.Empty<byte>();

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.ExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests Append to an existing file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      try
      {
        File.WriteAllBytes(this.testFileName, this.initialData);
        this.expectedData = this.initialData.Concat(this.dataToAppend).ToArray();
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Setup failed: Could not create initial test file '{this.testFileName}'. {e.Message}";
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

        Night.Filesystem.Append(this.testFileName, this.dataToAppend);
        byte[] fileContent = File.ReadAllBytes(this.testFileName);

        if (fileContent.SequenceEqual(this.expectedData))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully appended data to an existing file.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Appended content did not match. Expected: [{string.Join(", ", this.expectedData)}], Got: [{string.Join(", ", fileContent)}]";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          try
          {
            File.Delete(this.testFileName);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{this.testFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests appending a partial amount of data using the size parameter.
  /// </summary>
  public class FilesystemAppend_PartialDataTest : GameTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_append_partial.txt");
    private readonly byte[] fullData = Encoding.UTF8.GetBytes("FullDataString");
    private readonly long sizeToAppend = 5; // "FullD"
    private byte[] expectedData = Array.Empty<byte>();

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.PartialData";

    /// <inheritdoc/>
    public override string Description => "Tests Append with a specific size to append only part of the data.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      if (File.Exists(this.testFileName))
      {
        File.Delete(this.testFileName);
      }

      this.expectedData = new byte[this.sizeToAppend];
      Array.Copy(this.fullData, this.expectedData, this.sizeToAppend);
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
        Night.Filesystem.Append(this.testFileName, this.fullData, this.sizeToAppend);
        byte[] fileContent = File.ReadAllBytes(this.testFileName);

        if (fileContent.SequenceEqual(this.expectedData))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully appended partial data using the size parameter.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "Partially appended content did not match expected content.";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          try
          {
            File.Delete(this.testFileName);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test file '{this.testFileName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests argument validation for Filesystem.Append.
  /// </summary>
  public class FilesystemAppend_ArgumentValidationTest : GameTestCase
  {
    private readonly string validFileName = Path.Combine(Path.GetTempPath(), "night_test_append_validation.txt");
    private readonly byte[] validData = { 1, 2, 3 };

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.ArgumentValidation";

    /// <inheritdoc/>
    public override string Description => "Tests argument validation for Filesystem.Append (null filename, null data, empty filename).";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      int checksPassed = 0;
      string failureDetails = string.Empty;

      try
      {
        // Test null filename
        try
        {
          Night.Filesystem.Append(null!, this.validData);
          failureDetails += "ArgumentNullException not thrown for null filename. ";
        }
        catch (ArgumentNullException)
        {
          checksPassed++;
        }
        catch (Exception e)
        {
          failureDetails += $"Unexpected exception for null filename: {e.GetType().Name}. ";
        }

        // Test null data
        try
        {
          Night.Filesystem.Append(this.validFileName, null!);
          failureDetails += "ArgumentNullException not thrown for null data. ";
        }
        catch (ArgumentNullException)
        {
          checksPassed++;
        }
        catch (Exception e)
        {
          failureDetails += $"Unexpected exception for null data: {e.GetType().Name}. ";
        }

        // Test empty filename
        try
        {
          Night.Filesystem.Append(string.Empty, this.validData);
          failureDetails += "ArgumentException not thrown for empty filename. ";
        }
        catch (ArgumentException)
        {
          checksPassed++;
        }
        catch (Exception e)
        {
          failureDetails += $"Unexpected exception for empty filename: {e.GetType().Name}. ";
        }

        if (checksPassed == 3)
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully validated arguments for Append.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Argument validation failed. Checks passed: {checksPassed}/3. Details: {failureDetails.Trim()}";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred during validation tests: {e.Message}";
      }
      finally
      {
        // Clean up the valid file name if it was created by a failed test part
        if (File.Exists(this.validFileName))
        {
          try
          {
            File.Delete(this.validFileName);
          }
          catch
          { /* best effort */
          }
        }

        this.EndTest();
      }
    }
  }
}
