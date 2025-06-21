// <copyright file="SystemConsoleSink.cs" company="Night Circle">
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
using System.Globalization;

namespace Night
{
  /// <summary>
  /// An <see cref="ILogSink"/> implementation that writes log entries to the system console.
  /// </summary>
  public class SystemConsoleSink : ILogSink
  {
    /// <summary>
    /// Writes the specified log entry to the system console.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    public void Write(LogEntry entry)
    {
      if (entry == null)
      {
        return;
      }

      // Format: YYYY-MM-DDTHH:mm:ss.fffZ [LEVEL] [Category] Message
      // Exception details are appended on new lines if present.
      string timestamp = entry.TimestampUtc.ToString("o", CultureInfo.InvariantCulture); // ISO 8601
      string level = entry.Level.ToString().ToUpperInvariant();

      Console.WriteLine($"{timestamp} [{level}] [{entry.CategoryName}] {entry.Message}");

      if (entry.Exception != null)
      {
        Console.WriteLine(entry.Exception.ToString());
      }
    }
  }
}
