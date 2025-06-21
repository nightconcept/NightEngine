// <copyright file="LogEntry.cs" company="Night Circle">
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

namespace Night
{
  /// <summary>
  /// Represents a single log message.
  /// </summary>
  public record LogEntry
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class.
    /// Initializes a new instance of the <see cref="LogEntry"/> record.
    /// </summary>
    /// <param name="timestampUtc">The UTC timestamp of the log event.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="categoryName">The category name of the logger.</param>
    /// <param name="exception">The optional exception.</param>
    public LogEntry(
      DateTime timestampUtc,
      LogLevel level,
      string message,
      string categoryName,
      Exception? exception = null)
    {
      this.TimestampUtc = timestampUtc;
      this.Level = level;
      this.Message = message ?? string.Empty;
      this.CategoryName = categoryName ?? string.Empty;
      this.Exception = exception;
    }

    /// <summary>
    /// Gets the Coordinated Universal Time (UTC) when the log entry was created.
    /// </summary>
    public DateTime TimestampUtc { get; }

    /// <summary>
    /// Gets the severity level of the log entry.
    /// </summary>
    public LogLevel Level { get; }

    /// <summary>
    /// Gets the formatted log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the optional exception associated with the log entry.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the category name or source of the log entry.
    /// </summary>
    public string CategoryName { get; }
  }
}
