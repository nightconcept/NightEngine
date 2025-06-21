// <copyright file="ReadBytesTests.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.Filesystem.ReadBytes().
  /// </summary>
  public class FilesystemReadBytes_ReadExistingFileTest : ModTestCase
  {
    private readonly string testFileName = Path.Combine(Path.GetTempPath(), "night_test_readbytes_file.bin");
    private readonly byte[] expectedContent = { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x4E, 0x69, 0x67, 0x68, 0x74 }; // "Hello Night"

    /// <inheritdoc/>
    public override string Name => "Filesystem.ReadBytes.ReadExistingFile";

    /// <inheritdoc/>
    public override string Description => "Tests ReadBytes for an existing binary file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully read bytes from an existing file.";

    /// <inheritdoc/>
    public override void Run()
    {
      try
      {
        File.WriteAllBytes(this.testFileName, this.expectedContent);
        byte[] actualContent = Night.Filesystem.ReadBytes(this.testFileName);
        Assert.Equal(this.expectedContent, actualContent);
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
  /// Tests Night.Filesystem.ReadBytes for a non-existent file.
  /// </summary>
  public class FilesystemReadBytes_FileNotFoundTest : ModTestCase
  {
    private readonly string nonExistentFile = Path.Combine(Path.GetTempPath(), "night_test_readbytes_nonexistent.bin");

    /// <inheritdoc/>
    public override string Name => "Filesystem.ReadBytes.FileNotFound";

    /// <inheritdoc/>
    public override string Description => "Tests ReadBytes throws FileNotFoundException for a non-existent file.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully caught FileNotFoundException for ReadBytes.";

    /// <inheritdoc/>
    public override void Run()
    {
      if (File.Exists(this.nonExistentFile))
      {
        File.Delete(this.nonExistentFile); // Ensure it doesn't exist
      }

      _ = Assert.Throws<FileNotFoundException>(() => Night.Filesystem.ReadBytes(this.nonExistentFile));
    }
  }
}
