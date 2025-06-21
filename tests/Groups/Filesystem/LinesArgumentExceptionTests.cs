// <copyright file="LinesArgumentExceptionTests.cs" company="Night Circle">
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

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests that Filesystem.Lines(filePath) throws ArgumentNullException when filePath is null.
  /// </summary>
  public class FilesystemLines_ThrowsArgumentNullExceptionOnNullPathTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ThrowsArgumentNullExceptionOnNullPath";

    /// <inheritdoc/>
    public override string Description => "Tests that Filesystem.Lines(filePath) throws ArgumentNullException when filePath is null.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Filesystem.Lines(null) correctly threw ArgumentNullException.";

    /// <inheritdoc/>
    public override void Run()
    {
      var exception = Assert.Throws<ArgumentNullException>(() => Night.Filesystem.Lines(null!));
      Assert.Equal("filePath", exception.ParamName);
    }
  }

  /// <summary>
  /// Tests that Filesystem.Lines(filePath) throws ArgumentException when filePath is empty.
  /// </summary>
  public class FilesystemLines_ThrowsArgumentExceptionOnEmptyPathTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.Lines.ThrowsArgumentExceptionOnEmptyPath";

    /// <inheritdoc/>
    public override string Description => "Tests that Filesystem.Lines(filePath) throws ArgumentException when filePath is empty.";

    /// <inheritdoc/>
    public override string SuccessMessage => "Filesystem.Lines(\"\") correctly threw ArgumentException.";

    /// <inheritdoc/>
    public override void Run()
    {
      var exception = Assert.Throws<ArgumentException>(() => Night.Filesystem.Lines(string.Empty));
      Assert.Equal("filePath", exception.ParamName);
      Assert.Contains("File path cannot be empty.", exception.Message);
    }
  }
}
