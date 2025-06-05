// <copyright file="SDLGroup.cs" company="Night Circle">
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

using NightTest.Core;

using Xunit;
using Xunit.Abstractions;

namespace NightTest.Groups.SDL
{
  /// <summary>
  /// Test group for SDL related functionality.
  /// </summary>
  public class SDLGroup : TestGroup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SDLGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit output helper.</param>
    public SDLGroup(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    /// <summary>
    /// Runs the <see cref="NightSDL_GetVersionTest"/>.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_GetVersionTest()
    {
      this.Run_TestCase(new NightSDL_GetVersionTest());
    }

    /// <summary>
    /// Runs the <see cref="NightSDL_GetErrorTest"/>.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_GetErrorTest()
    {
      this.Run_TestCase(new NightSDL_GetErrorTest());
    }
  }
}
