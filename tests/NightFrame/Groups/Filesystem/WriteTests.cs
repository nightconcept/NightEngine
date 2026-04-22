// <copyright file="WriteTests.cs" company="Night Circle">
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
using Night.Log; // Added for ILogger and LogManager

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Base class for Filesystem.Write ModTestCases, providing common setup and teardown logic
  /// for creating and cleaning up temporary test files and directories.
  /// </summary>
  public abstract class BaseFilesystemWriteTest : ModTestCase
  {
    /// <summary>
    /// Gets or sets the path to the temporary file used for the current test.
    /// This path is automatically generated within a temporary test directory.
    /// </summary>
    protected string TestFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets the path to the temporary directory created for filesystem write tests.
    /// </summary>
    protected string TestDirectoryPath { get; private set; } = string.Empty;

    /// <summary>
    /// Sets up the test environment by creating a temporary directory and defining a test file path.
    /// It then calls <see cref="ExecuteWriteTestLogic"/> and ensures cleanup of created files/directories.
    /// </summary>
    public override void Run()
    {
      this.TestDirectoryPath = Path.Combine(Path.GetTempPath(), "NightEngineWriteTests");
      _ = Directory.CreateDirectory(this.TestDirectoryPath); // Ensure base test directory exists
      this.TestFilePath = Path.Combine(this.TestDirectoryPath, Guid.NewGuid().ToString("N") + ".txt");

      try
      {
        this.ExecuteWriteTestLogic();
      }
      finally
      {
        if (File.Exists(this.TestFilePath))
        {
          File.Delete(this.TestFilePath);
        }

        // Attempt to clean up the directory if it's empty, otherwise leave it for other tests.
        // This is a simple cleanup; more robust might be needed if tests run in parallel and create subdirs.
        if (Directory.Exists(this.TestDirectoryPath) && !Directory.EnumerateFileSystemEntries(this.TestDirectoryPath).Any())
        {
          try
          {
            Directory.Delete(this.TestDirectoryPath);
          }
          catch (IOException)
          {
            // Ignore if deletion fails, another test might still be using it or created something.
          }
        }
      }
    }

    /// <summary>
    /// Contains the specific test logic for a Filesystem.Write test case.
    /// This method is called by the <see cref="Run"/> method after setup and before teardown.
    /// </summary>
    protected abstract void ExecuteWriteTestLogic();
  }

  /// <summary>
  /// Tests writing a basic string to a new file using Filesystem.Write.
  /// </summary>
  public class FilesystemWrite_String_BasicNewFileTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.String.BasicNewFile";

    /// <inheritdoc/>
    public override string Description => "Tests writing a string to a new file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote string to a new file and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      string content = "Hello, Night Engine!";
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, content);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created.");

      string fileContent = File.ReadAllText(this.TestFilePath);
      Assert.Equal(content, fileContent);
    }
  }

  /// <summary>
  /// Tests writing a basic byte array to a new file using Filesystem.Write.
  /// </summary>
  public class FilesystemWrite_Bytes_BasicNewFileTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Bytes.BasicNewFile";

    /// <inheritdoc/>
    public override string Description => "Tests writing a byte array to a new file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote byte array to a new file and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      byte[] content = Encoding.UTF8.GetBytes("Hello, Bytes!");
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, content);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created.");

      byte[] fileContent = File.ReadAllBytes(this.TestFilePath);
      Assert.Equal(content, fileContent);
    }
  }

  /// <summary>
  /// Tests overwriting an existing file with string data using Filesystem.Write.
  /// </summary>
  public class FilesystemWrite_String_OverwriteExistingFileTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.String.OverwriteExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests overwriting an existing file with a string.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully overwrote existing file with string and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      string initialContent = "Initial content.";
      File.WriteAllText(this.TestFilePath, initialContent); // Pre-populate the file

      string newContent = "New overwritten content!";
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, newContent);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);

      string fileContent = File.ReadAllText(this.TestFilePath);
      Assert.Equal(newContent, fileContent);
    }
  }

  /// <summary>
  /// Tests overwriting an existing file with byte array data using Filesystem.Write.
  /// </summary>
  public class FilesystemWrite_Bytes_OverwriteExistingFileTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Bytes.OverwriteExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests overwriting an existing file with a byte array.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully overwrote existing file with byte array and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      byte[] initialContent = Encoding.UTF8.GetBytes("Initial byte content.");
      File.WriteAllBytes(this.TestFilePath, initialContent); // Pre-populate the file

      byte[] newContent = Encoding.UTF8.GetBytes("New overwritten byte content!");
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, newContent);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);

      byte[] fileContent = File.ReadAllBytes(this.TestFilePath);
      Assert.Equal(newContent, fileContent);
    }
  }

  /// <summary>
  /// Tests writing a portion of a string to a file using the size parameter with Filesystem.Write.
  /// </summary>
  public class FilesystemWrite_String_WithSizeTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.String.WithSize";

    /// <inheritdoc/>
    public override string Description => "Tests writing a string with a specific size parameter.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote partial string using size parameter and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      string fullContent = "This is a full string."; // UTF-8: 22 bytes
      byte[] fullContentBytes = Encoding.UTF8.GetBytes(fullContent);
      long sizeToWrite = 10; // Write first 10 bytes

      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, fullContent, sizeToWrite);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created.");

      byte[] fileBytes = File.ReadAllBytes(this.TestFilePath);
      Assert.Equal(sizeToWrite, fileBytes.Length);

      string expectedWrittenString = Encoding.UTF8.GetString(fullContentBytes, 0, (int)sizeToWrite);
      string actualWrittenString = Encoding.UTF8.GetString(fileBytes);
      Assert.Equal(expectedWrittenString, actualWrittenString);
    }
  }

  /// <summary>
  /// Tests writing a portion of a byte array to a file using the size parameter with Filesystem.Write.
  /// </summary>
  public class FilesystemWrite_Bytes_WithSizeTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Bytes.WithSize";

    /// <inheritdoc/>
    public override string Description => "Tests writing a byte array with a specific size parameter.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote partial byte array using size parameter and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      byte[] fullContent = Encoding.UTF8.GetBytes("This is a full byte array.");
      long sizeToWrite = 12;

      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, fullContent, sizeToWrite);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created.");

      byte[] fileBytes = File.ReadAllBytes(this.TestFilePath);
      Assert.Equal(sizeToWrite, fileBytes.Length);
      Assert.Equal(fullContent.Take((int)sizeToWrite).ToArray(), fileBytes);
    }
  }

  /// <summary>
  /// Tests writing a string where the specified size is larger than the actual string data.
  /// Expects the entire string to be written.
  /// </summary>
  public class FilesystemWrite_String_SizeLargerThanDataTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.String.SizeLargerThanData";

    /// <inheritdoc/>
    public override string Description => "Tests writing a string with size larger than actual data.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote full string when size was larger and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      string content = "Short string.";
      long sizeToWrite = 100; // Larger than content

      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, content, sizeToWrite);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);

      string fileContent = File.ReadAllText(this.TestFilePath);
      Assert.Equal(content, fileContent);
    }
  }

  /// <summary>
  /// Tests writing a byte array where the specified size is larger than the actual byte array data.
  /// Expects the entire byte array to be written.
  /// </summary>
  public class FilesystemWrite_Bytes_SizeLargerThanDataTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Bytes.SizeLargerThanData";

    /// <inheritdoc/>
    public override string Description => "Tests writing a byte array with size larger than actual data.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote full byte array when size was larger and verified content.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      byte[] content = Encoding.UTF8.GetBytes("Short bytes.");
      long sizeToWrite = 200; // Larger than content

      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, content, sizeToWrite);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);

      byte[] fileContent = File.ReadAllBytes(this.TestFilePath);
      Assert.Equal(content, fileContent);
    }
  }

  /// <summary>
  /// Tests writing an empty string to a file using Filesystem.Write.
  /// Expects an empty file to be created.
  /// </summary>
  public class FilesystemWrite_String_EmptyStringTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.String.EmptyString";

    /// <inheritdoc/>
    public override string Description => "Tests writing an empty string.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote an empty string, resulting in an empty file.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, string.Empty);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created for empty string write.");

      string fileContent = File.ReadAllText(this.TestFilePath);
      Assert.Empty(fileContent);
    }
  }

  /// <summary>
  /// Tests writing an empty byte array to a file using Filesystem.Write.
  /// Expects an empty file to be created.
  /// </summary>
  public class FilesystemWrite_Bytes_EmptyArrayTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Bytes.EmptyArray";

    /// <inheritdoc/>
    public override string Description => "Tests writing an empty byte array.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote an empty byte array, resulting in an empty file.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, Array.Empty<byte>());

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created for empty byte array write.");

      byte[] fileContent = File.ReadAllBytes(this.TestFilePath);
      Assert.Empty(fileContent);
    }
  }

  /// <summary>
  /// Tests writing a string with the size parameter explicitly set to 0.
  /// Expects an empty file to be created.
  /// </summary>
  public class FilesystemWrite_String_ZeroSizeTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.String.ZeroSize";

    /// <inheritdoc/>
    public override string Description => "Tests writing a string with size parameter set to 0.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote string with size 0, resulting in an empty file.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, "Some data", 0);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created for zero size string write.");

      string fileContent = File.ReadAllText(this.TestFilePath);
      Assert.Empty(fileContent);
    }
  }

  /// <summary>
  /// Tests writing a byte array with the size parameter explicitly set to 0.
  /// Expects an empty file to be created.
  /// </summary>
  public class FilesystemWrite_Bytes_ZeroSizeTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Bytes.ZeroSize";

    /// <inheritdoc/>
    public override string Description => "Tests writing a byte array with size parameter set to 0.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote byte array with size 0, resulting in an empty file.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, Encoding.UTF8.GetBytes("Some data"), 0);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created for zero size byte array write.");

      byte[] fileContent = File.ReadAllBytes(this.TestFilePath);
      Assert.Empty(fileContent);
    }
  }
}
