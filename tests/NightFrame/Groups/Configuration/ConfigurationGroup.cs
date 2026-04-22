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
  /// Tests for the <see cref="Night.ConfigurationManager"/> functionality.
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
    /// Runs ModTestCases for the GameConfig feature within the Configuration module.
    /// This includes tests for getting and setting GameConfig properties,
    /// and indirectly tests AudioConfig, WindowConfig, and ModuleConfig functionality
    /// as they are part of GameConfig.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationGameConfig_ModTests()
    {
      this.Run_ModTestCase(new ConfigurationGameConfig_GetSet());
    }

    /// <summary>
    /// Runs ModTestCases for the ConfigurationManager feature within the Configuration module.
    /// This includes tests for IsLoaded property, and various scenarios of LoadConfig method
    /// such as already loaded, file not existing, empty file, invalid JSON, and deserialization to null.
    /// </summary>
    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_ConfigurationManager_ModTests()
    {
      this.Run_ModTestCase(new ConfigurationManager_IsLoadedTest());
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_AlreadyLoadedTest());
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_FileNotExistsTest());
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_EmptyFileTest());
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_InvalidJsonTest());
      this.Run_ModTestCase(new ConfigurationManager_LoadConfig_DeserializesToNullTest());
    }
  }
}
