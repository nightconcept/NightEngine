// <copyright file="MemorySink.cs" company="Night Circle">
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

namespace Night
{
  /// <summary>
  /// An <see cref="ILogSink"/> implementation that stores recent log entries in memory.
  /// </summary>
  public class MemorySink : ILogSink
  {
    private readonly int capacity;
    private readonly ConcurrentQueue<LogEntry> entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemorySink"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of log entries to store. Defaults to 100.</param>
    public MemorySink(int capacity = 100)
    {
      if (capacity <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
      }

      this.capacity = capacity;
      this.entries = new ConcurrentQueue<LogEntry>();
    }

    /// <summary>
    /// Writes the specified log entry to the in-memory buffer.
    /// If the buffer exceeds capacity, the oldest entry is removed.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    public void Write(LogEntry entry)
    {
      if (entry == null)
      {
        return;
      }

      this.entries.Enqueue(entry);

      // Maintain capacity
      while (this.entries.Count > this.capacity && this.entries.TryDequeue(out _))
      {
        // Dequeued an old entry to maintain capacity
      }
    }

    /// <summary>
    /// Retrieves a snapshot of the currently buffered log entries.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="LogEntry"/> objects.</returns>
    public IEnumerable<LogEntry> GetEntries()
    {
      return this.entries.ToList(); // Return a copy
    }
  }
}
