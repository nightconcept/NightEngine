// <copyright file="AppendTests.cs" company="Night Circle">
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

using System.IO;
using System.Text;

using Night;

using NightTest.Core;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Base class for Filesystem.Append tests, handling setup and cleanup of the save directory.
  /// </summary>
  public abstract class BaseAppendTest : GameTestCase
  {
#pragma warning disable SA1401 // Fields should be private
    /// <summary>
    /// The unique identity used for this test group to isolate the save directory.
    /// </summary>
    protected readonly string TestIdentity = "NightTest_Append";
#pragma warning restore SA1401 // Fields should be private

    /// <inheritdoc/>
    protected override void Load()
    {
      Night.Filesystem.SetIdentity(this.TestIdentity);
      var saveRoot = Night.Filesystem.GetSaveDirectory();

      // Clean up from previous runs
      if (Directory.Exists(saveRoot))
      {
        Directory.Delete(saveRoot, true);
      }

      _ = Directory.CreateDirectory(saveRoot);
    }
  }

  /// <summary>
  /// Tests appending a string to a new file.
  /// </summary>
  public class Append_String_NewFile : BaseAppendTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Append_String_NewFile";

    /// <inheritdoc/>
    public override string Description => "Tests appending a string to a new file in the save directory.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var (success, error) = Night.Filesystem.Append("new_file.txt", "Hello");
      if (!success)
      {
        this.RecordFailure($"Append failed: {error}");
        this.EndTest();
        return;
      }

      var content = File.ReadAllText(Path.Combine(Night.Filesystem.GetSaveDirectory(), "new_file.txt"));
      if (content == "Hello")
      {
        this.RecordSuccess("Successfully appended to a new file.");
      }
      else
      {
        this.RecordFailure($"File content mismatch. Expected 'Hello', got '{content}'.");
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests appending a string to an already existing file.
  /// </summary>
  public class Append_String_ExistingFile : BaseAppendTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Append_String_ExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests appending a string to an existing file.";

    /// <inheritdoc/>
    protected override void Load()
    {
      base.Load();
      File.WriteAllText(Path.Combine(Night.Filesystem.GetSaveDirectory(), "existing.txt"), "Initial.");
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var (success, error) = Night.Filesystem.Append("existing.txt", " Appended.");
      if (!success)
      {
        this.RecordFailure($"Append failed: {error}");
        this.EndTest();
        return;
      }

      var content = File.ReadAllText(Path.Combine(Night.Filesystem.GetSaveDirectory(), "existing.txt"));
      if (content == "Initial. Appended.")
      {
        this.RecordSuccess("Successfully appended to an existing file.");
      }
      else
      {
        this.RecordFailure($"File content mismatch. Expected 'Initial. Appended.', got '{content}'.");
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests appending to a file located within a subdirectory that needs to be created.
  /// </summary>
  public class Append_String_WithPath : BaseAppendTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Append_String_WithPath";

    /// <inheritdoc/>
    public override string Description => "Tests appending to a file in a subdirectory of the save directory.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var (success, error) = Night.Filesystem.Append("subdir/path.txt", "Subdir content");
      if (!success)
      {
        this.RecordFailure($"Append failed: {error}");
        this.EndTest();
        return;
      }

      var filePath = Path.Combine(Night.Filesystem.GetSaveDirectory(), "subdir", "path.txt");
      if (File.Exists(filePath) && File.ReadAllText(filePath) == "Subdir content")
      {
        this.RecordSuccess("Successfully appended to a file in a new subdirectory.");
      }
      else
      {
        this.RecordFailure("File was not created or content is incorrect in subdirectory.");
      }

      this.EndTest();
    }
  }

  /// <summary>
  /// Tests appending a byte array to a new file.
  /// </summary>
  public class Append_Bytes_NewFile : BaseAppendTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Append_Bytes_NewFile";

    /// <inheritdoc/>
    public override string Description => "Tests appending bytes to a new file.";

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      var data = Encoding.UTF8.GetBytes("Byte data");
      var (success, error) = Night.Filesystem.Append("bytes.txt", data);
      if (!success)
      {
        this.RecordFailure($"Append failed: {error}");
        this.EndTest();
        return;
      }

      var content = File.ReadAllBytes(Path.Combine(Night.Filesystem.GetSaveDirectory(), "bytes.txt"));
      if (Encoding.UTF8.GetString(content) == "Byte data")
      {
        this.RecordSuccess("Successfully appended bytes to a new file.");
      }
      else
      {
        this.RecordFailure("Byte content mismatch.");
      }

      this.EndTest();
    }
  }
}
