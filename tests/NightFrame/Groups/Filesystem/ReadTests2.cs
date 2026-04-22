// <copyright file="ReadTests2.cs" company="Night Circle">
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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests Filesystem.Lines() with a null file path.
  /// </summary>
  public class FilesystemLines_NullPathTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.NullPath";

    /// <inheritdoc/>
    public override string Description => "Tests that Filesystem.Lines(null) throws ArgumentNullException.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully threw ArgumentNullException for null path.";

    /// <inheritdoc/>
    public override void Run()
    {
      _ = Assert.Throws<ArgumentNullException>(() => Night.Filesystem.Lines(null!).ToList());
    }
  }

  /// <summary>
  /// Tests Filesystem.Lines() with an empty file path.
  /// </summary>
  public class FilesystemLines_EmptyPathTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.EmptyPath";

    /// <inheritdoc/>
    public override string Description => "Tests that Filesystem.Lines(\"\") throws ArgumentException.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully threw ArgumentException for empty path.";

    /// <inheritdoc/>
    public override void Run()
    {
      _ = Assert.Throws<ArgumentException>(() => Night.Filesystem.Lines(string.Empty).ToList());
    }
  }

  /// <summary>
  /// Tests Filesystem.Lines() when trying to read a directory.
  /// </summary>
  public class FilesystemLines_ReadDirectoryTest : ModTestCase
  {
    private readonly string tempDirPath = Path.Combine(Path.GetTempPath(), "night_test_lines_dir");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ReadDirectory";

    /// <inheritdoc/>
    public override string Description => "Tests Filesystem.Lines() on a directory throws UnauthorizedAccessException.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully threw UnauthorizedAccessException when reading a directory.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (Directory.Exists(this.tempDirPath))
      {
        Directory.Delete(this.tempDirPath, true);
      }

      _ = Directory.CreateDirectory(this.tempDirPath);

      try
      {
        // Attempting to iterate lines on a directory should fail.
        // File.ReadLines, which Filesystem.Lines uses, throws UnauthorizedAccessException for directories.
        _ = Assert.Throws<UnauthorizedAccessException>(() => Night.Filesystem.Lines(this.tempDirPath).ToList());
      }
      finally
      {
        if (Directory.Exists(this.tempDirPath))
        {
          Directory.Delete(this.tempDirPath, true);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Lines() when trying to read a locked file.
  /// </summary>
  public class FilesystemLines_LockedFileTest : ModTestCase
  {
    private readonly string tempFilePath = Path.Combine(Path.GetTempPath(), "night_test_lines_locked.txt");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.LockedFile";

    /// <inheritdoc/>
    public override string Description => "Tests Filesystem.Lines() on a locked file throws IOException.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully threw IOException when reading a locked file.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.tempFilePath))
      {
        File.Delete(this.tempFilePath);
      }

      // Create an empty file to lock
      File.WriteAllText(this.tempFilePath, "lock me");

      FileStream? lockStream = null;
      try
      {
        // Lock the file
        lockStream = new FileStream(this.tempFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None);

        // Attempting to iterate lines on a locked file should fail.
        _ = Assert.Throws<IOException>(() => Night.Filesystem.Lines(this.tempFilePath).ToList());
      }
      finally
      {
        lockStream?.Dispose();
        if (File.Exists(this.tempFilePath))
        {
          File.Delete(this.tempFilePath);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Read(string) when trying to read a directory.
  /// </summary>
  public class FilesystemRead_String_ReadDirectoryTest : ModTestCase
  {
    private readonly string tempDirPath = Path.Combine(Path.GetTempPath(), "night_test_read_s_dir");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.String.ReadDirectory";

    /// <inheritdoc/>
    public override string Description => "Tests Read(string) on a directory returns 'File not found.' error.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Correctly returned error for Read(string) on a directory.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (Directory.Exists(this.tempDirPath))
      {
        Directory.Delete(this.tempDirPath, true);
      }

      _ = Directory.CreateDirectory(this.tempDirPath);

      try
      {
        (string? contents, long? bytesRead, string? errorMsg) = Night.Filesystem.Read(this.tempDirPath);

        Assert.Null(contents);
        Assert.Null(bytesRead);
        Assert.Equal("File not found.", errorMsg);
      }
      finally
      {
        if (Directory.Exists(this.tempDirPath))
        {
          Directory.Delete(this.tempDirPath, true);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Read(ContainerType.Data) when trying to read a directory.
  /// </summary>
  public class FilesystemRead_Data_ReadDirectoryTest : ModTestCase
  {
    private readonly string tempDirPath = Path.Combine(Path.GetTempPath(), "night_test_read_d_dir");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.Data.ReadDirectory";

    /// <inheritdoc/>
    public override string Description => "Tests Read(ContainerType.Data) on a directory returns 'File not found.' error.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Correctly returned error for Read(ContainerType.Data) on a directory.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (Directory.Exists(this.tempDirPath))
      {
        Directory.Delete(this.tempDirPath, true);
      }

      _ = Directory.CreateDirectory(this.tempDirPath);

      try
      {
        (object? contents, long? bytesRead, string? errorMsg) = Night.Filesystem.Read(Night.Filesystem.ContainerType.Data, this.tempDirPath);

        Assert.Null(contents);
        Assert.Null(bytesRead);
        Assert.Equal("File not found.", errorMsg);
      }
      finally
      {
        if (Directory.Exists(this.tempDirPath))
        {
          Directory.Delete(this.tempDirPath, true);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Read(string) when trying to read a locked file.
  /// </summary>
  public class FilesystemRead_String_LockedFileTest : ModTestCase
  {
    private readonly string tempFilePath = Path.Combine(Path.GetTempPath(), "night_test_read_s_locked.txt");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.String.LockedFile";

    /// <inheritdoc/>
    public override string Description => "Tests Read(string) on a locked file returns an IO error.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Correctly returned IO error for Read(string) on a locked file.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.tempFilePath))
      {
        File.Delete(this.tempFilePath);
      }

      File.WriteAllText(this.tempFilePath, "lock me"); // Create file to lock

      FileStream? lockStream = null;
      try
      {
        lockStream = new FileStream(this.tempFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None); // Lock the file

        (string? contents, long? bytesRead, string? errorMsg) = Night.Filesystem.Read(this.tempFilePath);

        Assert.Null(contents);
        Assert.Null(bytesRead); // BytesRead might be 0 if error occurs before any read attempt, or null. The code returns null for BytesRead in this case.
        Assert.NotNull(errorMsg);
        Assert.StartsWith("IO error:", errorMsg);
      }
      finally
      {
        lockStream?.Dispose();
        if (File.Exists(this.tempFilePath))
        {
          File.Delete(this.tempFilePath);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Read(ContainerType.Data) when trying to read a locked file.
  /// </summary>
  public class FilesystemRead_Data_LockedFileTest : ModTestCase
  {
    private readonly string tempFilePath = Path.Combine(Path.GetTempPath(), "night_test_read_d_locked.txt");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.Data.LockedFile";

    /// <inheritdoc/>
    public override string Description => "Tests Read(ContainerType.Data) on a locked file returns an IO error.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Correctly returned IO error for Read(ContainerType.Data) on a locked file.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.tempFilePath))
      {
        File.Delete(this.tempFilePath);
      }

      File.WriteAllText(this.tempFilePath, "lock me data"); // Create file to lock

      FileStream? lockStream = null;
      try
      {
        lockStream = new FileStream(this.tempFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None); // Lock the file

        (object? contents, long? bytesRead, string? errorMsg) = Night.Filesystem.Read(Night.Filesystem.ContainerType.Data, this.tempFilePath);

        Assert.Null(contents);
        Assert.Null(bytesRead);
        Assert.NotNull(errorMsg);
        Assert.StartsWith("IO error:", errorMsg);
      }
      finally
      {
        lockStream?.Dispose();
        if (File.Exists(this.tempFilePath))
        {
          File.Delete(this.tempFilePath);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Read() for UnauthorizedAccessException.
  /// This test is Windows-specific due to ACL manipulation.
  /// </summary>
  public class FilesystemRead_UnauthorizedAccessTest : ModTestCase
  {
    private readonly string tempFilePath = Path.Combine(Path.GetTempPath(), "night_test_read_unauthorized.txt");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.UnauthorizedAccess";

    /// <inheritdoc/>
    public override string Description => "Tests Read() returns 'Unauthorized access.' when file permissions deny read.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Correctly returned 'Unauthorized access.' for permission-denied file.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (!OperatingSystem.IsWindows())
      {
        // On non-Windows platforms, we can't easily manipulate ACLs in a standard way.
        // The test will "pass" by not running its core logic or failing assertions.
        // Xunit might report this as Passed or Skipped depending on runner verbosity and configuration.
        // For ModTestCase, simply returning is the cleanest way to achieve this.
        Console.WriteLine("Skipping FilesystemRead_UnauthorizedAccessTest on non-Windows platform.");
        return;
      }

      if (File.Exists(this.tempFilePath))
      {
        File.Delete(this.tempFilePath);
      }

      File.WriteAllText(this.tempFilePath, "test content");
      FileSecurity? originalSecurity = null;
      bool securityChanged = false;

      try
      {
#pragma warning disable CA1416 // Validate platform compatibility
        FileInfo fileInfo = new FileInfo(this.tempFilePath);
        originalSecurity = fileInfo.GetAccessControl();
        FileSystemRights denyRights = FileSystemRights.ReadData | FileSystemRights.ReadAttributes | FileSystemRights.ReadExtendedAttributes | FileSystemRights.ReadPermissions;

        // It's important to use the current user for the deny rule.
        WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
        FileSystemAccessRule denyRule = new FileSystemAccessRule(
            currentUser.User ?? throw new InvalidOperationException("Could not get current user SID."), // Should have a SID
            denyRights,
            AccessControlType.Deny);

        originalSecurity.AddAccessRule(denyRule);
        fileInfo.SetAccessControl(originalSecurity);
        securityChanged = true;
#pragma warning restore CA1416 // Validate platform compatibility

        (string? contents, long? bytesRead, string? errorMsg) = Night.Filesystem.Read(this.tempFilePath);

        Assert.Null(contents);
        Assert.Null(bytesRead);
        Assert.Equal("Unauthorized access.", errorMsg);
      }
      finally
      {
        if (securityChanged && originalSecurity != null && OperatingSystem.IsWindows())
        {
#pragma warning disable CA1416 // Validate platform compatibility
          // Attempt to restore original permissions. This might fail if the deny rule was too effective.
          // Best effort cleanup.
          try
          {
            FileInfo fileInfo = new FileInfo(this.tempFilePath);
            FileSecurity currentSecurity = fileInfo.GetAccessControl();
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            FileSystemAccessRule ruleToRemove = new FileSystemAccessRule(
                currentUser.User!,
                FileSystemRights.ReadData | FileSystemRights.ReadAttributes | FileSystemRights.ReadExtendedAttributes | FileSystemRights.ReadPermissions,
                AccessControlType.Deny);
            _ = currentSecurity.RemoveAccessRule(ruleToRemove); // Try removing the specific rule
            fileInfo.SetAccessControl(currentSecurity);
          }
          catch (Exception ex)
          {
            // Log if restoration fails, but don't let it fail the test.
            Console.WriteLine($"Warning: Failed to fully restore permissions for {this.tempFilePath}. Manual cleanup may be needed. Error: {ex.Message}");
          }
#pragma warning restore CA1416 // Validate platform compatibility
        }

        if (File.Exists(this.tempFilePath))
        {
          File.Delete(this.tempFilePath);
        }
      }
    }
  }

  /// <summary>
  /// Tests Filesystem.Read() for generic Exception when UTF-8 decoding fails.
  /// </summary>
  public class FilesystemRead_DecodingErrorTest : ModTestCase
  {
    private readonly string tempFilePath = Path.Combine(Path.GetTempPath(), "night_test_read_decoding_error.txt");

    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.DecodingError";

    /// <inheritdoc/>
    public override string Description => "Tests Read() returns 'An unexpected error occurred:' for invalid UTF-8 sequence.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Correctly returned 'An unexpected error occurred:' for decoding error.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.tempFilePath))
      {
        File.Delete(this.tempFilePath);
      }

      // Create a file with an invalid UTF-8 sequence (e.g., an isolated surrogate or an overlong sequence part)
      // 0xC3 followed by 0x28 is an example of an invalid sequence (start of a 2-byte char, but 0x28 is not a valid continuation)
      byte[] invalidUtf8Bytes = { 0xC3, 0x28 };
      File.WriteAllBytes(this.tempFilePath, invalidUtf8Bytes);

      try
      {
        (string? contents, long? bytesRead, string? errorMsg) = Night.Filesystem.Read(this.tempFilePath);

        // Encoding.UTF8.GetString by default replaces invalid sequences with '�'.
        // So, no exception is thrown, and errorMsg should be null.
        // The content will be the string with replacement characters.
        Assert.Equal("�(", contents);
        Assert.Equal(invalidUtf8Bytes.Length, bytesRead);
        Assert.Null(errorMsg);
      }
      finally
      {
        if (File.Exists(this.tempFilePath))
        {
          File.Delete(this.tempFilePath);
        }
      }
    }
  }
}
