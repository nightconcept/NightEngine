// <copyright file="RemoveTests.cs" company="Night Circle">
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

namespace NightTest.Tests.Groups.Filesystem
{
  /// <summary>
  /// Base class for Filesystem.Remove tests, handling setup and teardown.
  /// </summary>
  public abstract class BaseRemoveTest : GameTestCase
  {
    /// <summary>
    /// Gets the path to the save directory used for this test run.
    /// </summary>
#pragma warning disable SA1401 // Fields should be private
    protected string saveDir = string.Empty;
#pragma warning restore SA1401 // Fields should be private
    private readonly string testIdentity = "NightTest_Remove";

    /// <summary>
    /// Sets up the test environment by setting a unique filesystem identity
    /// and cleaning up any artifacts from previous test runs.
    /// </summary>
    protected override void Load()
    {
      Night.Filesystem.SetIdentity(this.testIdentity);
      this.saveDir = Night.Filesystem.GetSaveDirectory();

      // Clean up previous test runs
      if (Directory.Exists(this.saveDir))
      {
        Directory.Delete(this.saveDir, true);
      }

      _ = Directory.CreateDirectory(this.saveDir);
    }

    /// <summary>
    /// The main update loop for the test case.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last frame.</param>
    protected override void Update(double deltaTime)
    {
      // Most tests are synchronous and will complete in one frame
    }
  }

  /// <summary>
  /// Tests successfully removing a file from the save directory.
  /// </summary>
  public class RemoveFileTest : BaseRemoveTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Remove File";

    /// <inheritdoc/>
    public override string Description => "Tests successfully removing a file from the save directory.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var testFile = "testfile.txt";
      var fullPath = Path.Combine(this.saveDir, testFile);
      File.WriteAllText(fullPath, "delete me");

      if (!File.Exists(fullPath))
      {
        this.RecordFailure("Setup failed: Could not create test file.");
        return;
      }

      var result = Night.Filesystem.Remove(testFile);

      if (result && !File.Exists(fullPath))
      {
        this.RecordSuccess("Successfully removed file and it no longer exists.");
      }
      else
      {
        this.RecordFailure($"Remove returned {result} but file existence is {File.Exists(fullPath)}.");
      }
    }
  }

  /// <summary>
  /// Tests successfully removing an empty directory from the save directory.
  /// </summary>
  public class RemoveEmptyDirTest : BaseRemoveTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Remove Empty Directory";

    /// <inheritdoc/>
    public override string Description => "Tests successfully removing an empty directory from the save directory.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var testDir = "emptydir";
      var fullPath = Path.Combine(this.saveDir, testDir);
      _ = Directory.CreateDirectory(fullPath);

      if (!Directory.Exists(fullPath))
      {
        this.RecordFailure("Setup failed: Could not create test directory.");
        return;
      }

      var result = Night.Filesystem.Remove(testDir);

      if (result && !Directory.Exists(fullPath))
      {
        this.RecordSuccess("Successfully removed empty directory and it no longer exists.");
      }
      else
      {
        this.RecordFailure($"Remove returned {result} but directory existence is {Directory.Exists(fullPath)}.");
      }
    }
  }

  /// <summary>
  /// Tests that removing a non-empty directory fails.
  /// </summary>
  public class RemoveNonEmptyDirTest : BaseRemoveTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Remove Non-Empty Directory";

    /// <inheritdoc/>
    public override string Description => "Tests that removing a non-empty directory fails.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var testDir = "nonemptydir";
      var fullPath = Path.Combine(this.saveDir, testDir);
      _ = Directory.CreateDirectory(fullPath);
      File.WriteAllText(Path.Combine(fullPath, "somefile.txt"), "content");

      var result = Night.Filesystem.Remove(testDir);

      if (!result && Directory.Exists(fullPath))
      {
        this.RecordSuccess("Correctly failed to remove non-empty directory.");
      }
      else
      {
        this.RecordFailure($"Remove returned {result} for a non-empty directory.");
      }
    }
  }

  /// <summary>
  /// Tests that removing a file outside the save directory fails.
  /// </summary>
  public class RemoveOutsideSaveDirTest : BaseRemoveTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Remove Outside Save Directory";

    /// <inheritdoc/>
    public override string Description => "Tests that removing a file outside the save directory fails.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      // Create a file outside the save dir to attempt to delete
      var tempFile = Path.GetTempFileName();
      var relativePath = Path.GetRelativePath(this.saveDir, tempFile);

      var result = Night.Filesystem.Remove(relativePath);

      File.Delete(tempFile); // Clean up

      if (!result)
      {
        this.RecordSuccess("Correctly failed to remove a file outside the save directory.");
      }
      else
      {
        this.RecordFailure("Incorrectly succeeded in removing a file outside the save directory.");
      }
    }
  }

  /// <summary>
  /// Tests that removing a non-existent path fails gracefully.
  /// </summary>
  public class RemoveNotFoundTest : BaseRemoveTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Remove Non-Existent Path";

    /// <inheritdoc/>
    public override string Description => "Tests that removing a non-existent path fails gracefully.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var result = Night.Filesystem.Remove("nonexistent.file");

      if (!result)
      {
        this.RecordSuccess("Correctly failed to remove a non-existent file.");
      }
      else
      {
        this.RecordFailure("Incorrectly succeeded when trying to remove a non-existent file.");
      }
    }
  }
}
