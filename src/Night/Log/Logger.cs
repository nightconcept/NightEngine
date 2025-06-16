// <copyright file="Logger.cs" company="Night Circle">
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
  /// Default implementation of the <see cref="ILogger"/> interface.
  /// </summary>
  internal class Logger : ILogger
  {
    private readonly string categoryName;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="categoryName">The category name for this logger instance.</param>
    /// <remarks>
    /// This constructor is internal as loggers should be obtained via <see cref="LogManager.GetLogger(string)"/>.
    /// </remarks>
    internal Logger(string categoryName)
    {
      this.categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel level)
    {
      return level >= LogManager.MinLevel;
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
      if (!this.IsEnabled(level))
      {
        return;
      }

      var logEntry = new LogEntry(
          DateTime.UtcNow,
          level,
          message,
          this.categoryName,
          exception);

      LogManager.Dispatch(logEntry);
    }

    /// <inheritdoc/>
    public void Trace(string message)
    {
      this.Log(LogLevel.Trace, message);
    }

    /// <inheritdoc/>
    public void Debug(string message)
    {
      this.Log(LogLevel.Debug, message);
    }

    /// <inheritdoc/>
    public void Info(string message)
    {
      this.Log(LogLevel.Information, message);
    }

    /// <inheritdoc/>
    public void Warn(string message)
    {
      this.Log(LogLevel.Warning, message);
    }

    /// <inheritdoc/>
    public void Error(string message, Exception? exception = null)
    {
      this.Log(LogLevel.Error, message, exception);
    }

    /// <inheritdoc/>
    public void Fatal(string message, Exception? exception = null)
    {
      this.Log(LogLevel.Fatal, message, exception);
    }
  }
}
