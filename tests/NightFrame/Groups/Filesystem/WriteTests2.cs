// <copyright file="WriteTests2.cs" company="Night Circle">
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
using System.Runtime.InteropServices;
using System.Text;

using Night;
using Night.Log;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests argument validation for the Filesystem.Write methods,
  /// ensuring they fail correctly with invalid inputs.
  /// </summary>
  public class FilesystemWrite_ArgumentValidationTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.ArgumentValidation";

    /// <inheritdoc/>
    public override string Description => "Tests argument validation for Filesystem.Write.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Argument validation for Filesystem.Write behaves as expected.";

    /// <inheritdoc/>
    public override void Run()
    {
      string validPath = Path.Combine(Path.GetTempPath(), "NightEngineWriteTests", "valid.txt");
      _ = Directory.CreateDirectory(Path.GetDirectoryName(validPath)!); // Ensure dir exists

      // Null name
      var (success, errorMessage) = Night.Filesystem.Write(null!, "data");
      Assert.False(success);
      Assert.Equal("File name cannot be null or empty.", errorMessage);

      (success, errorMessage) = Night.Filesystem.Write(null!, Array.Empty<byte>());
      Assert.False(success);
      Assert.Equal("File name cannot be null or empty.", errorMessage);

      // Empty name
      (success, errorMessage) = Night.Filesystem.Write(string.Empty, "data");
      Assert.False(success);
      Assert.Equal("File name cannot be null or empty.", errorMessage);

      (success, errorMessage) = Night.Filesystem.Write(string.Empty, Array.Empty<byte>());
      Assert.False(success);
      Assert.Equal("File name cannot be null or empty.", errorMessage);

      // Null string data
      (success, errorMessage) = Night.Filesystem.Write(validPath, (string)null!);
      Assert.False(success);
      Assert.Equal("Data to write cannot be null.", errorMessage);

      // Null byte data
      (success, errorMessage) = Night.Filesystem.Write(validPath, (byte[])null!);
      Assert.False(success);
      Assert.Equal("Data to write cannot be null.", errorMessage);

      // Negative size
      (success, errorMessage) = Night.Filesystem.Write(validPath, "data", -1);
      Assert.False(success);
      Assert.Equal("Size to write cannot be negative.", errorMessage);

      (success, errorMessage) = Night.Filesystem.Write(validPath, Encoding.UTF8.GetBytes("data"), -1);
      Assert.False(success);
      Assert.Equal("Size to write cannot be negative.", errorMessage);

      if (File.Exists(validPath))
      {
        File.Delete(validPath);
      }

      if (Directory.Exists(Path.GetDirectoryName(validPath)) && !Directory.EnumerateFileSystemEntries(Path.GetDirectoryName(validPath)!).Any())
      {
        try
        {
          Directory.Delete(Path.GetDirectoryName(validPath)!);
        }
        catch
        { /* ignore */
        }
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write automatically creates non-existent parent directories
  /// when writing a file.
  /// </summary>
  public class FilesystemWrite_CreateDirectoryTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.CreateDirectory";

    /// <inheritdoc/>
    public override string Description => "Tests that Write creates non-existent parent directories.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully wrote file and created parent directory.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      // Override TestFilePath to include a subdirectory that doesn't exist yet
      string subDir = Guid.NewGuid().ToString("N");
      this.TestFilePath = Path.Combine(this.TestDirectoryPath, subDir, "file_in_subdir.txt");
      string parentOfTestFile = Path.GetDirectoryName(this.TestFilePath)!;

      Assert.False(Directory.Exists(parentOfTestFile), "Subdirectory should not exist before write.");

      string content = "Content in a new subdirectory.";
      var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, content);

      Assert.True(success, $"Write operation failed: {errorMessage}");
      Assert.Null(errorMessage);
      Assert.True(Directory.Exists(parentOfTestFile), "Parent directory was not created.");
      Assert.True(File.Exists(this.TestFilePath), "Test file was not created in subdirectory.");

      string fileContent = File.ReadAllText(this.TestFilePath);
      Assert.Equal(content, fileContent);

      // Cleanup the created subdirectory
      if (Directory.Exists(parentOfTestFile))
      {
        Directory.Delete(parentOfTestFile, true); // Recursive delete for this specific test's subdir
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly when attempting to write to a path
  /// that is an existing directory.
  /// </summary>
  public class FilesystemWrite_PathIsDirectoryTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.PathIsDirectory";

    /// <inheritdoc/>
    public override string Description => "Tests writing to a path that is an existing directory.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed when path is a directory.";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      // TestFilePath from BaseFilesystemWriteTest is a file path.
      // For this test, we want 'name' to be a directory.
      string directoryAsFilePath = Path.Combine(this.TestDirectoryPath, "existing_dir_as_file");
      _ = Directory.CreateDirectory(directoryAsFilePath); // Create it as a directory

      var (success, errorMessage) = Night.Filesystem.Write(directoryAsFilePath, "some data");

      Assert.False(success, "Write operation should have failed for a directory path.");
      Assert.NotNull(errorMessage);

      // Exact error message can be OS-dependent for "is a directory" or "access denied"
      // For .NET FileStream, it's typically UnauthorizedAccessException
      Assert.Contains("Unauthorized access", errorMessage, StringComparison.OrdinalIgnoreCase);

      // Cleanup
      if (Directory.Exists(directoryAsFilePath))
      {
        Directory.Delete(directoryAsFilePath);
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly when the path contains invalid characters.
  /// This should trigger an ArgumentException from the underlying FileStream.
  /// </summary>
  public class FilesystemWrite_Error_InvalidArgumentCharsTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Error.InvalidArgumentChars";

    /// <inheritdoc/>
    public override string Description => "Tests Write with a path containing invalid characters.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed due to invalid characters in path.";

    /// <inheritdoc/>
    public override void Run()
    {
      string invalidFileName;
      string expectedErrorSubstring;
      string problematicCharDisplay;

      if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
      {
        invalidFileName = "file_with_pipe|.txt";

        // On Windows, '|' in filename leads to IOException, wrapped as "IO error:".
        expectedErrorSubstring = "IO error";
        problematicCharDisplay = "|";
      }
      else
      {
        // Use a null character, which should cause ArgumentException from FileStream.
        invalidFileName = "file_with_null\0char.txt";

        // Night.Filesystem.Write wraps ArgumentException as "Argument error:".
        expectedErrorSubstring = "Argument error";
        problematicCharDisplay = "\\0";
      }

      // Define the base directory for test files to ensure it exists.
      // Path.GetDirectoryName will correctly extract this base directory even if invalidFileName contains problematic characters.
      string testFilesBaseDir = Path.Combine(Path.GetTempPath(), "NightEngineWriteTests");
      _ = Directory.CreateDirectory(testFilesBaseDir);

      string invalidPath = Path.Combine(testFilesBaseDir, invalidFileName);

      var (success, errorMessage) = Night.Filesystem.Write(invalidPath, "some data");

      Assert.False(success, $"Write operation should have failed. OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}, Char: '{problematicCharDisplay}', Path: {invalidPath}");
      Assert.NotNull(errorMessage); // Ensure there is an error message.
      Assert.Contains(expectedErrorSubstring, errorMessage, StringComparison.OrdinalIgnoreCase);

      // Cleanup: Attempt to delete the directory if it was created and is empty.
      // The invalid file itself wouldn't have been created.
      string? dirName = Path.GetDirectoryName(invalidPath);
      if (dirName != null && Directory.Exists(dirName) && !Directory.EnumerateFileSystemEntries(dirName).Any())
      {
        try
        {
          Directory.Delete(dirName);
        }
        catch (IOException)
        { /* Ignore cleanup errors */
        }
      }
      else if (dirName != null && Directory.Exists(dirName) && Directory.GetFiles(dirName).Length == 0 && Directory.GetDirectories(dirName).Length == 0)
      {
        try
        {
          Directory.Delete(dirName);
        }
        catch (IOException)
        { /* Ignore cleanup errors */
        }
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly when the path is too long.
  /// </summary>
  public class FilesystemWrite_Error_PathTooLongTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Error.PathTooLong";

    /// <inheritdoc/>
    public override string Description => "Tests Write with a path that exceeds system limits.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed due to path being too long.";

    /// <inheritdoc/>
    public override void Run()
    {
      string baseDir = Path.Combine(Path.GetTempPath(), "NightEngineWriteTests");
      _ = Directory.CreateDirectory(baseDir);

      // Create a very long path. Max path is often around 260, but varies.
      // Let's aim for something definitely too long.
      string excessivelyLongName = new string('a', 300);
      string longPath = Path.Combine(baseDir, excessivelyLongName, excessivelyLongName, $"{excessivelyLongName}.txt");

      var (success, errorMessage) = Night.Filesystem.Write(longPath, "some data");

      Assert.False(success, "Write operation should have failed for a path that is too long.");
      Assert.NotNull(errorMessage);
      Assert.Equal("The specified path, file name, or both exceed the system-defined maximum length.", errorMessage);

      // Cleanup
      if (Directory.Exists(baseDir))
      {
        try
        {
          Directory.Delete(baseDir, true);
        }
        catch (Exception)
        { /* Ignore cleanup errors for problematic long paths */
        }
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly when the path refers to an unmapped drive.
  /// </summary>
  public class FilesystemWrite_Error_DirectoryNotFoundUnmappedDriveTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Error.DirectoryNotFoundUnmappedDrive";

    /// <inheritdoc/>
    public override string Description => "Tests Write to an unmapped drive.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed for an unmapped drive.";

    /// <inheritdoc/>
    public override void Run()
    {
      // Assume Z: is an unmapped drive. This is a common convention for such tests.
      string unmappedPath = @"Z:\non_existent_dir_on_unmapped_drive\file.txt";

      var (success, errorMessage) = Night.Filesystem.Write(unmappedPath, "some data");

      Assert.False(success, "Write operation should have failed for an unmapped drive.");
      Assert.NotNull(errorMessage);
      Assert.Equal("The specified path is invalid (for example, it is on an unmapped drive).", errorMessage);
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly due to an IOException (e.g., file locked).
  /// </summary>
  public class FilesystemWrite_Error_IOExceptionLockedTest : BaseFilesystemWriteTest
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Error.IOExceptionLockedTest";

    /// <inheritdoc/>
    public override string Description => "Tests Write to a file that is locked.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed due to IOException (file locked).";

    /// <inheritdoc/>
    protected override void ExecuteWriteTestLogic()
    {
      // Create and lock the file
      using (global::System.IO.FileStream lockStream = new global::System.IO.FileStream(this.TestFilePath, global::System.IO.FileMode.Create, global::System.IO.FileAccess.ReadWrite, global::System.IO.FileShare.None))
      {
        lockStream.Write(Encoding.UTF8.GetBytes("locked content"), 0, 14);
        lockStream.Flush();

        var (success, errorMessage) = Night.Filesystem.Write(this.TestFilePath, "attempt to overwrite locked file");

        Assert.False(success, "Write operation should have failed because the file is locked.");
        Assert.NotNull(errorMessage);
        Assert.StartsWith("IO error:", errorMessage, StringComparison.OrdinalIgnoreCase);
        bool accessConflictMessage = errorMessage.Contains("being used by another process", StringComparison.OrdinalIgnoreCase) ||
                                     errorMessage.Contains("access to the path", StringComparison.OrdinalIgnoreCase);
        Assert.True(accessConflictMessage, $"Expected an access conflict message, but got: {errorMessage}");
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly due to a SecurityException.
  /// Triggering this reliably is hard; this attempts a common scenario but may be environment-dependent.
  /// </summary>
  public class FilesystemWrite_Error_SecurityExceptionTest : ModTestCase
  {
    private static readonly ILogger Logger = LogManager.GetLogger(nameof(FilesystemWrite_Error_SecurityExceptionTest));

    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Error.SecurityException";

    /// <inheritdoc/>
    public override string Description => "Tests Write when a SecurityException occurs (best effort).";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed with a security error.";

    /// <inheritdoc/>
    public override void Run()
    {
      string potentiallyRestrictedPath = @"\\.\GLOBALROOT\Device\Null\ProtectedFile.txt";

      var (success, errorMessage) = Night.Filesystem.Write(potentiallyRestrictedPath, "some data");

      Assert.False(success, "Write operation should have failed.");
      Assert.NotNull(errorMessage);

      bool isExpectedSecurityError = errorMessage == "A security error occurred.";
      bool isUnauthorized = errorMessage.StartsWith("Unauthorized access", StringComparison.OrdinalIgnoreCase);
      bool isIOError = errorMessage.StartsWith("IO error", StringComparison.OrdinalIgnoreCase);
      bool isNotSupported = errorMessage.StartsWith("Operation not supported", StringComparison.OrdinalIgnoreCase);

      Assert.True(
        isExpectedSecurityError || isUnauthorized || isIOError || isNotSupported,
        $"Expected a security-related error, UnauthorizedAccess, IOException or NotSupportedException, but got: {errorMessage}");

      if (isExpectedSecurityError)
      {
        Logger.Info("SecurityException test successfully triggered the specific SecurityException handler.");
      }
      else
      {
        Logger.Warn($"SecurityException test did not trigger the specific SecurityException handler with path '{potentiallyRestrictedPath}'. It triggered: {errorMessage}");
      }
    }
  }

  /// <summary>
  /// Tests that Filesystem.Write fails correctly when the path is not supported (e.g., a device).
  /// </summary>
  public class FilesystemWrite_Error_NotSupportedTest : ModTestCase
  {
    private static readonly ILogger Logger = LogManager.GetLogger(nameof(FilesystemWrite_Error_NotSupportedTest));

    /// <inheritdoc/>
    public override string Name => "Filesystem.Write.Error.NotSupported";

    /// <inheritdoc/>
    public override string Description => "Tests Write with a path that is not supported (e.g., CON).";

    /// <inheritdoc/>
    public override string SuccessMessage => "Write operation correctly failed due to an unsupported path.";

    /// <inheritdoc/>
    public override void Run()
    {
      string devicePath;
      if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
      {
        devicePath = "CON"; // Console output, not a regular file
      }
      else
      {
        Logger.Info("Skipping NotSupportedException test for non-Windows platform with 'CON' as it's Windows-specific. Awaiting a better cross-platform unsupported path example.");
        Assert.True(true, "Test skipped on non-Windows for 'CON' device path.");
        return;
      }

      var (success, errorMessage) = Night.Filesystem.Write(devicePath, "some data");

      Assert.False(success, "Write operation should have failed for an unsupported path.");
      Assert.NotNull(errorMessage);
      Assert.StartsWith("Operation not supported", errorMessage, StringComparison.OrdinalIgnoreCase);
    }
  }
}
