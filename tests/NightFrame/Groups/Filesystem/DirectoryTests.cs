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

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.Filesystem.CreateDirectory().
  /// </summary>
  public class FilesystemCreateDirectory_NewSingleDirTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.NewSingleDir";

    /// <inheritdoc/>
    public override string Description => "Tests CreateDirectory for a new single directory.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully created a new single directory.";

    /// <inheritdoc/>
    public override void Run()
    {
      var testDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_single");
      if (Directory.Exists(testDirName))
      {
        Directory.Delete(testDirName, true);
      }

      try
      {
        var created = Night.Filesystem.CreateDirectory(testDirName);
        Assert.True(created, "CreateDirectory should return true for a new directory.");
        Assert.True(Directory.Exists(testDirName), "Directory should exist after creation.");
      }
      finally
      {
        if (Directory.Exists(testDirName))
        {
          Directory.Delete(testDirName, true);
        }
      }
    }
  }

  /// <summary>
  /// Tests CreateDirectory when the directory already exists.
  /// </summary>
  public class FilesystemCreateDirectory_ExistingDirTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.ExistingDir";

    /// <inheritdoc/>
    public override string Description => "Tests CreateDirectory when the directory already exists, expecting false.";

    /// <inheritdoc/>
    public override string SuccessMessage => "CreateDirectory correctly returned false for an existing directory.";

    /// <inheritdoc/>
    public override void Run()
    {
      var testDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_existing");
      _ = Directory.CreateDirectory(testDirName); // Ensure it exists

      try
      {
        var created = Night.Filesystem.CreateDirectory(testDirName);
        Assert.False(created, "CreateDirectory should return false for an existing directory.");
        Assert.True(Directory.Exists(testDirName), "Directory should still exist.");
      }
      finally
      {
        if (Directory.Exists(testDirName))
        {
          Directory.Delete(testDirName, true);
        }
      }
    }
  }

  /// <summary>
  /// Tests CreateDirectory for nested directories.
  /// </summary>
  public class FilesystemCreateDirectory_NestedDirTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.NestedDir";

    /// <inheritdoc/>
    public override string Description => "Tests CreateDirectory for creating nested directories.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully created nested directories.";

    /// <inheritdoc/>
    public override void Run()
    {
      var parentDirName = Path.Combine(Path.GetTempPath(), "night_test_createdir_parent");
      var nestedDirName = Path.Combine(parentDirName, "child", "grandchild");

      if (Directory.Exists(parentDirName))
      {
        Directory.Delete(parentDirName, true);
      }

      try
      {
        var created = Night.Filesystem.CreateDirectory(nestedDirName);
        Assert.True(created, "CreateDirectory should return true for nested directories.");
        Assert.True(Directory.Exists(nestedDirName), "Nested directory should exist after creation.");
      }
      finally
      {
        if (Directory.Exists(parentDirName))
        {
          Directory.Delete(parentDirName, true);
        }
      }
    }
  }

  /// <summary>
  /// Tests argument validation for CreateDirectory.
  /// </summary>
  public class FilesystemCreateDirectory_ArgumentValidationTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.CreateDirectory.ArgumentValidation";

    /// <inheritdoc/>
    public override string Description => "Tests argument validation for CreateDirectory (null path, empty path).";

    /// <inheritdoc/>
    public override string SuccessMessage => "Argument validation for CreateDirectory works correctly.";

    /// <inheritdoc/>
    public override void Run()
    {
      _ = Assert.Throws<ArgumentNullException>(() => Night.Filesystem.CreateDirectory(null!));
      _ = Assert.Throws<ArgumentException>(() => Night.Filesystem.CreateDirectory(string.Empty));
      _ = Assert.Throws<ArgumentException>(() => Night.Filesystem.CreateDirectory("   "));
    }
  }

  /// <summary>
  /// Tests GetAppdataDirectory returns a valid path and creates the directory.
  /// </summary>
  public class FilesystemGetAppdataDirectory_ReturnsValidPathTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetAppdataDirectory.ReturnsValidPath";

    /// <inheritdoc/>
    public override string Description => "Tests GetAppdataDirectory returns a valid path and creates the directory.";

    /// <inheritdoc/>
    public override string SuccessMessage => "GetAppdataDirectory returned a valid, existing path.";

    /// <inheritdoc/>
    public override void Run()
    {
      // Note: This test can have side effects by creating a directory in the user's
      // real AppData folder. This is acceptable for this test suite's scope.
      var appDataDir = Night.Filesystem.GetAppdataDirectory();

      Assert.False(string.IsNullOrWhiteSpace(appDataDir), "Appdata directory path should not be null or whitespace.");
      Assert.True(Directory.Exists(appDataDir), "Appdata directory should be created by the method.");

      // Further check if the path seems reasonable (contains app name, etc.)
      // This is a basic check and might need adjustment based on final GetAppdataDirectory logic.
      var expectedEnd = "NightDefault";
      Assert.EndsWith(expectedEnd, appDataDir);
    }
  }
}
