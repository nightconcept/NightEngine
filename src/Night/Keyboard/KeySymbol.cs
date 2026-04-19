// <copyright file="KeySymbol.cs" company="Night Circle">
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

using SDL3;

namespace Night
{
  /// <summary>
  /// Represents logical key symbols. Values correspond to SDL_Keycode.
  /// </summary>
  /// <remarks>
  /// This enum maps to SDL_Keycode values, representing the symbol produced by a key press
  /// under the current keyboard layout.
  /// </remarks>
  public enum KeySymbol : uint // Explicitly set underlying type to uint
  {
    /// <summary>An unknown key symbol.</summary>
    Unknown = SDL.Keycode.Unknown,

    // Letters (match SDL.Keycode values, which are ASCII for letters)

    /// <summary>The 'A' symbol.</summary>
    A = SDL.Keycode.A,

    /// <summary>The 'B' symbol.</summary>
    B = SDL.Keycode.B,

    /// <summary>The 'C' symbol.</summary>
    C = SDL.Keycode.C,

    /// <summary>The 'D' symbol.</summary>
    D = SDL.Keycode.D,

    /// <summary>The 'E' symbol.</summary>
    E = SDL.Keycode.E,

    /// <summary>The 'F' symbol.</summary>
    F = SDL.Keycode.F,

    /// <summary>The 'G' symbol.</summary>
    G = SDL.Keycode.G,

    /// <summary>The 'H' symbol.</summary>
    H = SDL.Keycode.H,

    /// <summary>The 'I' symbol.</summary>
    I = SDL.Keycode.I,

    /// <summary>The 'J' symbol.</summary>
    J = SDL.Keycode.J,

    /// <summary>The 'K' symbol.</summary>
    K = SDL.Keycode.K,

    /// <summary>The 'L' symbol.</summary>
    L = SDL.Keycode.L,

    /// <summary>The 'M' symbol.</summary>
    M = SDL.Keycode.M,

    /// <summary>The 'N' symbol.</summary>
    N = SDL.Keycode.N,

    /// <summary>The 'O' symbol.</summary>
    O = SDL.Keycode.O,

    /// <summary>The 'P' symbol.</summary>
    P = SDL.Keycode.P,

    /// <summary>The 'Q' symbol.</summary>
    Q = SDL.Keycode.Q,

    /// <summary>The 'R' symbol.</summary>
    R = SDL.Keycode.R,

    /// <summary>The 'S' symbol.</summary>
    S = SDL.Keycode.S,

    /// <summary>The 'T' symbol.</summary>
    T = SDL.Keycode.T,

    /// <summary>The 'U' symbol.</summary>
    U = SDL.Keycode.U,

    /// <summary>The 'V' symbol.</summary>
    V = SDL.Keycode.V,

    /// <summary>The 'W' symbol.</summary>
    W = SDL.Keycode.W,

    /// <summary>The 'X' symbol.</summary>
    X = SDL.Keycode.X,

    /// <summary>The 'Y' symbol.</summary>
    Y = SDL.Keycode.Y,

    /// <summary>The 'Z' symbol.</summary>
    Z = SDL.Keycode.Z,

    // Numbers (Top row - match SDL.Keycode values, which are ASCII for numbers)

    /// <summary>The '0' symbol.</summary>
    Alpha0 = SDL.Keycode.Alpha0,

    /// <summary>The '1' symbol.</summary>
    Alpha1 = SDL.Keycode.Alpha1,

    /// <summary>The '2' symbol.</summary>
    Alpha2 = SDL.Keycode.Alpha2,

    /// <summary>The '3' symbol.</summary>
    Alpha3 = SDL.Keycode.Alpha3,

    /// <summary>The '4' symbol.</summary>
    Alpha4 = SDL.Keycode.Alpha4,

    /// <summary>The '5' symbol.</summary>
    Alpha5 = SDL.Keycode.Alpha5,

    /// <summary>The '6' symbol.</summary>
    Alpha6 = SDL.Keycode.Alpha6,

    /// <summary>The '7' symbol.</summary>
    Alpha7 = SDL.Keycode.Alpha7,

    /// <summary>The '8' symbol.</summary>
    Alpha8 = SDL.Keycode.Alpha8,

    /// <summary>The '9' symbol.</summary>
    Alpha9 = SDL.Keycode.Alpha9,

    // Common control keys

