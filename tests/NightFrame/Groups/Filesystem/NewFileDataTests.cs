// <copyright file="NewFileDataTests.cs" company="Night Circle">
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

using System.Linq;
using System.Text;

using Night;

using NightTest.Core;

namespace NightTest.Groups.Filesystem;

/// <summary>
/// Tests creating FileData from a byte array.
/// </summary>
public class NewFileDataFromBytesTest : GameTestCase
{
  /// <inheritdoc/>
  public override string Name => "FileData.CreateFromBytes";

  /// <inheritdoc/>
  public override string Description => "Tests creating FileData from a byte array.";

  /// <inheritdoc/>
  protected override void Update(double deltaTime)
  {
    var content = "Hello, Bytes!";
    var bytes = Encoding.UTF8.GetBytes(content);
    var fileData = Night.Filesystem.NewFileData(bytes, "test.bin");

    if (fileData.GetString() != content)
    {
      this.RecordFailure($"GetString() returned '{fileData.GetString()}' instead of '{content}'.");
      return;
    }

    if (fileData.GetSize() != bytes.Length)
    {
      this.RecordFailure($"GetSize() returned {fileData.GetSize()} instead of {bytes.Length}.");
      return;
    }

    if (fileData.GetFilenameHint() != "test.bin")
    {
      this.RecordFailure($"GetFilenameHint() returned '{fileData.GetFilenameHint()}' instead of 'test.bin'.");
      return;
    }

    if (fileData.GetExtension() != ".bin")
    {
      this.RecordFailure($"GetExtension() returned '{fileData.GetExtension()}' instead of '.bin'.");
      return;
    }

    this.RecordSuccess("Successfully created FileData from bytes and verified all properties.");
  }
}

/// <summary>
/// Tests creating FileData from a string.
/// </summary>
public class NewFileDataFromStringTest : GameTestCase
{
  /// <inheritdoc/>
  public override string Name => "FileData.CreateFromString";

  /// <inheritdoc/>
  public override string Description => "Tests creating FileData from a string.";

  /// <inheritdoc/>
  protected override void Update(double deltaTime)
  {
    var content = "Hello, String!";
    var fileData = Night.Filesystem.NewFileData(content, "test.txt");
    var bytes = Encoding.UTF8.GetBytes(content);

    if (fileData.GetString() != content)
    {
      this.RecordFailure($"GetString() returned '{fileData.GetString()}' instead of '{content}'.");
      return;
    }

    if (fileData.GetSize() != bytes.Length)
    {
      this.RecordFailure($"GetSize() returned {fileData.GetSize()} instead of {bytes.Length}.");
      return;
    }

    if (Enumerable.SequenceEqual(fileData.GetBytes(), bytes) == false)
    {
      this.RecordFailure($"GetBytes() did not return the expected byte array.");
      return;
    }

    if (fileData.GetFilenameHint() != "test.txt")
    {
      this.RecordFailure($"GetFilenameHint() returned '{fileData.GetFilenameHint()}' instead of 'test.txt'.");
      return;
    }

    if (fileData.GetExtension() != ".txt")
    {
      this.RecordFailure($"GetExtension() returned '{fileData.GetExtension()}' instead of '.txt'.");
      return;
    }

    this.RecordSuccess("Successfully created FileData from a string and verified all properties.");
  }
}
