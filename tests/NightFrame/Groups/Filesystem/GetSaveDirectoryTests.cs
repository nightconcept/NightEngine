// <copyright file="GetSaveDirectoryTests.cs" company="Night Circle">
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
using System.Runtime.InteropServices;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Filesystem
{
  /// <summary>
  /// Tests for Night.Filesystem.GetSaveDirectory().
  /// </summary>
  public class GetSaveDirectory_DefaultIdentityTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Filesystem.GetSaveDirectory.DefaultIdentity";

    /// <inheritdoc/>
    public override string Description => "Tests GetSaveDirectory with the default identity and ensures directory creation.";

    /// <inheritdoc/>
    public override string SuccessMessage => "GetSaveDirectory with default identity returned a valid, existing path.";

    /// <inheritdoc/>
    public override void Run()
    {
      Night.Filesystem.SetIdentity(null); // Ensure default identity
      var saveDir = Night.Filesystem.GetSaveDirectory();

      Assert.False(string.IsNullOrWhiteSpace(saveDir), "Save directory path should not be null or whitespace.");
      Assert.True(Directory.Exists(saveDir), "Save directory should be created by the method.");

      var expectedIdentity = "NightDefault";
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var expectedPath = Path.Combine(appData, "Night", expectedIdentity);
        Assert.Equal(expectedPath, saveDir);
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        var appSupport = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support");
        var expectedPath = Path.Combine(appSupport, "Night", expectedIdentity);
        Assert.Equal(expectedPath, saveDir);
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
      {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var expectedBase = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(home, ".local", "share");
        var expectedPath = Path.Combine(expectedBase, "night", expectedIdentity);
        Assert.Equal(expectedPath, saveDir);
      }
    }
  }

  /// <summary>
  /// Tests GetSaveDirectory with a custom identity.
  /// </summary>
  public class GetSaveDirectory_CustomIdentityTest : ModTestCase
  {
    private const string CustomIdentity = "MyCustomGame";

    /// <inheritdoc/>
    public override string Name => "Filesystem.GetSaveDirectory.CustomIdentity";

    /// <inheritdoc/>
    public override string Description => "Tests GetSaveDirectory with a custom identity.";

    /// <inheritdoc/>
    public override string SuccessMessage => "GetSaveDirectory with custom identity returned the correct path.";

    /// <inheritdoc/>
    public override void Run()
    {
      Night.Filesystem.SetIdentity(CustomIdentity);
      var saveDir = Night.Filesystem.GetSaveDirectory();

      try
      {
        Assert.True(Directory.Exists(saveDir), "Save directory with custom identity should be created.");
        Assert.EndsWith(CustomIdentity, saveDir);
      }
      finally
      {
        // Cleanup: remove the custom directory
        if (Directory.Exists(saveDir))
        {
          var parentDir = Directory.GetParent(saveDir);
          if (parentDir != null && (parentDir.Name == "Night" || parentDir.Name == "night"))
          {
            Directory.Delete(saveDir, true);
          }
        }

        Night.Filesystem.SetIdentity(null); // Reset to default
      }
    }
  }
}
