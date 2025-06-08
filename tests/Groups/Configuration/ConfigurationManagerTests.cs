// <copyright file="ConfigurationManagerTests.cs" company="Night Circle">
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
using System.Reflection;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Configuration
{
  /// <summary>
  /// Tests the <see cref="ConfigurationManager.IsLoaded"/> property.
  /// </summary>
  public class ConfigurationManager_IsLoadedTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.Manager.IsLoaded";

    /// <inheritdoc/>
    public override string Description => "Tests the IsLoaded property of ConfigurationManager.";

    /// <inheritdoc/>
    public override string SuccessMessage => "ConfigurationManager.IsLoaded property behaves as expected.";

    /// <inheritdoc/>
    public override void Run()
    {
      ConfigurationManagerTestHelper.ResetConfigurationManager();
      Assert.False(ConfigurationManager.IsLoaded, "IsLoaded should be false initially.");

      ConfigurationManager.LoadConfig(); // Load with default (no file)

      Assert.True(ConfigurationManager.IsLoaded, "IsLoaded should be true after LoadConfig is called.");
      ConfigurationManagerTestHelper.ResetConfigurationManager(); // Cleanup
    }
  }

  /// <summary>
  /// Tests that <see cref="ConfigurationManager.LoadConfig(string?)"/> does not reload the configuration
  /// if it has already been loaded.
  /// </summary>
  public class ConfigurationManager_LoadConfig_AlreadyLoadedTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.Manager.LoadConfig.AlreadyLoaded";

    /// <inheritdoc/>
    public override string Description => "Tests that LoadConfig does not reload if already loaded.";

    /// <inheritdoc/>
    public override string SuccessMessage => "LoadConfig correctly skips reloading if already loaded.";

    /// <inheritdoc/>
    public override void Run()
    {
      string? tempDir = null;
      try
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        tempDir = ConfigurationManagerTestHelper.CreateTempConfigDirectory();

        // Initial load (e.g., file not found, uses defaults)
        ConfigurationManager.LoadConfig(tempDir);
        GameConfig initialConfig = ConfigurationManager.CurrentConfig;
        Assert.True(ConfigurationManager.IsLoaded, "Should be loaded after first call.");

        // Create a config file that *would* change settings if loaded
        ConfigurationManagerTestHelper.CreateConfigFile(tempDir, "{\"Window\": {\"Title\": \"Specific Test Title\"}}");

        // Attempt to load again
        ConfigurationManager.LoadConfig(tempDir);
        GameConfig secondConfig = ConfigurationManager.CurrentConfig;

        // Assert that the config hasn't changed because it shouldn't have reloaded
        // This assumes GameConfig has a comparable Title or we check a specific default.
        // For simplicity, we'll assume the default GameConfig() constructor sets a non-"Specific Test Title".
        Assert.NotEqual("Specific Test Title", secondConfig.Window.Title); // Removed message for NotEqual
        Assert.Same(initialConfig, secondConfig); // Removed message for Same
      }
      finally
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        ConfigurationManagerTestHelper.CleanupTempDirectory(tempDir);
      }
    }
  }

  /// <summary>
  /// Tests the behavior of <see cref="ConfigurationManager.LoadConfig(string?)"/> when the
  /// 'config.json' file does not exist. Expects default configuration to be used.
  /// </summary>
  public class ConfigurationManager_LoadConfig_FileNotExistsTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.Manager.LoadConfig.FileNotExists";

    /// <inheritdoc/>
    public override string Description => "Tests LoadConfig behavior when config.json does not exist; should use defaults.";

    /// <inheritdoc/>
    public override string SuccessMessage => "LoadConfig uses default configuration when config.json is not found.";

    /// <inheritdoc/>
    public override void Run()
    {
      string? tempDir = null;
      try
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        tempDir = ConfigurationManagerTestHelper.CreateTempConfigDirectory();
        GameConfig defaultConfig = new GameConfig(); // For comparison

        ConfigurationManager.LoadConfig(tempDir); // tempDir is empty

        Assert.True(ConfigurationManager.IsLoaded, "IsLoaded should be true.");
        Assert.Equal(defaultConfig.Window.Title, ConfigurationManager.CurrentConfig.Window.Title);
        Assert.Equal(defaultConfig.Window.Width, ConfigurationManager.CurrentConfig.Window.Width);
      }
      finally
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        ConfigurationManagerTestHelper.CleanupTempDirectory(tempDir);
      }
    }
  }

  /// <summary>
  /// Tests the behavior of <see cref="ConfigurationManager.LoadConfig(string?)"/> when 'config.json'
  /// is empty. Expects default configuration to be used.
  /// </summary>
  public class ConfigurationManager_LoadConfig_EmptyFileTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.Manager.LoadConfig.EmptyFile";

    /// <inheritdoc/>
    public override string Description => "Tests LoadConfig behavior with an empty config.json; should use defaults.";

    /// <inheritdoc/>
    public override string SuccessMessage => "LoadConfig uses default configuration for an empty config.json.";

    /// <inheritdoc/>
    public override void Run()
    {
      string? tempDir = null;
      try
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        tempDir = ConfigurationManagerTestHelper.CreateTempConfigDirectory();
        ConfigurationManagerTestHelper.CreateConfigFile(tempDir, string.Empty); // Create empty config file
        GameConfig defaultConfig = new GameConfig();

        ConfigurationManager.LoadConfig(tempDir);

        Assert.True(ConfigurationManager.IsLoaded, "IsLoaded should be true.");
        Assert.Equal(defaultConfig.Window.Title, ConfigurationManager.CurrentConfig.Window.Title);
      }
      finally
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        ConfigurationManagerTestHelper.CleanupTempDirectory(tempDir);
      }
    }
  }

  /// <summary>
  /// Tests the behavior of <see cref="ConfigurationManager.LoadConfig(string?)"/> when 'config.json'
  /// contains malformed JSON. Expects default configuration to be used.
  /// </summary>
  public class ConfigurationManager_LoadConfig_InvalidJsonTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.Manager.LoadConfig.InvalidJson";

    /// <inheritdoc/>
    public override string Description => "Tests LoadConfig behavior with a malformed config.json; should use defaults.";

    /// <inheritdoc/>
    public override string SuccessMessage => "LoadConfig uses default configuration for invalid JSON.";

    /// <inheritdoc/>
    public override void Run()
    {
      string? tempDir = null;
      try
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        tempDir = ConfigurationManagerTestHelper.CreateTempConfigDirectory();
        ConfigurationManagerTestHelper.CreateConfigFile(tempDir, "{ \"Window\": { \"Title\": \"Test\", "); // Invalid JSON
        GameConfig defaultConfig = new GameConfig();

        ConfigurationManager.LoadConfig(tempDir);

        Assert.True(ConfigurationManager.IsLoaded, "IsLoaded should be true.");
        Assert.Equal(defaultConfig.Window.Title, ConfigurationManager.CurrentConfig.Window.Title);
      }
      finally
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        ConfigurationManagerTestHelper.CleanupTempDirectory(tempDir);
      }
    }
  }

  /// <summary>
  /// Tests the behavior of <see cref="ConfigurationManager.LoadConfig(string?)"/> when the JSON content
  /// of 'config.json' deserializes to null. Expects default configuration to be used.
  /// </summary>
  public class ConfigurationManager_LoadConfig_DeserializesToNullTest : ModTestCase
  {
    /// <inheritdoc/>
    public override string Name => "Configuration.Manager.LoadConfig.DeserializesToNull";

    /// <inheritdoc/>
    public override string Description => "Tests LoadConfig behavior when JSON deserializes to null; should use defaults.";

    /// <inheritdoc/>
    public override string SuccessMessage => "LoadConfig uses default configuration when deserialization results in null.";

    /// <inheritdoc/>
    public override void Run()
    {
      string? tempDir = null;
      try
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        tempDir = ConfigurationManagerTestHelper.CreateTempConfigDirectory();

        // Valid JSON structure, but perhaps for a different type or content that results in null for GameConfig
        // For GameConfig, an empty object might deserialize to a default GameConfig, not null.
        // A more direct way to test the "loadedConfig == null" path (line 82 in ConfigurationManager)
        // would be if JsonSerializer.Deserialize itself returned null for some valid-looking JSON.
        // This test assumes such a scenario is possible, e.g. "null" as content.
        ConfigurationManagerTestHelper.CreateConfigFile(tempDir, "null");
        GameConfig defaultConfig = new GameConfig();

        ConfigurationManager.LoadConfig(tempDir);

        Assert.True(ConfigurationManager.IsLoaded, "IsLoaded should be true.");
        Assert.Equal(defaultConfig.Window.Title, ConfigurationManager.CurrentConfig.Window.Title);
      }
      finally
      {
        ConfigurationManagerTestHelper.ResetConfigurationManager();
        ConfigurationManagerTestHelper.CleanupTempDirectory(tempDir);
      }
    }
  }

  /// <summary>
  /// Provides helper methods for testing the <see cref="ConfigurationManager"/>.
  /// This includes resetting its static state and managing temporary files/directories for tests.
  /// </summary>
  internal static class ConfigurationManagerTestHelper
  {
    private static FieldInfo? isLoadedField;
    private static FieldInfo? currentConfigField;

    /// <summary>
    /// Initializes static members of the <see cref="ConfigurationManagerTestHelper"/> class.
    /// Retrieves reflection info for private static fields of <see cref="ConfigurationManager"/>.
    /// </summary>
    static ConfigurationManagerTestHelper()
    {
      isLoadedField = typeof(ConfigurationManager).GetField("isLoaded", BindingFlags.NonPublic | BindingFlags.Static);
      currentConfigField = typeof(ConfigurationManager).GetField("currentConfig", BindingFlags.NonPublic | BindingFlags.Static);
    }

    /// <summary>
    /// Resets the static state of the <see cref="ConfigurationManager"/> to its default.
    /// This sets 'isLoaded' to false and 'currentConfig' to a new default <see cref="GameConfig"/> instance.
    /// </summary>
    public static void ResetConfigurationManager()
    {
      isLoadedField?.SetValue(null, false);
      currentConfigField?.SetValue(null, new GameConfig()); // Reset to default config
    }

    /// <summary>
    /// Creates a temporary directory for configuration test files.
    /// </summary>
    /// <returns>The path to the created temporary directory.</returns>
    public static string CreateTempConfigDirectory()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), "NightTest_Config_" + Guid.NewGuid().ToString());
      _ = Directory.CreateDirectory(tempDir);
      return tempDir;
    }

    /// <summary>
    /// Cleans up (deletes) a specified temporary directory and its contents.
    /// </summary>
    /// <param name="directoryPath">The path to the temporary directory to delete.</param>
    public static void CleanupTempDirectory(string? directoryPath)
    {
      if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
      {
        try
        {
          Directory.Delete(directoryPath, true);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error cleaning up temp directory '{directoryPath}': {ex.Message}");
        }
      }
    }

    /// <summary>
    /// Creates a 'config.json' file with the specified content in the given directory.
    /// </summary>
    /// <param name="directoryPath">The directory where the config file will be created.</param>
    /// <param name="content">The string content to write to the config file.</param>
    public static void CreateConfigFile(string directoryPath, string content)
    {
      File.WriteAllText(Path.Combine(directoryPath, "config.json"), content);
    }
  }
}
