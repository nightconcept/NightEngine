// <copyright file="FilesystemGroup.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

using Xunit;
using Xunit.Abstractions;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for the Night.Filesystem functionality.
  /// </summary>
  [Collection("SequentialTests")]
  public class FilesystemGroup : TestGroup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesystemGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper for logging.</param>
    public FilesystemGroup(ITestOutputHelper outputHelper)
      : base(outputHelper)
    {
    }

    /// <summary>
    /// Runs the FilesystemLines_ReadStandardFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemLines_ReadStandardFileTest()
    {
      this.Run_GameTestCase(new FilesystemLines_ReadStandardFileTest());
    }

    /// <summary>
    /// Runs the FilesystemLines_ReadEmptyFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemLines_ReadEmptyFileTest()
    {
      this.Run_GameTestCase(new FilesystemLines_ReadEmptyFileTest());
    }

    /// <summary>
    /// Runs the FilesystemLines_FileNotFoundTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemLines_FileNotFoundTest()
    {
      this.Run_GameTestCase(new FilesystemLines_FileNotFoundTest());
    }

    /// <summary>
    /// Runs the FilesystemLines_ReadSingleLineFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemLines_ReadSingleLineFileTest()
    {
      this.Run_GameTestCase(new FilesystemLines_ReadSingleLineFileTest());
    }

    // Tests from GetInfoTests.cs

    /// <summary>
    /// Runs the FilesystemGetInfo_FileExistsTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetInfo_FileExistsTest()
    {
      this.Run_GameTestCase(new FilesystemGetInfo_FileExistsTest());
    }

    /// <summary>
    /// Runs the FilesystemGetInfo_DirectoryExistsTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetInfo_DirectoryExistsTest()
    {
      this.Run_GameTestCase(new FilesystemGetInfo_DirectoryExistsTest());
    }

    /// <summary>
    /// Runs the FilesystemGetInfo_PathDoesNotExistTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetInfo_PathDoesNotExistTest()
    {
      this.Run_GameTestCase(new FilesystemGetInfo_PathDoesNotExistTest());
    }

    /// <summary>
    /// Runs the FilesystemGetInfo_FilterTypeFile_MatchesTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetInfo_FilterTypeFile_MatchesTest()
    {
      this.Run_GameTestCase(new FilesystemGetInfo_FilterTypeFile_MatchesTest());
    }

    /// <summary>
    /// Runs the FilesystemGetInfo_FilterTypeFile_MismatchesTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetInfo_FilterTypeFile_MismatchesTest()
    {
      this.Run_GameTestCase(new FilesystemGetInfo_FilterTypeFile_MismatchesTest());
    }

    // Tests from ReadWriteTests.cs

    /// <summary>
    /// Runs the FilesystemReadBytes_ReadExistingFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemReadBytes_ReadExistingFileTest()
    {
      this.Run_GameTestCase(new FilesystemReadBytes_ReadExistingFileTest());
    }

    /// <summary>
    /// Runs the FilesystemReadText_ReadExistingFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemReadText_ReadExistingFileTest()
    {
      this.Run_GameTestCase(new FilesystemReadText_ReadExistingFileTest());
    }

    /// <summary>
    /// Runs the FilesystemReadBytes_FileNotFoundTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemReadBytes_FileNotFoundTest()
    {
      this.Run_GameTestCase(new FilesystemReadBytes_FileNotFoundTest());
    }

    /// <summary>
    /// Runs the FilesystemReadText_FileNotFoundTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemReadText_FileNotFoundTest()
    {
      this.Run_GameTestCase(new FilesystemReadText_FileNotFoundTest());
    }

    /// <summary>
    /// Runs the FilesystemAppend_AppendToNewFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemAppend_AppendToNewFileTest()
    {
      this.Run_GameTestCase(new FilesystemAppend_AppendToNewFileTest());
    }

    /// <summary>
    /// Runs the FilesystemAppend_AppendToExistingFileTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemAppend_AppendToExistingFileTest()
    {
      this.Run_GameTestCase(new FilesystemAppend_AppendToExistingFileTest());
    }

    /// <summary>
    /// Runs the FilesystemAppend_PartialDataTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemAppend_PartialDataTest()
    {
      this.Run_GameTestCase(new FilesystemAppend_PartialDataTest());
    }

    /// <summary>
    /// Runs the FilesystemAppend_ArgumentValidationTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemAppend_ArgumentValidationTest()
    {
      this.Run_GameTestCase(new FilesystemAppend_ArgumentValidationTest());
    }

    // Tests from DirectoryTests.cs

    /// <summary>
    /// Runs the FilesystemCreateDirectory_NewSingleDirTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemCreateDirectory_NewSingleDirTest()
    {
      this.Run_GameTestCase(new FilesystemCreateDirectory_NewSingleDirTest());
    }

    /// <summary>
    /// Runs the FilesystemCreateDirectory_ExistingDirTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemCreateDirectory_ExistingDirTest()
    {
      this.Run_GameTestCase(new FilesystemCreateDirectory_ExistingDirTest());
    }

    /// <summary>
    /// Runs the FilesystemCreateDirectory_NestedDirTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemCreateDirectory_NestedDirTest()
    {
      this.Run_GameTestCase(new FilesystemCreateDirectory_NestedDirTest());
    }

    /// <summary>
    /// Runs the FilesystemCreateDirectory_ArgumentValidationTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemCreateDirectory_ArgumentValidationTest()
    {
      this.Run_GameTestCase(new FilesystemCreateDirectory_ArgumentValidationTest());
    }

    /// <summary>
    /// Runs the FilesystemGetAppdataDirectory_ReturnsValidPathTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetAppdataDirectory_ReturnsValidPathTest()
    {
      this.Run_GameTestCase(new FilesystemGetAppdataDirectory_ReturnsValidPathTest());
    }

    // Tests from NightFileTests.cs

    /// <summary>
    /// Runs the NightFile_Dispose_DoesNotThrowTest IGame instance.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_NightFile_Dispose_DoesNotThrowTest()
    {
      this.Run_GameTestCase(new NightFile_Dispose_DoesNotThrowTest());
    }
  }
}