    /// <summary>The Return/Enter symbol.</summary>
    Return = SDL.Keycode.Return,

    /// <summary>The Escape symbol.</summary>
    Escape = SDL.Keycode.Escape,

    /// <summary>The Backspace symbol.</summary>
    Backspace = SDL.Keycode.Backspace,

    /// <summary>The Tab symbol.</summary>
    Tab = SDL.Keycode.Tab,

    /// <summary>The Space symbol.</summary>
    Space = SDL.Keycode.Space,

    // Punctuation (example, more can be added)

    /// <summary>The Minus '-' symbol.</summary>
    Minus = SDL.Keycode.Minus,

    /// <summary>The Equals '=' symbol.</summary>
    Equals = SDL.Keycode.Equals,

    /// <summary>The Left Bracket '[' symbol.</summary>
    Leftbracket = SDL.Keycode.LeftBracket,

    /// <summary>The Right Bracket ']' symbol.</summary>
    Rightbracket = SDL.Keycode.RightBracket,

    /// <summary>The Backslash '' symbol.</summary>
    Backslash = SDL.Keycode.Backslash,

    /// <summary>The Semicolon ';' symbol.</summary>
    Semicolon = SDL.Keycode.Semicolon,

    /// <summary>The Apostrophe ''' symbol.</summary>
    Apostrophe = SDL.Keycode.Apostrophe,

    /// <summary>The Grave Accent (Backtick) '`' symbol.</summary>
    Grave = SDL.Keycode.Grave,

    /// <summary>The Comma ',' symbol.</summary>
    Comma = SDL.Keycode.Comma,

    /// <summary>The Period '.' symbol.</summary>
    Period = SDL.Keycode.Period,

    /// <summary>The Slash '/' symbol.</summary>
    Slash = SDL.Keycode.Slash,

    // Function keys

    /// <summary>The F1 symbol.</summary>
    F1 = SDL.Keycode.F1,

    /// <summary>The F2 symbol.</summary>
    F2 = SDL.Keycode.F2,

    /// <summary>The F3 symbol.</summary>
    F3 = SDL.Keycode.F3,

    /// <summary>The F4 symbol.</summary>
    F4 = SDL.Keycode.F4,

    /// <summary>The F5 symbol.</summary>
    F5 = SDL.Keycode.F5,

    /// <summary>The F6 symbol.</summary>
    F6 = SDL.Keycode.F6,

    /// <summary>The F7 symbol.</summary>
    F7 = SDL.Keycode.F7,

    /// <summary>The F8 symbol.</summary>
    F8 = SDL.Keycode.F8,

    /// <summary>The F9 symbol.</summary>
    F9 = SDL.Keycode.F9,

    /// <summary>The F10 symbol.</summary>
    F10 = SDL.Keycode.F10,

    /// <summary>The F11 symbol.</summary>
    F11 = SDL.Keycode.F11,

    /// <summary>The F12 symbol.</summary>
    F12 = SDL.Keycode.F12,

    // Arrow keys

    /// <summary>The Right Arrow symbol.</summary>
    Right = SDL.Keycode.Right,

    /// <summary>The Left Arrow symbol.</summary>
    Left = SDL.Keycode.Left,

    /// <summary>The Down Arrow symbol.</summary>
    Down = SDL.Keycode.Down,

    /// <summary>The Up Arrow symbol.</summary>
    Up = SDL.Keycode.Up,

    // Modifiers

    /// <summary>The Left Control symbol.</summary>
    LCtrl = SDL.Keycode.LCtrl,

    /// <summary>The Left Shift symbol.</summary>
    LShift = SDL.Keycode.LShift,

    /// <summary>The Left Alt symbol.</summary>
    LAlt = SDL.Keycode.LAlt,

    /// <summary>The Left GUI symbol (Windows/Command/Meta key).</summary>
    LGUI = SDL.Keycode.LGUI,

    /// <summary>The Right Control symbol.</summary>
    RCtrl = SDL.Keycode.RCtrl,

    /// <summary>The Right Shift symbol.</summary>
    RShift = SDL.Keycode.RShift,

    /// <summary>The Right Alt symbol.</summary>
    RAlt = SDL.Keycode.RAlt,

    /// <summary>The Right GUI symbol (Windows/Command/Meta key).</summary>
    RGUI = SDL.Keycode.RGUI,
  }
}
