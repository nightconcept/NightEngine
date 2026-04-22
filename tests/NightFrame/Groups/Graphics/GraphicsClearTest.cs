// <copyright file="GraphicsClearTest.cs" company="Night Circle">
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

using Night;

using NightTest.Core;

namespace NightTest.Groups.Graphics
{
  /// <summary>
  /// Tests the Graphics.Clear() method with a specific color.
  /// Requires manual confirmation that the color is correct.
  /// </summary>
  public class GraphicsClearColorTest : ManualTestCase
  {
    private readonly Color skyBlue = new Color(135, 206, 235);

    /// <inheritdoc/>
    public override string Name => "Graphics.Clear";

    /// <inheritdoc/>
    public override string Description => "Tests clearing the screen to sky blue (135, 206, 235). User must confirm color.";

    /// <inheritdoc/>
    protected override void Load()
    {
      this.Details = "Test running, displaying sky blue color.";
    }

    /// <inheritdoc/>
    protected override void Update(double deltaTime)
    {
      this.RequestManualConfirmation("Is the screen cleared to a SKY BLUE color (like a clear daytime sky)?");
    }

    /// <inheritdoc/>
    protected override void Draw()
    {
      Night.Graphics.Clear(this.skyBlue);
    }
  }
}
