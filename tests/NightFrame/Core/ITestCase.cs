// <copyright file="ITestCase.cs" company="Night Circle">
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

namespace NightTest.Core
{
  /// <summary>
  /// Defines the contract for a runnable test case.
  /// Test cases must also implement Night.IGame to be executed by the framework.
  /// </summary>
  public interface ITestCase
  {
    /// <summary>
    /// Gets the unique name of the test case.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of the test case (e.g., Automated, Manual).
    /// </summary>
    TestType Type { get; }

    /// <summary>
    /// Gets a brief description of what the test case does.
    /// </summary>
    string Description { get; }
  }
}
