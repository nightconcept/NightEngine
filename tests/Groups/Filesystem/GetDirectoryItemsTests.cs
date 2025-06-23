// <copyright file="GetDirectoryItemsTests.cs" company="Night Circle">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Night;

using NightTest.Core;

namespace NightTest.Tests.Filesystem
{
  /// <summary>
  /// Base class for GetDirectoryItems tests, handling setup and cleanup.
  /// </summary>
  internal abstract class BaseGetDirectoryItemsTest : GameTestCase
  {
    /// <summary>
    /// Gets the name of the temporary directory used for testing.
    /// </summary>
#pragma warning disable SA1401 // Fields should be private
    protected readonly string TestDirName = "gdi_test_dir";

    /// <summary>
    /// Gets the full path to the test directory within the save directory.
    /// </summary>
    protected string testSaveDir = string.Empty;

    /// <summary>
    /// Gets the full path to the test directory within the source directory.
    /// </summary>
    protected string testSourceDir = string.Empty;
#pragma warning restore SA1401 // Fields should be private

    private readonly string testIdentity = "NightTest_GDI";

    /// <inheritdoc/>
    protected override void Load()
    {
      Night.Filesystem.SetIdentity(this.testIdentity);
      var saveRoot = Night.Filesystem.GetSaveDirectory();
      var sourceRoot = Night.Filesystem.GetSource();

      this.testSaveDir = Path.Combine(saveRoot, this.TestDirName);
      this.testSourceDir = Path.Combine(sourceRoot, this.TestDirName);

      // Clean up from previous runs to ensure a clean slate
      if (Directory.Exists(this.testSaveDir))
      {
        Directory.Delete(this.testSaveDir, true);
      }

      if (Directory.Exists(this.testSourceDir))
      {
        Directory.Delete(this.testSourceDir, true);
      }
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      // Base update is empty; derived classes implement test logic.
    }
  }

  /// <summary>
  /// Tests GetDirectoryItems with files in both save and source directories.
  /// </summary>
  internal class GetDirectoryItems_SaveAndSource_Combined : BaseGetDirectoryItemsTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetDirectoryItems_SaveAndSource_Combined";

    /// <inheritdoc/>
    public override string Description => "Tests GetDirectoryItems with files in both save and source directories.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();

      _ = Directory.CreateDirectory(this.testSaveDir);
      _ = Directory.CreateDirectory(this.testSourceDir);

      File.WriteAllText(Path.Combine(this.testSaveDir, "file_save.txt"), "save");
      File.WriteAllText(Path.Combine(this.testSourceDir, "file_source.txt"), "source");
      File.WriteAllText(Path.Combine(this.testSaveDir, "file_both.txt"), "save_version");
      File.WriteAllText(Path.Combine(this.testSourceDir, "file_both.txt"), "source_version");
      _ = Directory.CreateDirectory(Path.Combine(this.testSaveDir, "subdir_save"));
      _ = Directory.CreateDirectory(Path.Combine(this.testSourceDir, "subdir_source"));
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var items = Night.Filesystem.GetDirectoryItems(this.TestDirName).ToList();
      var expectedItems = new List<string> { "file_both.txt", "file_save.txt", "file_source.txt", "subdir_save", "subdir_source" };

      items.Sort(StringComparer.Ordinal);
      expectedItems.Sort(StringComparer.Ordinal);

      bool success = items.SequenceEqual(expectedItems);

      if (success)
      {
        this.RecordSuccess("Correctly listed and merged items from save and source.");
      }
      else
      {
        this.RecordFailure($"Item list mismatch. Expected: [{string.Join(", ", expectedItems)}], Got: [{string.Join(", ", items)}]");
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests GetDirectoryItems with files only in the save directory.
  /// </summary>
  internal class GetDirectoryItems_SaveOnly : BaseGetDirectoryItemsTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetDirectoryItems_SaveOnly";

    /// <inheritdoc/>
    public override string Description => "Tests GetDirectoryItems with files only in the save directory.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      _ = Directory.CreateDirectory(this.testSaveDir);
      File.WriteAllText(Path.Combine(this.testSaveDir, "save_file.txt"), "data");
      _ = Directory.CreateDirectory(Path.Combine(this.testSaveDir, "save_subdir"));
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var items = Night.Filesystem.GetDirectoryItems(this.TestDirName).ToList();
      var expectedItems = new List<string> { "save_file.txt", "save_subdir" };

      items.Sort(StringComparer.Ordinal);
      expectedItems.Sort(StringComparer.Ordinal);

      bool success = items.SequenceEqual(expectedItems);

      if (success)
      {
        this.RecordSuccess("Correctly listed items from save directory.");
      }
      else
      {
        this.RecordFailure($"Item list mismatch. Expected: [{string.Join(", ", expectedItems)}], Got: [{string.Join(", ", items)}]");
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests GetDirectoryItems with files only in the source directory.
  /// </summary>
  internal class GetDirectoryItems_SourceOnly : BaseGetDirectoryItemsTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetDirectoryItems_SourceOnly";

    /// <inheritdoc/>
    public override string Description => "Tests GetDirectoryItems with files only in the source directory.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      _ = Directory.CreateDirectory(this.testSourceDir);
      File.WriteAllText(Path.Combine(this.testSourceDir, "source_file.txt"), "data");
      _ = Directory.CreateDirectory(Path.Combine(this.testSourceDir, "source_subdir"));
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var items = Night.Filesystem.GetDirectoryItems(this.TestDirName).ToList();
      var expectedItems = new List<string> { "source_file.txt", "source_subdir" };

      items.Sort(StringComparer.Ordinal);
      expectedItems.Sort(StringComparer.Ordinal);

      bool success = items.SequenceEqual(expectedItems);

      if (success)
      {
        this.RecordSuccess("Correctly listed items from source directory.");
      }
      else
      {
        this.RecordFailure($"Item list mismatch. Expected: [{string.Join(", ", expectedItems)}], Got: [{string.Join(", ", items)}]");
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests GetDirectoryItems on a non-existent directory.
  /// </summary>
  internal class GetDirectoryItems_NotFound : BaseGetDirectoryItemsTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetDirectoryItems_NotFound";

    /// <inheritdoc/>
    public override string Description => "Tests GetDirectoryItems on a non-existent directory.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var items = Night.Filesystem.GetDirectoryItems("non_existent_dir_gdi").ToList();
      if (items.Count == 0)
      {
        this.RecordSuccess("Returned an empty list for a non-existent path as expected.");
      }
      else
      {
        this.RecordFailure($"Expected an empty list but got {items.Count} items.");
      }

      this.EndTest();
    }
  }
}
