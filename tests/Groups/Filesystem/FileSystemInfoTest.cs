// <copyright file="FileSystemInfoTest.cs" company="Night Circle">
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
  /// Tests for Night.FileSystemInfo() with no parameter values.
  /// </summary>
  public class FileSystemInfo_Constructor_DefaultValues : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Night.FileSystemInfo.Constructor.DefaultValues";

    /// <inheritdoc/>
    public override string Description => "Tests FileSystemInfo constructor with default values.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully created FileSystemInfo with default values.";

    /// <inheritdoc/>
    public override void Run()
    {
      var info = new Night.FileSystemInfo();
      Assert.NotNull(info);
      Assert.Equal(FileType.None, info.Type);
      Assert.Null(info.Size);
      Assert.Null(info.ModTime);
    }
  }

  /// <summary>
  /// Tests for Night.FileSystemInfo() with parameter values.
  /// </summary>
  public class FileSystemInfo_Constructor : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Night.FileSystemInfo.Constructor";

    /// <inheritdoc/>
    public override string Description => "Tests FileSystemInfo constructor with parameter values.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Successfully created FileSystemInfo with specified values.";

    /// <inheritdoc/>
    public override void Run()
    {
      var info = new Night.FileSystemInfo(FileType.File, 12345, 1622547800);
      Assert.NotNull(info);
      Assert.Equal(FileType.File, info.Type);
      Assert.Equal(12345, info.Size);
      Assert.Equal(1622547800, info.ModTime);
    }
  }
}
