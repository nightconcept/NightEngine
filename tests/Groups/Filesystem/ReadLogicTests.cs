// <copyright file="ReadLogicTests.cs" company="Night Circle">
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

using Night; // For LogManager, MemorySink, LogLevel, LogEntry

// Removed: using Night.Filesystem; // This was incorrect as Filesystem is a static class
using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests Filesystem.Read's capping logic when file length > int.MaxValue,
  /// expecting a warning and an error return due to likely OutOfMemoryException.
  /// </summary>
  public class FilesystemRead_CappingLogicForLargeFileTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Read.CappingLogicForLargeFile";

    /// <inheritdoc/>
    public override string Description => "Tests Filesystem.Read's capping logic when file length > int.MaxValue, expecting a warning and an error return due to likely OOM.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Filesystem.Read logged warning for >int.MaxValue file length and returned an error as expected.";

    /// <inheritdoc/>
    public override void Run()
    {
      string? tempFilePath = null;
      MemorySink? memorySink = null;
      long testFileLength = (long)int.MaxValue + 1;

      try
      {
        tempFilePath = Path.GetTempFileName();

        // Create a sparse file larger than int.MaxValue
        using (var fs = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
        {
          fs.SetLength(testFileLength);
        }

        memorySink = new MemorySink();
        Night.LogManager.AddSink(memorySink);

        var (contents, bytesRead, errorMsg) = Night.Filesystem.Read(Night.Filesystem.ContainerType.Data, tempFilePath, sizeToRead: null);

        // Assertions
        Assert.Null(contents);
        Assert.Null(bytesRead);
        Assert.NotNull(errorMsg);

        // Check for the specific warning log
        bool warningLogged = memorySink.GetEntries().Any(entry =>
          entry.Level == Night.LogLevel.Warning &&
          entry.Message.Contains($"Requested read size ({testFileLength} bytes) for '{tempFilePath}' exceeds int.MaxValue. Capping read at {int.MaxValue} bytes."));
        Assert.True(warningLogged, "Expected warning log for capping read size was not found.");

        // Check if the error message indicates an OutOfMemoryException or a general unexpected error
        Assert.Contains("An unexpected error occurred", errorMsg, StringComparison.OrdinalIgnoreCase);
      }
      finally
      {
        if (memorySink != null)
        {
          Night.LogManager.RemoveSink(memorySink);
        }

        if (tempFilePath != null && File.Exists(tempFilePath))
        {
          File.Delete(tempFilePath);
        }
      }
    }
  }
}
