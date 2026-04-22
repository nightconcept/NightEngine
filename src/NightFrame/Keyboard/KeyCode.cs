// <copyright file="KeyCode.cs" company="Night Circle">
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
  /// Represents keyboard scancodes. Values correspond to SDL_Scancode.
  /// </summary>
  /// <remarks>
  /// See SDL_scancode.h for the full list of SDL_Scancode values.
  /// This enum maps directly to SDL_Scancode values, representing the physical key on the keyboard.
  /// </remarks>
  public enum KeyCode
  {
    /// <summary>An unknown scancode.</summary>
    Unknown = 0,

    // Letters

    /// <summary>The 'A' key.</summary>
    A = 4,

    /// <summary>The 'B' key.</summary>
    B = 5,

    /// <summary>The 'C' key.</summary>
    C = 6,

    /// <summary>The 'D' key.</summary>
    D = 7,

    /// <summary>The 'E' key.</summary>
    E = 8,

    /// <summary>The 'F' key.</summary>
    F = 9,

    /// <summary>The 'G' key.</summary>
    G = 10,

    /// <summary>The 'H' key.</summary>
    H = 11,

    /// <summary>The 'I' key.</summary>
    I = 12,

    /// <summary>The 'J' key.</summary>
    J = 13,

    /// <summary>The 'K' key.</summary>
    K = 14,

    /// <summary>The 'L' key.</summary>
    L = 15,

    /// <summary>The 'M' key.</summary>
    M = 16,

    /// <summary>The 'N' key.</summary>
    N = 17,

    /// <summary>The 'O' key.</summary>
    O = 18,

    /// <summary>The 'P' key.</summary>
    P = 19,

    /// <summary>The 'Q' key.</summary>
    Q = 20,

    /// <summary>The 'R' key.</summary>
    R = 21,

    /// <summary>The 'S' key.</summary>
    S = 22,

    /// <summary>The 'T' key.</summary>
    T = 23,

    /// <summary>The 'U' key.</summary>
    U = 24,

    /// <summary>The 'V' key.</summary>
    V = 25,

    /// <summary>The 'W' key.</summary>
    W = 26,

    /// <summary>The 'X' key.</summary>
    X = 27,

    /// <summary>The 'Y' key.</summary>
    Y = 28,

    /// <summary>The 'Z' key.</summary>
    Z = 29,

    // Numbers (Top row)

    /// <summary>The '1' key.</summary>
    Alpha1 = 30,

    /// <summary>The '2' key.</summary>
    Alpha2 = 31,

    /// <summary>The '3' key.</summary>
    Alpha3 = 32,

    /// <summary>The '4' key.</summary>
    Alpha4 = 33,

    /// <summary>The '5' key.</summary>
    Alpha5 = 34,

    /// <summary>The '6' key.</summary>
    Alpha6 = 35,

    /// <summary>The '7' key.</summary>
    Alpha7 = 36,

    /// <summary>The '8' key.</summary>
    Alpha8 = 37,

    /// <summary>The '9' key.</summary>
    Alpha9 = 38,

    /// <summary>The '0' key.</summary>
    Alpha0 = 39,

    // Control keys

    /// <summary>The Return/Enter key.</summary>
    Return = 40,

    /// <summary>The Escape key.</summary>
    Escape = 41,

    /// <summary>The Backspace key.</summary>
    Backspace = 42,

    /// <summary>The Tab key.</summary>
    Tab = 43,

    /// <summary>The Space bar.</summary>
    Space = 44,

    /// <summary>The Minus '-' key.</summary>
    Minus = 45,

    /// <summary>The Equals '=' key.</summary>
    Equals = 46,

    /// <summary>The Left Bracket '[' key.</summary>
    Leftbracket = 47,

    /// <summary>The Right Bracket ']' key.</summary>
    Rightbracket = 48,

    /// <summary>The Backslash '' key.</summary>
    Backslash = 49,

    /// <summary>The Non-US Hash key (e.g., UK pound sign).</summary>
    NonUshash = 50,

    /// <summary>The Semicolon ';' key.</summary>
    Semicolon = 51,

    /// <summary>The Apostrophe ''' key.</summary>
    Apostrophe = 52,

    /// <summary>The Grave Accent (Backtick) '`' key.</summary>
    Grave = 53,

    /// <summary>The Comma ',' key.</summary>
    Comma = 54,

    /// <summary>The Period '.' key.</summary>
    Period = 55,

    /// <summary>The Slash '/' key.</summary>
    Slash = 56,

    /// <summary>The Caps Lock key.</summary>
    Capslock = 57,

    // Function keys

    /// <summary>The F1 key.</summary>
    F1 = 58,

    /// <summary>The F2 key.</summary>
    F2 = 59,

    /// <summary>The F3 key.</summary>
    F3 = 60,

    /// <summary>The F4 key.</summary>
    F4 = 61,

    /// <summary>The F5 key.</summary>
    F5 = 62,

    /// <summary>The F6 key.</summary>
    F6 = 63,

    /// <summary>The F7 key.</summary>
    F7 = 64,

    /// <summary>The F8 key.</summary>
    F8 = 65,

    /// <summary>The F9 key.</summary>
    F9 = 66,

    /// <summary>The F10 key.</summary>
    F10 = 67,

    /// <summary>The F11 key.</summary>
    F11 = 68,

    /// <summary>The F12 key.</summary>
    F12 = 69,

    /// <summary>The Print Screen key.</summary>
    Printscreen = 70,

    /// <summary>The Scroll Lock key.</summary>
    Scrolllock = 71,

    /// <summary>The Pause key.</summary>
    Pause = 72,

    /// <summary>The Insert key.</summary>
    Insert = 73,

    /// <summary>The Home key.</summary>
    Home = 74,

    /// <summary>The Page Up key.</summary>
    Pageup = 75,

    /// <summary>The Delete key.</summary>
    Delete = 76,

    /// <summary>The End key.</summary>
    End = 77,

    /// <summary>The Page Down key.</summary>
    Pagedown = 78,

    // Arrow keys

    /// <summary>The Right Arrow key.</summary>
    Right = 79,

    /// <summary>The Left Arrow key.</summary>
    Left = 80,

    /// <summary>The Down Arrow key.</summary>
    Down = 81,

    /// <summary>The Up Arrow key.</summary>
    Up = 82,

    /// <summary>The Num Lock or Clear key.</summary>
    NumlockClear = 83,

    // Keypad

    /// <summary>The Keypad Divide '/' key.</summary>
    KpDivide = 84,

    /// <summary>The Keypad Multiply '*' key.</summary>
    KpMultiply = 85,

    /// <summary>The Keypad Minus '-' key.</summary>
    KpMinus = 86,

    /// <summary>The Keypad Plus '+' key.</summary>
    KpPlus = 87,

    /// <summary>The Keypad Enter key.</summary>
    KpEnter = 88,

    /// <summary>The Keypad '1' key.</summary>
    Kp1 = 89,

    /// <summary>The Keypad '2' key.</summary>
    Kp2 = 90,

    /// <summary>The Keypad '3' key.</summary>
    Kp3 = 91,

    /// <summary>The Keypad '4' key.</summary>
    Kp4 = 92,

    /// <summary>The Keypad '5' key.</summary>
    Kp5 = 93,

    /// <summary>The Keypad '6' key.</summary>
    Kp6 = 94,

    /// <summary>The Keypad '7' key.</summary>
    Kp7 = 95,

    /// <summary>The Keypad '8' key.</summary>
    Kp8 = 96,

    /// <summary>The Keypad '9' key.</summary>
    Kp9 = 97,

    /// <summary>The Keypad '0' key.</summary>
    Kp0 = 98,

    /// <summary>The Keypad Period '.' key.</summary>
    KpPeriod = 99,

    /// <summary>The Non-US Backslash key.</summary>
    NonUsbackslash = 100,

    /// <summary>The Application key (context menu).</summary>
    Application = 101,

    /// <summary>The Power key.</summary>
    Power = 102,

    /// <summary>The Keypad Equals '=' key.</summary>
    KpEquals = 103,

    /// <summary>The F13 key.</summary>
    F13 = 104,

    /// <summary>The F14 key.</summary>
    F14 = 105,

    /// <summary>The F15 key.</summary>
    F15 = 106,

    /// <summary>The F16 key.</summary>
    F16 = 107,

    /// <summary>The F17 key.</summary>
    F17 = 108,

    /// <summary>The F18 key.</summary>
    F18 = 109,

    /// <summary>The F19 key.</summary>
    F19 = 110,

    /// <summary>The F20 key.</summary>
    F20 = 111,

    /// <summary>The F21 key.</summary>
    F21 = 112,

    /// <summary>The F22 key.</summary>
    F22 = 113,

    /// <summary>The F23 key.</summary>
    F23 = 114,

    /// <summary>The F24 key.</summary>
    F24 = 115,

    /// <summary>The Execute key.</summary>
    Execute = 116,

    /// <summary>The Help key.</summary>
    Help = 117,

    /// <summary>The Menu key.</summary>
    Menu = 118,

    /// <summary>The Select key.</summary>
    Select = 119,

    /// <summary>The Stop key.</summary>
    Stop = 120,

    /// <summary>The Again key.</summary>
    Again = 121,

    /// <summary>The Undo key.</summary>
    Undo = 122,

    /// <summary>The Cut key.</summary>
    Cut = 123,

    /// <summary>The Copy key.</summary>
    Copy = 124,

    /// <summary>The Paste key.</summary>
    Paste = 125,

    /// <summary>The Find key.</summary>
    Find = 126,

    /// <summary>The Mute key.</summary>
    Mute = 127,

    /// <summary>The Volume Up key.</summary>
    VolumeUp = 128,

    /// <summary>The Volume Down key.</summary>
    VolumeDown = 129,

    /// <summary>The Keypad Comma ',' key.</summary>
    KpComma = 133,

    // Modifier keys

    /// <summary>The Left Control key.</summary>
    LCtrl = 224,

    /// <summary>The Left Shift key.</summary>
    LShift = 225,

    /// <summary>The Left Alt key.</summary>
    LAlt = 226,

    /// <summary>The Left GUI key (Windows/Command/Meta key).</summary>
    LGUI = 227,

    /// <summary>The Right Control key.</summary>
    RCtrl = 228,

    /// <summary>The Right Shift key.</summary>
    RShift = 229,

    /// <summary>The Right Alt key.</summary>
    RAlt = 230,

    /// <summary>The Right GUI key (Windows/Command/Meta key).</summary>
    RGUI = 231,

    /// <summary>The Mode Switch key.</summary>
    Mode = 257,

    // Media keys (subset)

    /// <summary>The Media Play key.</summary>
    MediaPlay = 262,

    /// <summary>The Media Pause key.</summary>
    MediaPause = 263,

    /// <summary>The Media Record key.</summary>
    MediaRecord = 264,

    /// <summary>The Media Fast Forward key.</summary>
    MediaFastForward = 265,

    /// <summary>The Media Rewind key.</summary>
    MediaRewind = 266,

    /// <summary>The Media Next Track key.</summary>
    MediaNextTrack = 267,

    /// <summary>The Media Previous Track key.</summary>
    MediaPreviousTrack = 268,

    /// <summary>The Media Stop key.</summary>
    MediaStop = 269,

    /// <summary>The Media Eject key.</summary>
    MediaEject = 270,

    /// <summary>The Media Play/Pause key.</summary>
    MediaPlayPause = 271,

    /// <summary>The Media Select key.</summary>
    MediaSelect = 272,

    // Application control keys (subset)

    /// <summary>The Application Search key.</summary>
    AppSearch = 280,

    /// <summary>The Application Home key.</summary>
    AppHome = 281,

    /// <summary>The Application Back key.</summary>
    AppBack = 282,

    /// <summary>The Application Forward key.</summary>
    AppForward = 283,

    /// <summary>The Application Stop key.</summary>
    AppStop = 284,

    /// <summary>The Application Refresh key.</summary>
    AppRefresh = 285,

    /// <summary>The Application Bookmarks key.</summary>
    AppBookmarks = 286,
  }
}
