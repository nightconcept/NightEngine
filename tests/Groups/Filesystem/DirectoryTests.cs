// <copyright file="DirectoryTests.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.Filesystem.CreateDirectory().
  /// </summary>
  public class FilesystemCreateDirectory_NewSingleDirTest : GameTestCase
  {
    private readonly string testDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_single");

    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.NewSingleDir";

    /// <inheritdoc/>
    public override string Description => "Tests CreateDirectory for a new single directory.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      if (Directory.Exists(this.testDirName))
      {
        Directory.Delete(this.testDirName, true); // true for recursive, just in case
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
        bool created = Night.Filesystem.CreateDirectory(this.testDirName);

        if (created && Directory.Exists(this.testDirName))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully created a new single directory and CreateDirectory returned true.";
        }
        else if (!created && Directory.Exists(this.testDirName))
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "CreateDirectory returned false, but the directory was created.";
        }
        else if (created && !Directory.Exists(this.testDirName))
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "CreateDirectory returned true, but the directory was not found.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "Failed to create directory or CreateDirectory returned false unexpectedly.";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (Directory.Exists(this.testDirName))
        {
          try
          {
            Directory.Delete(this.testDirName, true);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test directory '{this.testDirName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests CreateDirectory when the directory already exists.
  /// </summary>
  public class FilesystemCreateDirectory_ExistingDirTest : GameTestCase
  {
    private readonly string testDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_existing");

    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.ExistingDir";

    /// <inheritdoc/>
    public override string Description => "Tests CreateDirectory when the directory already exists, expecting false.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      try
      {
        _ = Directory.CreateDirectory(this.testDirName); // Ensure it exists
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"Setup failed: Could not create pre-existing test directory '{this.testDirName}'. {e.Message}";
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

        bool created = Night.Filesystem.CreateDirectory(this.testDirName);

        if (!created && Directory.Exists(this.testDirName))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "CreateDirectory correctly returned false for an existing directory.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"CreateDirectory returned {created} for an existing directory. Expected false.";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        if (Directory.Exists(this.testDirName))
        {
          try
          {
            Directory.Delete(this.testDirName, true);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete test directory '{this.testDirName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests CreateDirectory for nested directories.
  /// </summary>
  public class FilesystemCreateDirectory_NestedDirTest : GameTestCase
  {
    private readonly string parentDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_parent");
    private readonly string nestedDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_parent", "child", "grandchild");

    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.NestedDir";

    /// <inheritdoc/>
    public override string Description => "Tests CreateDirectory for creating nested directories.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      if (Directory.Exists(this.parentDirName))
      {
        Directory.Delete(this.parentDirName, true);
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
        bool created = Night.Filesystem.CreateDirectory(this.nestedDirName);

        if (created && Directory.Exists(this.nestedDirName))
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully created nested directories and CreateDirectory returned true.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Failed to create nested directories or CreateDirectory returned {created}. Path exists: {Directory.Exists(this.nestedDirName)}";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        // Clean up the parent, which should remove nested
        if (Directory.Exists(this.parentDirName))
        {
          try
          {
            Directory.Delete(this.parentDirName, true);
          }
          catch (Exception ex)
          {
            this.Details += $" | Warning: Failed to delete parent test directory '{this.parentDirName}': {ex.Message}";
          }
        }

        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests argument validation for Filesystem.CreateDirectory.
  /// </summary>
  public class FilesystemCreateDirectory_ArgumentValidationTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.ArgumentValidation";

    /// <inheritdoc/>
    public override string Description => "Tests argument validation for CreateDirectory (null path, empty path).";

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
        // Test null path
        try
        {
          _ = Night.Filesystem.CreateDirectory(null!);
          failureDetails += "ArgumentNullException not thrown for null path. ";
        }
        catch (ArgumentNullException)
        {
          checksPassed++;
        }
        catch (Exception e)
        {
          failureDetails += $"Unexpected exception for null path: {e.GetType().Name}. ";
        }

        // Test empty path
        try
        {
          _ = Night.Filesystem.CreateDirectory(string.Empty);
          failureDetails += "ArgumentException not thrown for empty path. ";
        }
        catch (ArgumentException)
        {
          checksPassed++;
        }
        catch (Exception e)
        {
          failureDetails += $"Unexpected exception for empty path: {e.GetType().Name}. ";
        }

        if (checksPassed == 2)
        {
          this.CurrentStatus = TestStatus.Passed;
          this.Details = "Successfully validated arguments for CreateDirectory.";
        }
        else
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"Argument validation failed. Checks passed: {checksPassed}/2. Details: {failureDetails.Trim()}";
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred during validation tests: {e.Message}";
      }
      finally
      {
        this.EndTest();
      }
    }
  }

  /// <summary>
  /// Tests for Night.Filesystem.GetAppdataDirectory().
  /// </summary>
  public class FilesystemGetAppdataDirectory_ReturnsValidPathTest : GameTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetAppdataDirectory.ReturnsValidPath";

    /// <inheritdoc/>
    public override string Description => "Tests GetAppdataDirectory returns a valid path and creates the directory.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      if (this.IsDone)
      {
        return;
      }

      try
      {
        string appDataPath1 = Night.Filesystem.GetAppdataDirectory();

        if (string.IsNullOrEmpty(appDataPath1))
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = "GetAppdataDirectory returned a null or empty path.";
        }
        else if (!Directory.Exists(appDataPath1))
        {
          this.CurrentStatus = TestStatus.Failed;
          this.Details = $"GetAppdataDirectory returned path '{appDataPath1}', but the directory does not exist.";
        }
        else
        {
          // Call it again to ensure it's idempotent and directory creation doesn't fail
          string appDataPath2 = Night.Filesystem.GetAppdataDirectory();
          if (appDataPath1 == appDataPath2 && Directory.Exists(appDataPath2))
          {
            this.CurrentStatus = TestStatus.Passed;
            this.Details = $"Successfully retrieved appdata directory: {appDataPath1}. Directory exists and subsequent call returned same path.";
          }
          else
          {
            this.CurrentStatus = TestStatus.Failed;
            this.Details = $"Subsequent call to GetAppdataDirectory returned '{appDataPath2}' (expected '{appDataPath1}') or directory no longer exists.";
          }
        }
      }
      catch (Exception e)
      {
        this.CurrentStatus = TestStatus.Failed;
        this.Details = $"An unexpected error occurred: {e.Message}";
      }
      finally
      {
        // It's generally not advisable to delete the *actual* appdata directory in a test.
        // We are only verifying its creation and path retrieval.
        // If specific test content was written there, that should be cleaned.
        // For this test, we just check existence.
        this.EndTest();
      }
    }
  }
}
