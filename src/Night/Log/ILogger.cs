// <copyright file="ILogger.cs" company="Night Circle">
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
  /// Interface for logging messages.
  /// </summary>
  public interface ILogger
  {
    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    /// <param name="level">The severity level of the message.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">Optional. The exception associated with the message.</param>
    void Log(LogLevel level, string message, Exception? exception = null);

    /// <summary>
    /// Checks if the specified log level is enabled for this logger.
    /// </summary>
    /// <param name="level">The log level to check.</param>
    /// <returns><c>true</c> if the log level is enabled; otherwise, <c>false</c>.</returns>
    bool IsEnabled(LogLevel level);

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Trace(string message);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Debug(string message);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Info(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Warn(string message);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">Optional. The exception associated with the message.</param>
    void Error(string message, Exception? exception = null);

    /// <summary>
    /// Logs a fatal error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">Optional. The exception associated with the message.</param>
    void Fatal(string message, Exception? exception = null);
  }
}
