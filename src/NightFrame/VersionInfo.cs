// <copyright file="VersionInfo.cs" company="Night Circle">
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
  /// Provides information about the Night library, such as its version.
  /// </summary>
  public static class VersionInfo
  {
    /// <summary>
    /// Gets the full semantic version string (e.g., "1.0.0", "1.2.3-beta.1").
    /// This value is updated by the GitHub release Action.
    /// </summary>
    public const string Version = "0.0.2";

    /// <summary>
    /// Gets the developer-assigned codename for the current version.
    /// This value is manually updated by the developer.
    /// </summary>
    public const string CodeName = "Initial Codename"; // TODO: Placeholder

    /// <summary>
    /// Gets the Semantic Version of the Night library.
    /// This version is set during the release process.
    /// </summary>
    /// <returns>The library's semantic version string.</returns>
    public static string GetVersion()
    {
      return Version;
    }
  }
}
