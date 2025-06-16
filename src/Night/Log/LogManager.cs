// <copyright file="LogManager.cs" company="Night Circle">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Night.Log.Sinks;

namespace Night
{
  /// <summary>
  /// Manages logger instances and log sinks for the Night logging system.
  /// </summary>
  public static class LogManager
  {
    private static readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>();
    private static readonly List<ILogSink> Sinks = new List<ILogSink>();
    private static readonly object SinksLock = new object();

    private static SystemConsoleSink? systemConsoleSinkInstance;
    private static FileSink? fileSinkInstance;

    /// <summary>
    /// Gets or sets the global minimum log level.
    /// Only log entries with a level equal to or higher than this will be dispatched to sinks.
    /// Defaults to <see cref="LogLevel.Information"/>.
    /// </summary>
    public static LogLevel MinLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets a logger instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger"/> instance.</returns>
    public static ILogger GetLogger(string categoryName)
    {
      if (string.IsNullOrEmpty(categoryName))
      {
        throw new ArgumentNullException(nameof(categoryName));
      }

      return Loggers.GetOrAdd(categoryName, name => new Logger(name));
    }

    /// <summary>
    /// Adds a log sink to the logging system.
    /// </summary>
    /// <param name="sink">The log sink to add.</param>
    public static void AddSink(ILogSink sink)
    {
      if (sink == null)
      {
        throw new ArgumentNullException(nameof(sink));
      }

      lock (SinksLock)
      {
        if (!Sinks.Contains(sink))
        {
          Sinks.Add(sink);
        }
      }
    }

    /// <summary>
    /// Removes a log sink from the logging system.
    /// </summary>
    /// <param name="sink">The log sink to remove.</param>
    public static void RemoveSink(ILogSink sink)
    {
      if (sink == null)
      {
        throw new ArgumentNullException(nameof(sink));
      }

      lock (SinksLock)
      {
        _ = Sinks.Remove(sink);
      }
    }

    /// <summary>
    /// Removes all log sinks from the logging system.
    /// </summary>
    public static void ClearSinks()
    {
      lock (SinksLock)
      {
        Sinks.Clear();
      }
    }

    /// <summary>
    /// Enables or disables the system console log sink.
    /// </summary>
    /// <param name="enable">True to enable, false to disable.</param>
    public static void EnableSystemConsoleSink(bool enable)
    {
      lock (SinksLock)
      {
        if (enable)
        {
          if (systemConsoleSinkInstance == null)
          {
            systemConsoleSinkInstance = new SystemConsoleSink();
            AddSink(systemConsoleSinkInstance);
          }
        }
        else
        {
          if (systemConsoleSinkInstance != null)
          {
            RemoveSink(systemConsoleSinkInstance);
            systemConsoleSinkInstance = null;
          }
        }
      }
    }

    /// <summary>
    /// Checks if the system console sink is currently enabled.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public static bool IsSystemConsoleSinkEnabled()
    {
      lock (SinksLock)
      {
        return systemConsoleSinkInstance != null && Sinks.Contains(systemConsoleSinkInstance);
      }
    }

    /// <summary>
    /// Configures and enables the file log sink, replacing any existing file sink.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    public static void ConfigureFileSink(string filePath)
    {
      // TODO: The epic's manual tests (e.g., project/epics/logger-tasks.md:262)
      // imply FileSink should support its own MinLevel.
      // This overload exists to satisfy current Game.cs compilation.
      // FileSink.cs needs enhancement for sink-specific level filtering.
      // For now, we use a default or rely on global MinLevel.
      ConfigureFileSink(filePath, LogLevel.Trace); // Default to Trace for the sink itself
    }

    /// <summary>
    /// Configures and enables the file log sink, replacing any existing file sink.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <param name="minLevelForFile">The minimum log level for this specific file sink.</param>
    public static void ConfigureFileSink(string filePath, LogLevel minLevelForFile)
    {
      if (string.IsNullOrEmpty(filePath))
      {
        throw new ArgumentNullException(nameof(filePath));
      }

      lock (SinksLock)
      {
        if (fileSinkInstance != null)
        {
          RemoveSink(fileSinkInstance);

          // If FileSink implemented IDisposable, we would call _fileSinkInstance.Dispose(); here
          fileSinkInstance = null;
        }

        // FileSink constructor now accepts minLevelForFile.
        try
        {
          fileSinkInstance = new FileSink(filePath, minLevelForFile);
          AddSink(fileSinkInstance);
        }
        catch (Exception ex)
        {
          // Log configuration errors to System.Diagnostics.Trace
          Trace.WriteLine($"Night.Log.LogManager: Error configuring FileSink for path '{filePath}': {ex.Message}");
          Trace.WriteLine(ex.StackTrace);
          fileSinkInstance = null; // Ensure it's null if configuration failed
        }
      }
    }

    /// <summary>
    /// Disables the file log sink if it is currently active.
    /// </summary>
    public static void DisableFileSink()
    {
      lock (SinksLock)
      {
        if (fileSinkInstance != null)
        {
          RemoveSink(fileSinkInstance);

          // If FileSink implemented IDisposable, we would call _fileSinkInstance.Dispose(); here
          fileSinkInstance = null;
        }
      }
    }

    /// <summary>
    /// Dispatches a log entry to all active sinks if its level meets the <see cref="MinLevel"/>.
    /// This method is intended for internal use by <see cref="ILogger"/> implementations.
    /// </summary>
    /// <param name="entry">The log entry to dispatch.</param>
    internal static void Dispatch(LogEntry entry)
    {
      if (entry == null)
      {
        // Or throw ArgumentNullException, depending on desired strictness
        return;
      }

      if (entry.Level < MinLevel)
      {
        return;
      }

      List<ILogSink> currentSinks;
      lock (SinksLock)
      {
        // Create a copy to iterate over, avoiding issues if sinks are modified during dispatch
        currentSinks = new List<ILogSink>(Sinks);
      }

      foreach (var sinkInstance in currentSinks)
      {
        try
        {
          // Check for sink-specific log levels, e.g., for FileSink.
          if (sinkInstance is FileSink specificFileSink && entry.Level < specificFileSink.MinLevel)
          {
            continue; // Skip this sink if the entry's level is below the FileSink's specific minimum.
          }

          sinkInstance.Write(entry);
        }
        catch (Exception ex)
        {
          // Log sink errors to System.Diagnostics.Trace to avoid recursive logging or crashing.
          Trace.WriteLine($"Night.Log.LogManager: Error in sink '{sinkInstance.GetType().FullName}': {ex.Message}");
          Trace.WriteLine(ex.StackTrace);
        }
      }
    }
  }
}
