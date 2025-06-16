// <copyright file="CLI.cs" company="Night Circle">
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
using System.Globalization; // Added for DateTime parsing if needed for session log, though path generation is in Program.cs
using System.IO;
using System.Linq;

using Night;

namespace Night
{
  /// <summary>
  /// Handles command-line argument parsing for the Night Engine.
  /// </summary>
  public class CLI
  {
    private readonly List<string> remainingArgs = new();
    private bool isSilentMode = false;
    private LogLevel? parsedLogLevel = null;
    private bool isDebugMode = false;
    private bool enableSessionLog = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="CLI"/> class.
    /// Parses the provided command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public CLI(string[] args)
    {
      if (args == null)
      {
        return;
      }

      for (int i = 0; i < args.Length; i++)
      {
        string arg = args[i];

        if (string.Equals(arg, "-s", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "--silent", StringComparison.OrdinalIgnoreCase))
        {
          this.isSilentMode = true;
        }
        else if (string.Equals(arg, "--log-level", StringComparison.OrdinalIgnoreCase))
        {
          if (i + 1 < args.Length)
          {
            i++; // Consume the level value
            string levelString = args[i];
            if (Enum.TryParse(levelString, true, out LogLevel parsedLevel))
            {
              this.parsedLogLevel = parsedLevel;
            }
            else
            {
              // Invalid log level, add --log-level and its value back to remaining to be handled as an error or ignored by Program.cs
              this.remainingArgs.Add(arg);
              this.remainingArgs.Add(levelString);
            }
          }
          else
          {
            // Missing log level value, add --log-level back
            this.remainingArgs.Add(arg);
          }
        }
        else if (string.Equals(arg, "--debug", StringComparison.OrdinalIgnoreCase))
        {
          this.isDebugMode = true;
        }
        else if (string.Equals(arg, "--session-log", StringComparison.OrdinalIgnoreCase))
        {
          this.enableSessionLog = true;
        }
        else
        {
          this.remainingArgs.Add(arg);
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether silent mode was requested via command-line arguments.
    /// Silent mode typically suppresses startup console messages.
    /// </summary>
    public bool IsSilentMode => this.isSilentMode;

    /// <summary>
    /// Gets the log level parsed from the command-line arguments, if provided and valid.
    /// </summary>
    public LogLevel? ParsedLogLevel => this.parsedLogLevel;

    /// <summary>
    /// Gets a value indicating whether debug mode was requested via command-line arguments.
    /// </summary>
    public bool IsDebugMode => this.isDebugMode;

    /// <summary>
    /// Gets a value indicating whether session logging was requested via command-line arguments.
    /// </summary>
    public bool EnableSessionLog => this.enableSessionLog;

    /// <summary>
    /// Gets the list of arguments that were not processed as specific CLI flags by this parser.
    /// </summary>
    public IReadOnlyList<string> RemainingArgs => this.remainingArgs.AsReadOnly();

    /// <summary>
    /// Applies logging and other settings based on the parsed command-line arguments.
    /// </summary>
    public void ApplySettings()
    {
      // Apply settings based on parsed CLI arguments
      if (this.ParsedLogLevel.HasValue)
      {
        LogManager.MinLevel = this.ParsedLogLevel.Value;
        if (!this.IsSilentMode)
        {
          Console.WriteLine($"[Night.Engine.CLI] Log level set to: {this.ParsedLogLevel.Value}");
        }
      }

      if (this.IsDebugMode)
      {
        LogManager.MinLevel = LogLevel.Debug; // Ensure debug level if --debug is set
        LogManager.EnableSystemConsoleSink(true);
        if (!this.IsSilentMode)
        {
          Console.WriteLine("[Night.Engine.CLI] Debug mode enabled: Log level set to Debug, console sink enabled.");
        }
      }

      if (this.EnableSessionLog)
      {
        try
        {
          string baseDirectory = AppContext.BaseDirectory ?? ".";
          string sessionDirPath = Path.Combine(baseDirectory, "session");
          _ = Directory.CreateDirectory(sessionDirPath);

          string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
          string logFileName = $"session_log_{timestamp}.log";
          string logFilePath = Path.Combine(sessionDirPath, logFileName);

          LogManager.ConfigureFileSink(logFilePath, LogLevel.Trace); // FileSink itself will capture all from Trace, LogManager.MinLevel filters what's sent
          if (!this.IsSilentMode)
          {
            Console.WriteLine($"[Night.Engine.CLI] Session log enabled. Logging to: {logFilePath}");
          }
        }
        catch (Exception ex)
        {
          if (!this.IsSilentMode)
          {
            Console.WriteLine($"[Night.Engine.CLI] Error enabling session log: {ex.Message}");
          }
        }
      }

      // Handle any remaining arguments that were not processed by this parser
      if (this.RemainingArgs.Any())
      {
        if (!this.IsSilentMode)
        {
          Console.WriteLine($"[Night.Engine.CLI] Warning: Unprocessed or invalid arguments found: {string.Join(" ", this.RemainingArgs)}");
          if (this.RemainingArgs.Contains("--log-level", StringComparer.OrdinalIgnoreCase))
          {
            bool valueMissing = true;
            int logLevelIndex = this.RemainingArgs.ToList().FindIndex(x => x.Equals("--log-level", StringComparison.OrdinalIgnoreCase));
            if (logLevelIndex != -1 && logLevelIndex + 1 < this.RemainingArgs.Count)
            {
              if (!this.RemainingArgs[logLevelIndex + 1].StartsWith("--"))
              {
                Console.WriteLine($"[Night.Engine.CLI] Warning: The value '{this.RemainingArgs[logLevelIndex + 1]}' provided for --log-level is invalid. Using current default.");
                valueMissing = false;
              }
            }

            if (valueMissing)
            {
              Console.WriteLine("[Night.Engine.CLI] Warning: --log-level option requires a valid level argument (Trace, Debug, Information, Warning, Error, Fatal).");
            }
          }
        }
      }
    }
  }
}
