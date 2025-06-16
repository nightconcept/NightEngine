// <copyright file="InGameConsoleSink.cs" company="Night Circle">
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
using System.Linq;

namespace Night.Log.Sinks
{
  /// <summary>
  /// A log sink that buffers log entries in memory for an in-game console display.
  /// </summary>
  public class InGameConsoleSink : ILogSink
  {
    private readonly ConcurrentQueue<LogEntry> logEntries;
    private readonly int? capacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="InGameConsoleSink"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of log entries to store. If null, the buffer is unbounded.</param>
    public InGameConsoleSink(int? capacity = null)
    {
      if (capacity.HasValue && capacity.Value <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be a positive integer if specified.");
      }

      this.logEntries = new ConcurrentQueue<LogEntry>();
      this.capacity = capacity;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
      this.logEntries.Enqueue(entry);

      if (this.capacity.HasValue)
      {
        while (this.logEntries.Count > this.capacity.Value && this.logEntries.TryDequeue(out _))
        {
          // Dequeue oldest entries if capacity is exceeded
        }
      }
    }

    /// <summary>
    /// Retrieves a snapshot of the current log entries buffered by this sink.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="LogEntry"/> objects.</returns>
    public IEnumerable<LogEntry> GetEntries()
    {
      // Returns a copy to prevent modification of the internal collection
      // and to ensure thread safety during enumeration.
      return this.logEntries.ToList();
    }
  }
}
