// <copyright file="XUnitLogSink.cs" company="Night Circle">
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

using System.Globalization;
using System.Text;

using Night;

using Xunit.Abstractions;

namespace NightTest.Core
{
  /// <summary>
  /// An ILogSink implementation that writes log entries to the xUnit test output.
  /// </summary>
  public class XUnitLogSink : ILogSink
  {
    private readonly ITestOutputHelper outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="XUnitLogSink"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper.</param>
    public XUnitLogSink(ITestOutputHelper outputHelper)
    {
      this.outputHelper = outputHelper;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
      var sb = new StringBuilder();
      _ = sb.Append(entry.TimestampUtc.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture));
      _ = sb.Append(" [").Append(entry.Level.ToString().ToUpperInvariant()).Append(']');
      _ = sb.Append(" [").Append(entry.CategoryName).Append(']');
      _ = sb.Append(' ').Append(entry.Message);

      if (entry.Exception != null)
      {
        _ = sb.Append('\n').Append(entry.Exception);
      }

      try
      {
        this.outputHelper.WriteLine(sb.ToString());
      }
      catch (InvalidOperationException)
      {
        // This can happen if a test completes while a background thread is still logging.
        // It's safe to ignore in this context.
      }
    }
  }
}
