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

using System;
using System.IO;
using System.Linq;
using System.Text;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.Filesystem.Append().
  /// </summary>
  public class FilesystemAppend_AppendToNewFileTest : ModTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_append_new.txt");
    private readonly byte[] dataToAppend = Encoding.UTF8.GetBytes("First line.");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.NewFile";

    /// <inheritdoc/>
    public override string Description => "Tests Append to a new file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully appended data to a new file.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.testFileName))
      {
        File.Delete(this.testFileName);
      }

      try
      {
        Night.Filesystem.Append(this.testFileName, this.dataToAppend);
        byte[] fileContent = File.ReadAllBytes(this.testFileName);
        Assert.Equal(this.dataToAppend, fileContent);
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          File.Delete(this.testFileName);
        }
      }
    }
  }

  /// <summary>
  /// Tests appending data to an existing file.
  /// </summary>
  public class FilesystemAppend_AppendToExistingFileTest : ModTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_append_existing.txt");
    private readonly byte[] initialData = Encoding.UTF8.GetBytes("Initial content. ");
    private readonly byte[] dataToAppend = Encoding.UTF8.GetBytes("Appended data.");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.ExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests Append to an existing file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully appended data to an existing file.";

    /// <inheritdoc/>
    public override void Run()
    {
      try
      {
        File.WriteAllBytes(this.testFileName, this.initialData);
        byte[] expectedData = this.initialData.Concat(this.dataToAppend).ToArray();

        Night.Filesystem.Append(this.testFileName, this.dataToAppend);
        byte[] fileContent = File.ReadAllBytes(this.testFileName);

        Assert.Equal(expectedData, fileContent);
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          File.Delete(this.testFileName);
        }
      }
    }
  }

  /// <summary>
  /// Tests appending a partial amount of data using the size parameter.
  /// </summary>
  public class FilesystemAppend_PartialDataTest : ModTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_append_partial.txt");
    private readonly byte[] fullData = Encoding.UTF8.GetBytes("FullDataString");
    private readonly long sizeToAppend = 5; // "FullD"

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.PartialData";

    /// <inheritdoc/>
    public override string Description => "Tests Append with a specific size to append only part of the data.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully appended partial data using the size parameter.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.testFileName))
      {
        File.Delete(this.testFileName);
      }

      byte[] expectedData = new byte[this.sizeToAppend];
      Array.Copy(this.fullData, expectedData, this.sizeToAppend);

      try
      {
        Night.Filesystem.Append(this.testFileName, this.fullData, this.sizeToAppend);
        byte[] fileContent = File.ReadAllBytes(this.testFileName);
        Assert.Equal(expectedData, fileContent);
      }
      finally
      {
        if (File.Exists(this.testFileName))
        {
          File.Delete(this.testFileName);
        }
      }
    }
  }

  /// <summary>
  /// Tests argument validation for Filesystem.Append.
  /// </summary>
  public class FilesystemAppend_ArgumentValidationTest : ModTestCase
  {
    private readonly string validFileName = Path.Combine(Path.GetTempPath(), "night_test_append_validation.txt");
    private readonly byte[] validData = { 1, 2, 3 };

    /// <inheritdoc/>
    public override string Name => "Filesystem.Append.ArgumentValidation";

    /// <inheritdoc/>
    public override string Description => "Tests argument validation for Filesystem.Append (null filename, null data, empty filename).";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully validated arguments for Append.";

    /// <inheritdoc/>
    public override void Run()
    {
      try
      {
        // Test null filename
        _ = Assert.Throws<ArgumentNullException>(() => Night.Filesystem.Append(null!, this.validData));

        // Test null data
        _ = Assert.Throws<ArgumentNullException>(() => Night.Filesystem.Append(this.validFileName, null!));

        // Test empty filename
        _ = Assert.Throws<ArgumentException>(() => Night.Filesystem.Append(string.Empty, this.validData));
      }
      finally
      {
        // Clean up the valid file name if it was created by a failed test part
        if (File.Exists(this.validFileName))
        {
          File.Delete(this.validFileName);
        }
      }
    }
  }
}
