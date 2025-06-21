// <copyright file="LogLevel.cs" company="Night Circle">
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

namespace Night
{
  /// <summary>
  /// Defines the severity levels for log messages.
  /// </summary>
  public enum LogLevel
  {
    /// <summary>
    /// Detailed information, typically of interest only when diagnosing problems.
    /// </summary>
    Trace,

    /// <summary>
    /// Information that is diagnostically helpful to people more than just developers.
    /// </summary>
    Debug,

    /// <summary>
    /// Generally useful information to log (service start/stop, configuration assumptions, etc).
    /// </summary>
    Information,

    /// <summary>
    /// Indicates a potential problem or an unexpected event.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates a failure in the current operation or task, not necessarily application-wide.
    /// </summary>
    Error,

    /// <summary>
    /// A critical error that might lead to application termination.
    /// </summary>
    Fatal,
  }
}
