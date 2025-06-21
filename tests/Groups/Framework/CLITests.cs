// <copyright file="CLITests.cs" company="Night Circle">
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
using System.IO; // Required for Path.Combine, AppContext.BaseDirectory
using System.Linq;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Framework
{
  /// <summary>
  /// Tests for the CLI constructor focusing on default values with null or empty arguments.
  /// </summary>
  public class NightCLI_Constructor_DefaultValuesTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.DefaultValues";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor with null and empty arguments, ensuring default property values.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI constructor default values verified successfully.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act: Null arguments
      var cliNull = new Night.CLI(null!); // Test with null, suppress warning as it's a test case

      // Assert: Null arguments
      Assert.False(cliNull.IsSilentMode, "IsSilentMode should be false for null args.");
      Assert.Null(cliNull.ParsedLogLevel);
      Assert.False(cliNull.IsDebugMode, "IsDebugMode should be false for null args.");
      Assert.False(cliNull.EnableSessionLog, "EnableSessionLog should be false for null args.");
      Assert.Empty(cliNull.RemainingArgs);

      // Arrange & Act: Empty arguments
      var cliEmpty = new Night.CLI(Array.Empty<string>());

      // Assert: Empty arguments
      Assert.False(cliEmpty.IsSilentMode, "IsSilentMode should be false for empty args.");
      Assert.Null(cliEmpty.ParsedLogLevel);
      Assert.False(cliEmpty.IsDebugMode, "IsDebugMode should be false for empty args.");
      Assert.False(cliEmpty.EnableSessionLog, "EnableSessionLog should be false for empty args.");
      Assert.Empty(cliEmpty.RemainingArgs);
    }
  }

  /// <summary>
  /// Tests for the CLI constructor focusing on silent mode flags (-s, --silent).
  /// </summary>
  public class NightCLI_Constructor_SilentModeTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.SilentMode";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor for -s and --silent flags.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI silent mode flags parsed correctly.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act: -s
      var cliShort = new Night.CLI(new[] { "-s" });

      // Assert: -s
      Assert.True(cliShort.IsSilentMode, "IsSilentMode should be true for '-s'.");

      // Arrange & Act: --silent
      var cliLong = new Night.CLI(new[] { "--silent" });

      // Assert: --silent
      Assert.True(cliLong.IsSilentMode, "IsSilentMode should be true for '--silent'.");

      // Arrange & Act: Case insensitivity
      var cliCaps = new Night.CLI(new[] { "-S" });

      // Assert: Case insensitivity
      Assert.True(cliCaps.IsSilentMode, "IsSilentMode should be true for '-S' (case-insensitive).");
    }
  }

  /// <summary>
  /// Tests for the CLI constructor focusing on the --log-level argument.
  /// </summary>
  public class NightCLI_Constructor_LogLevelTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.LogLevel";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor for --log-level with valid, invalid, and missing values.";

    /// <inheritdoc />
    public override string SuccessMessage => "--log-level argument parsing verified.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act: Valid log level
      var cliValid = new Night.CLI(new[] { "--log-level", "Debug" });

      // Assert: Valid log level
      Assert.Equal(LogLevel.Debug, cliValid.ParsedLogLevel);
      Assert.Empty(cliValid.RemainingArgs);

      // Arrange & Act: Valid log level (case-insensitive)
      var cliValidCase = new Night.CLI(new[] { "--log-level", "wArNiNg" });

      // Assert: Valid log level (case-insensitive)
      Assert.Equal(LogLevel.Warning, cliValidCase.ParsedLogLevel);
      Assert.Empty(cliValidCase.RemainingArgs);

      // Arrange & Act: Invalid log level
      var cliInvalid = new Night.CLI(new[] { "--log-level", "InvalidValue" });

      // Assert: Invalid log level
      Assert.Null(cliInvalid.ParsedLogLevel);
      Assert.Equal(2, cliInvalid.RemainingArgs.Count);
      Assert.Contains("--log-level", cliInvalid.RemainingArgs);
      Assert.Contains("InvalidValue", cliInvalid.RemainingArgs);

      // Arrange & Act: Missing log level value
      var cliMissingValue = new Night.CLI(new[] { "--log-level" });

      // Assert: Missing log level value
      Assert.Null(cliMissingValue.ParsedLogLevel);
      _ = Assert.Single(cliMissingValue.RemainingArgs);
      Assert.Contains("--log-level", cliMissingValue.RemainingArgs);

      // Arrange & Act: --log-level followed by another option (missing value)
      var cliMissingValueFollowedByOpt = new Night.CLI(new[] { "--log-level", "--debug" });

      // Assert: --log-level followed by another option
      Assert.Null(cliMissingValueFollowedByOpt.ParsedLogLevel);
      Assert.False(cliMissingValueFollowedByOpt.IsDebugMode, "--debug should not be parsed as IsDebugMode when it's consumed by --log-level.");
      Assert.Equal(2, cliMissingValueFollowedByOpt.RemainingArgs.Count); // Both --log-level and --debug should be in remaining args
      Assert.Contains("--log-level", cliMissingValueFollowedByOpt.RemainingArgs);
      Assert.Contains("--debug", cliMissingValueFollowedByOpt.RemainingArgs); // --debug is treated as the invalid value for --log-level
    }
  }

  /// <summary>
  /// Tests for the CLI constructor focusing on the --debug flag.
  /// </summary>
  public class NightCLI_Constructor_DebugModeTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.DebugMode";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor for --debug flag.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI debug mode flag parsed correctly.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act
      var cli = new Night.CLI(new[] { "--debug" });

      // Assert
      Assert.True(cli.IsDebugMode, "IsDebugMode should be true for '--debug'.");

      // Arrange & Act: Case insensitivity
      var cliCaps = new Night.CLI(new[] { "--DEBUG" });

      // Assert: Case insensitivity
      Assert.True(cliCaps.IsDebugMode, "IsDebugMode should be true for '--DEBUG' (case-insensitive).");
    }
  }

  /// <summary>
  /// Tests for the CLI constructor focusing on the --session-log flag.
  /// </summary>
  public class NightCLI_Constructor_SessionLogTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.SessionLog";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor for --session-log flag.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI session log flag parsed correctly.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act
      var cli = new Night.CLI(new[] { "--session-log" });

      // Assert
      Assert.True(cli.EnableSessionLog, "EnableSessionLog should be true for '--session-log'.");
    }
  }

  /// <summary>
  /// Tests for the CLI constructor focusing on how unrecognized arguments are handled.
  /// </summary>
  public class NightCLI_Constructor_RemainingArgsTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.RemainingArgs";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor for handling of unrecognized arguments.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI remaining arguments handled correctly.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act
      var cli = new Night.CLI(new[] { "arg1", "--unknown", "value", "-x" });

      // Assert
      Assert.Equal(4, cli.RemainingArgs.Count);
      Assert.Contains("arg1", cli.RemainingArgs);
      Assert.Contains("--unknown", cli.RemainingArgs);
      Assert.Contains("value", cli.RemainingArgs);
      Assert.Contains("-x", cli.RemainingArgs);
    }
  }

  /// <summary>
  /// Tests for the CLI constructor focusing on parsing a combination of different arguments.
  /// </summary>
  public class NightCLI_Constructor_CombinedArgsTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.Constructor.CombinedArgs";

    /// <inheritdoc />
    public override string Description => "Tests CLI constructor with a combination of arguments.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI combined arguments parsed correctly.";

    /// <inheritdoc />
    public override void Run()
    {
      // Arrange & Act
      var cli = new Night.CLI(new[] { "-s", "--log-level", "Error", "remaining1", "--debug", "remaining2", "--session-log" });

      // Assert
      Assert.True(cli.IsSilentMode);
      Assert.Equal(LogLevel.Error, cli.ParsedLogLevel);
      Assert.True(cli.IsDebugMode);
      Assert.True(cli.EnableSessionLog);
      Assert.Equal(2, cli.RemainingArgs.Count);
      Assert.Contains("remaining1", cli.RemainingArgs);
      Assert.Contains("remaining2", cli.RemainingArgs);
    }
  }

  /// <summary>
  /// Tests for the CLI ApplySettings method focusing on how it affects LogManager.MinLevel.
  /// </summary>
  public class NightCLI_ApplySettings_LogLevelTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.ApplySettings.LogLevel";

    /// <inheritdoc />
    public override string Description => "Tests CLI ApplySettings method for LogLevel changes.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI ApplySettings correctly updated LogManager.MinLevel.";

    /// <inheritdoc />
    public override void Run()
    {
      LogLevel originalMinLevel = LogManager.MinLevel;
      try
      {
        // Arrange: CLI with specified log level
        var cli = new Night.CLI(new[] { "--log-level", "Warning" });
        Assert.Equal(LogLevel.Warning, cli.ParsedLogLevel); // Pre-condition

        // Act
        cli.ApplySettings();

        // Assert
        Assert.Equal(LogLevel.Warning, LogManager.MinLevel);

        // Arrange: CLI with no log level (should not change LogManager.MinLevel from its current state)
        LogManager.MinLevel = LogLevel.Fatal; // Set a known state
        var cliNoLevel = new Night.CLI(Array.Empty<string>());

        // Act
        cliNoLevel.ApplySettings();

        // Assert
        Assert.Equal(LogLevel.Fatal, LogManager.MinLevel); // Should remain Fatal
      }
      finally
      {
        LogManager.MinLevel = originalMinLevel; // Restore original
      }
    }
  }

  /// <summary>
  /// Tests for the CLI ApplySettings method focusing on the effects of debug mode.
  /// </summary>
  public class NightCLI_ApplySettings_DebugModeTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.ApplySettings.DebugMode";

    /// <inheritdoc />
    public override string Description => "Tests CLI ApplySettings method for debug mode effects.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI ApplySettings correctly handled debug mode.";

    /// <inheritdoc />
    public override void Run()
    {
      LogLevel originalMinLevel = LogManager.MinLevel;
      bool originalConsoleSinkState = LogManager.IsSystemConsoleSinkEnabled();
      try
      {
        // Arrange: CLI with debug mode
        var cli = new Night.CLI(new[] { "--debug" });
        Assert.True(cli.IsDebugMode); // Pre-condition
        LogManager.MinLevel = LogLevel.Information; // Set a non-debug level
        LogManager.EnableSystemConsoleSink(false); // Ensure console sink is off

        // Act
        cli.ApplySettings();

        // Assert
        Assert.Equal(LogLevel.Debug, LogManager.MinLevel);
        Assert.True(LogManager.IsSystemConsoleSinkEnabled(), "Console sink should be enabled in debug mode.");
      }
      finally
      {
        LogManager.MinLevel = originalMinLevel;
        LogManager.EnableSystemConsoleSink(originalConsoleSinkState); // Restore original console sink state
      }
    }
  }

  /// <summary>
  /// Tests that the CLI ApplySettings method does not crash when session logging is enabled
  /// and that it attempts to configure the session log.
  /// </summary>
  public class NightCLI_ApplySettings_SessionLogNoCrashTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.ApplySettings.SessionLogNoCrash";

    /// <inheritdoc />
    public override string Description => "Tests that CLI ApplySettings with session log enabled does not crash and attempts configuration.";

    /// <inheritdoc />
    public override string SuccessMessage => "CLI ApplySettings with session log ran without crashing.";

    // We can't easily check if the file sink was added without exposing LogManager.Sinks or FileSink details
    // So this test primarily ensures no crash and that DisableFileSink can be called.

    /// <inheritdoc />
    public override void Run()
    {
      LogLevel originalMinLevel = LogManager.MinLevel;
      bool originalConsoleSinkState = LogManager.IsSystemConsoleSinkEnabled();

      // Ensure no file sink from previous tests or other sources for a clean test environment.
      // It's important that DisableFileSink() correctly nullifies and removes the sink.
      LogManager.DisableFileSink();

      string expectedSessionPath = Path.Combine(AppContext.BaseDirectory ?? ".", "session");
      bool sessionDirExistedInitially = Directory.Exists(expectedSessionPath);

      try
      {
        // Arrange: CLI with session log enabled
        var cli = new Night.CLI(new[] { "--session-log" });
        Assert.True(cli.EnableSessionLog); // Pre-condition

        // Act: Apply settings. This will attempt to create a session log.
        Exception? ex = null;
        try
        {
          cli.ApplySettings(); // This should create "session" dir and a log file in it.
        }
        catch (Exception e)
        {
          ex = e;
        }

        // Assert: No crash during ApplySettings
        Assert.Null(ex);

        // Assert: Check if the "session" directory was created.
        Assert.True(Directory.Exists(expectedSessionPath), "Session directory should be created by ApplySettings.");
      }
      finally
      {
        LogManager.MinLevel = originalMinLevel;
        LogManager.EnableSystemConsoleSink(originalConsoleSinkState);
        LogManager.DisableFileSink(); // Ensure file sink is disabled after test

        // Cleanup: Remove the created session directory and its contents
        // only if it was created by this specific test run.
        if (Directory.Exists(expectedSessionPath) && !sessionDirExistedInitially)
        {
          try
          {
            Directory.Delete(expectedSessionPath, true);
          }
          catch
          { /* ignored, best effort */
          }
        }

        // If it existed before and we are not supposed to touch it, we leave it.
        // If it was created and delete failed, it might interfere with subsequent local runs,
        // but CI environments are usually clean.
      }
    }
  }

  /// <summary>
  /// Tests that the CLI ApplySettings method handles remaining or invalid arguments
  /// without crashing. Console output for warnings is not tested.
  /// </summary>
  public class NightCLI_ApplySettings_RemainingArgsWarningTest : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "CLI.ApplySettings.RemainingArgsWarning";

    /// <inheritdoc />
    public override string Description => "Tests that ApplySettings handles remaining/invalid args (no crash, console output not tested).";

    /// <inheritdoc />
    public override string SuccessMessage => "ApplySettings handled remaining/invalid args without crash.";

    /// <inheritdoc />
    public override void Run()
    {
      LogLevel originalMinLevel = LogManager.MinLevel; // Save original state
      try
      {
        // Arrange: CLI with remaining args including a bad --log-level
        var cli = new Night.CLI(new[] { "--log-level", "InvalidLevel", "otherArg" });
        Assert.Contains("InvalidLevel", cli.RemainingArgs);
        Assert.Contains("--log-level", cli.RemainingArgs);
        Assert.Contains("otherArg", cli.RemainingArgs);

        // Act & Assert: Ensure ApplySettings doesn't crash
        // We are not capturing console output here as per simplicity.
        Exception? ex = null;
        try
        {
          cli.ApplySettings();
        }
        catch (Exception e)
        {
          ex = e;
        }

        Assert.Null(ex);

        // Arrange: CLI with --log-level missing its value
        var cli2 = new Night.CLI(new[] { "--log-level" });
        Assert.Contains("--log-level", cli2.RemainingArgs);

        // Act & Assert
        ex = null;
        try
        {
          cli2.ApplySettings();
        }
        catch (Exception e)
        {
          ex = e;
        }

        Assert.Null(ex);
      }
      finally
      {
        LogManager.MinLevel = originalMinLevel; // Restore original state
      }
    }
  }
}
