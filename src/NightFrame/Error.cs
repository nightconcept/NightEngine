// <copyright file="Error.cs" company="Night Circle">
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
using System.IO;

namespace Night
{
  /// <summary>
  /// Defines the delegate for handling unhandled exceptions from game code.
  /// The error handler function will be called when an unhandled exception occurs
  /// within the game's Load, Update, Draw, or input callback methods.
  /// </summary>
  /// <param name="e">The exception that occurred.</param>
  public delegate void ErrorHandlerDelegate(Exception e);

  /// <summary>
  /// Manages the user-defined error handler.
  /// </summary>
  public static class Error
  {
    /// <summary>
    /// Gets the currently set custom error handler. Returns null if no custom handler is set.
    /// This is used internally by the framework to invoke the handler.
    /// </summary>
    internal static ErrorHandlerDelegate? CustomErrorHandler { get; private set; }

    /// <summary>
    /// Sets a custom function to be called when an unhandled error occurs in game code.
    /// </summary>
    /// <param name="handler">The delegate to handle errors.</param>
    public static void SetHandler(ErrorHandlerDelegate handler)
    {
      CustomErrorHandler = handler;
    }

    /// <summary>
    /// Gets the currently set custom error handler.
    /// This is used internally by the framework.
    /// </summary>
    /// <returns>The custom error handler delegate, or null if none is set.</returns>
    internal static ErrorHandlerDelegate? GetHandler()
    {
      return CustomErrorHandler;
    }
  }
}
