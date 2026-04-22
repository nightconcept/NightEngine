// <copyright file="FileSink.cs" company="Night Circle">
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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Night.Log.Sinks
{
  /// <summary>
  /// A log sink that writes log entries to a specified file.
  /// </summary>
  public class FileSink : ILogSink
  {
    private readonly string filePath;
    private readonly object lockObject = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSink"/> class.
    /// </summary>
    /// <param name="filePath">The full path to the log file.</param>
    /// <param name="minLevel">The minimum log level for this specific sink.</param>
    /// <exception cref="ArgumentNullException">Thrown if filePath is null or empty.</exception>
    public FileSink(string filePath, LogLevel minLevel)
    {
      if (string.IsNullOrEmpty(filePath))
      {
        throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");
      }

      this.filePath = filePath;
      this.MinLevel = minLevel;
      this.EnsureDirectoryExists();
    }

    /// <summary>
    /// Gets the minimum log level for this file sink.
    /// Only messages at this level or higher will be written by this sink,
    /// assuming they also pass the global LogManager.MinLevel.
    /// </summary>
    public LogLevel MinLevel { get; }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
      var messageBuilder = new StringBuilder();
      _ = messageBuilder.Append($"[{entry.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}Z] ");
      _ = messageBuilder.Append($"[{entry.Level,-11}] "); // Padded for alignment
      _ = messageBuilder.Append($"[{entry.CategoryName}] ");
      _ = messageBuilder.Append(entry.Message);

      if (entry.Exception != null)
      {
        _ = messageBuilder.AppendLine();
        _ = messageBuilder.Append($"    Exception: {entry.Exception.GetType().FullName}: {entry.Exception.Message}");
        if (!string.IsNullOrEmpty(entry.Exception.StackTrace))
        {
          _ = messageBuilder.AppendLine();
          _ = messageBuilder.Append($"    Stack Trace: {entry.Exception.StackTrace.Replace(Environment.NewLine, Environment.NewLine + "    ")}");
        }
      }

      lock (this.lockObject)
      {
        try
        {
          // Ensure directory exists right before writing, in case it was deleted externally.
          this.EnsureDirectoryExists();
          File.AppendAllText(this.filePath, messageBuilder.ToString() + Environment.NewLine, Encoding.UTF8);
        }
        catch (Exception ex)
        {
          // Log to diagnostics trace as a fallback.
          // This prevents a logging failure from crashing the application or affecting other sinks.
          Trace.WriteLine($"Night.Log.Sinks.FileSink: Failed to write to log file '{this.filePath}'. Error: {ex.Message}");
        }
      }
    }

    private void EnsureDirectoryExists()
    {
      try
      {
        string? directoryName = Path.GetDirectoryName(this.filePath);
        if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
        {
          _ = Directory.CreateDirectory(directoryName);
        }
      }
      catch (Exception ex)
      {
        // Log to diagnostics trace as a fallback, to avoid logger-ception or silent failure.
        Trace.WriteLine($"Night.Log.Sinks.FileSink: Failed to create directory for log file '{this.filePath}'. Error: {ex.Message}");
      }
    }
  }
}
