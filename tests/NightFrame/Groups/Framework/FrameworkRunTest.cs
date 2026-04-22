// <copyright file="FrameworkRunTest.cs" company="Night Circle">
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
using System.Linq;

using Night;

using NightTest.Core;

using Xunit;

namespace NightTest.Groups.Framework
{
  /// <summary>
  /// Tests for the Framework.Run method with null IGame handling.
  /// </summary>
  public class FrameworkRun_NullIGame : ModTestCase
  {
    /// <inheritdoc />
    public override string Name => "Night.Framework.Run";

    /// <inheritdoc />
    public override string Description => "Tests null IGame handling in Framework.Run().";

    /// <inheritdoc />
    public override string SuccessMessage => "Null game handling in Framework.Run() passed successfully.";

    /// <inheritdoc />
    public override void Run()
    {
      try
      {
        // Act: Call Framework.Run with null IGame
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Night.Framework.Run(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
      }
      catch (ArgumentNullException ex)
      {
        // Assert: Expect ArgumentNullException for null IGame
        Assert.Equal("game", ex.ParamName);
      }
      catch (Exception ex)
      {
        // Fail if any other exception is thrown
        Assert.Fail($"Unexpected exception type: {ex.GetType().Name} - {ex.Message}");
      }
    }
  }
}
