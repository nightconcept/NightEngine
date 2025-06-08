// <copyright file="ConfigurationGroup.cs" company="Night Circle">
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
using System.Collections.Generic;

using Night;

using NightTest.Core;

using Xunit;
using Xunit.Abstractions;

namespace NightTest.Groups.Configuration
{
  /// <summary>
  /// Tests for the Night.Configuration functionality.
  /// </summary>
  [Collection("SequentialTests")]
  public class ConfigurationGroup : TestGroup
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationGroup"/> class.
    /// </summary>
    /// <param name="outputHelper">The xUnit test output helper for logging.</param>
    public ConfigurationGroup(ITestOutputHelper outputHelper)
      : base(outputHelper)
    {
    }

    /// <summary>
    /// Runs the GameConfig mod test. Also tests the AudioConfig, WindowConfig and ModuleConfig functionality.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationGameConfig_GetSet()
    {
      this.Run_ModTestCase(new ConfigurationGameConfig_GetSet());
    }

    /// <summary>
    /// Runs the ConfigurationManager IsLoaded property test.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_IsLoadedTest()
    {
      this.Run_ModTestCase(new ConfigurationManager_IsLoadedTest());
    }

    /// <summary>
    /// Runs the ConfigurationManager LoadConfig already loaded test.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_LoadConfig_AlreadyLoadedTest()
    {
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_AlreadyLoadedTest());
    }

    /// <summary>
    /// Runs the ConfigurationManager LoadConfig file not exists test.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_LoadConfig_FileNotExistsTest()
    {
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_FileNotExistsTest());
    }

    /// <summary>
    /// Runs the ConfigurationManager LoadConfig empty file test.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_LoadConfig_EmptyFileTest()
    {
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_EmptyFileTest());
    }

    /// <summary>
    /// Runs the ConfigurationManager LoadConfig invalid JSON test.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_LoadConfig_InvalidJsonTest()
    {
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_InvalidJsonTest());
    }

    /// <summary>
    /// Runs the ConfigurationManager LoadConfig deserializes to null test.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_LoadConfig_DeserializesToNullTest()
    {
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_DeserializesToNullTest());
    }
  }
}
