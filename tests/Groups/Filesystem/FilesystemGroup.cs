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
  public partial class FilesystemGroup : TestGroup
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
    /// Runs all Filesystem.Lines mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemLines_ModTests()
    {
      this.Run_ModTestCase(new FilesystemLines_ReadStandardFileTest());
      this.Run_ModTestCase(new FilesystemLines_ReadEmptyFileTest());
      this.Run_ModTestCase(new FilesystemLines_FileNotFoundTest());
      this.Run_ModTestCase(new FilesystemLines_ReadSingleLineFileTest());
      this.Run_ModTestCase(new FilesystemLines_NullPathTest());
      this.Run_ModTestCase(new FilesystemLines_EmptyPathTest());
      this.Run_ModTestCase(new FilesystemLines_ReadDirectoryTest());
      this.Run_ModTestCase(new FilesystemLines_LockedFileTest());
      this.Run_ModTestCase(new FilesystemLines_ThrowsArgumentNullExceptionOnNullPathTest());
      this.Run_ModTestCase(new FilesystemLines_ThrowsArgumentExceptionOnEmptyPathTest());
    }

    // Tests from FilesystemGetInfoTests.cs

    /// <summary>
    /// Runs all Filesystem.GetInfo mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetInfo_ModTests()
    {
      this.Run_ModTestCase(new FilesystemGetInfo_NullPath_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_EmptyPath_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PathDoesNotExist_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_FileExists_NoFilter_ReturnsFileInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_FileExists_MatchingFilter_ReturnsFileInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_FileExists_NonMatchingFilter_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_DirectoryExists_NoFilter_ReturnsDirectoryInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_DirectoryExists_MatchingFilter_ReturnsDirectoryInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_DirectoryExists_NonMatchingFilter_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateNullInfoObject_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateValidInfo_FileExists_PopulatesAndReturnsInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateValidInfo_DirectoryExists_PopulatesAndReturnsInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateValidInfo_PathDoesNotExist_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateWithFilter_NullInfoObject_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateWithFilter_FileExists_MatchingFilter_PopulatesInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateWithFilter_FileExists_NonMatchingFilter_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateWithFilter_DirectoryExists_MatchingFilter_PopulatesInfoTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateWithFilter_DirectoryExists_NonMatchingFilter_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_PopulateWithFilter_PathDoesNotExist_ReturnsNullTest());
      this.Run_ModTestCase(new FilesystemGetInfo_SymbolicLinkTest());
    }

    // Tests from ReadWriteTests.cs

    /// <summary>
    /// Runs all Filesystem.ReadBytes mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemReadBytes_ModTests()
    {
      this.Run_ModTestCase(new FilesystemReadBytes_ReadExistingFileTest());
      this.Run_ModTestCase(new FilesystemReadBytes_FileNotFoundTest());
    }

    /// <summary>
    /// Runs all Filesystem.ReadText mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemReadText_ModTests()
    {
      this.Run_ModTestCase(new FilesystemReadText_ReadExistingFileTest());
      this.Run_ModTestCase(new FilesystemReadText_FileNotFoundTest());
    }

    /// <summary>
    /// Runs all Filesystem.Append mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemAppend_ModTests()
    {
      this.Run_ModTestCase(new FilesystemAppend_AppendToNewFileTest());
      this.Run_ModTestCase(new FilesystemAppend_AppendToExistingFileTest());
      this.Run_ModTestCase(new FilesystemAppend_PartialDataTest());
      this.Run_ModTestCase(new FilesystemAppend_ArgumentValidationTest());
    }

    /// <summary>
    /// Runs all Filesystem.Read mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemRead_ModTests()
    {
      this.Run_ModTestCase(new FilesystemRead_String_ReadExistingFileTest());
      this.Run_ModTestCase(new FilesystemRead_Data_ReadExistingFileTest());
      this.Run_ModTestCase(new FilesystemRead_FileNotFoundTest());
      this.Run_ModTestCase(new FilesystemRead_String_PartialReadTest());
      this.Run_ModTestCase(new FilesystemRead_Data_PartialReadTest());
      this.Run_ModTestCase(new FilesystemRead_EmptyFileTest());
      this.Run_ModTestCase(new FilesystemRead_ArgumentValidationTest());
      this.Run_ModTestCase(new FilesystemRead_String_ReadDirectoryTest());
      this.Run_ModTestCase(new FilesystemRead_Data_ReadDirectoryTest());
      this.Run_ModTestCase(new FilesystemRead_String_LockedFileTest());
      this.Run_ModTestCase(new FilesystemRead_Data_LockedFileTest());
      this.Run_ModTestCase(new FilesystemRead_UnauthorizedAccessTest());
      this.Run_ModTestCase(new FilesystemRead_DecodingErrorTest());
      this.Run_ModTestCase(new FilesystemRead_CappingLogicForLargeFileTest());
    }

    /// <summary>
    /// Runs all Filesystem.CreateDirectory mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemCreateDirectory_ModTests()
    {
      this.Run_ModTestCase(new FilesystemCreateDirectory_NewSingleDirTest());
      this.Run_ModTestCase(new FilesystemCreateDirectory_ExistingDirTest());
      this.Run_ModTestCase(new FilesystemCreateDirectory_NestedDirTest());
      this.Run_ModTestCase(new FilesystemCreateDirectory_ArgumentValidationTest());
    }

    /// <summary>
    /// Runs a test to ensure <see cref="Night.Filesystem.GetAppdataDirectory"/> returns a valid path.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetAppdataDirectory_ReturnsValidPathTest()
    {
      this.Run_ModTestCase(new FilesystemGetAppdataDirectory_ReturnsValidPathTest());
    }

    /// <summary>
    /// Runs all Filesystem.GetSaveDirectory mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemGetSaveDirectory_ModTests()
    {
      this.Run_ModTestCase(new GetSaveDirectory_DefaultIdentityTest());
      this.Run_ModTestCase(new GetSaveDirectory_CustomIdentityTest());
    }

    /// <summary>
    /// Runs all <see cref="Night.NightFile"/> mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_NightFile_ModTests()
    {
      this.Run_ModTestCase(new NightFile_Constructor());
      this.Run_ModTestCase(new NightFile_OpenClose());
      this.Run_ModTestCase(new NightFile_OpenModes());
      this.Run_ModTestCase(new NightFile_OpenInvalidCases());
      this.Run_ModTestCase(new NightFile_Read_Full());
      this.Run_ModTestCase(new NightFile_Read_Bytes());
      this.Run_ModTestCase(new NightFile_Read_BytesCounted());
      this.Run_ModTestCase(new NightFile_Dispose());
    }

    /// <summary>
    /// Runs all <see cref="Night.Filesystem.Write(string, byte[], long?)" /> mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FilesystemWrite_ModTests()
    {
      this.Run_ModTestCase(new FilesystemWrite_String_BasicNewFileTest());
      this.Run_ModTestCase(new FilesystemWrite_Bytes_BasicNewFileTest());
      this.Run_ModTestCase(new FilesystemWrite_String_OverwriteExistingFileTest());
      this.Run_ModTestCase(new FilesystemWrite_Bytes_OverwriteExistingFileTest());
      this.Run_ModTestCase(new FilesystemWrite_String_WithSizeTest());
      this.Run_ModTestCase(new FilesystemWrite_Bytes_WithSizeTest());
      this.Run_ModTestCase(new FilesystemWrite_String_SizeLargerThanDataTest());
      this.Run_ModTestCase(new FilesystemWrite_Bytes_SizeLargerThanDataTest());
      this.Run_ModTestCase(new FilesystemWrite_String_EmptyStringTest());
      this.Run_ModTestCase(new FilesystemWrite_Bytes_EmptyArrayTest());
      this.Run_ModTestCase(new FilesystemWrite_String_ZeroSizeTest());
      this.Run_ModTestCase(new FilesystemWrite_Bytes_ZeroSizeTest());
      this.Run_ModTestCase(new FilesystemWrite_ArgumentValidationTest());
      this.Run_ModTestCase(new FilesystemWrite_CreateDirectoryTest());
      this.Run_ModTestCase(new FilesystemWrite_PathIsDirectoryTest());
      this.Run_ModTestCase(new FilesystemWrite_Error_InvalidArgumentCharsTest());
      this.Run_ModTestCase(new FilesystemWrite_Error_IOExceptionLockedTest());

      // TODO: Fix these tests
      // this.Run_ModTestCase(new FilesystemWrite_Error_PathTooLongTest());
      // this.Run_ModTestCase(new FilesystemWrite_Error_SecurityExceptionTest());
      // this.Run_ModTestCase(new FilesystemWrite_Error_NotSupportedTest());
      // this.Run_ModTestCase(new FilesystemWrite_Error_DirectoryNotFoundUnmappedDriveTest());
    }

    /// <summary>
    /// Runs all <see cref="Night.FileSystemInfo" /> mod test cases.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FileSystemInfo_ModTests()
    {
      this.Run_ModTestCase(new FileSystemInfo_Constructor_DefaultValues());
      this.Run_ModTestCase(new FileSystemInfo_Constructor());
    }

    /// <summary>
    /// Runs the FileDropEventTest.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_FileDropEventTest()
    {
      this.Run_ModTestCase(new FileDropEventTest());
    }
  }
}
