# Guidelines

## Code Style

The project adheres to the **Google C# Style Guide** with these project-specific rules.

- **Indentation:** 2 spaces, no tabs.
- **Column Limit:** 100 characters.
- **Braces:** Always used, even when optional. No line break before opening brace.
- **`using` directives:** System.* first, blank line, then other groups (Night, SDL3, etc.) each separated by a blank line. No inline comments on `using` lines.

## Naming Conventions

- Classes, methods, enumerations, public fields/properties, namespaces: `PascalCase`
- Local variables, parameters: `camelCase`
- Private/protected/internal fields and properties: `_camelCase`
- Interfaces: prefix with `I` (e.g., `IGame`)
- Filenames and directories: `PascalCase`
- Acronyms treated as words: `MyRpc` not `MyRPC`

## Code Organization

- **Modifier order:** `public protected internal private new abstract virtual override sealed static readonly extern unsafe volatile async`
- **`using` declarations:** Top of file, before namespace. Alphabetical, `System` always first.
- **Class member order:**
  1. Fields (static/const/readonly, then instance)
  2. Properties
  3. Constructors / Finalizers
  4. Methods (Public → Internal → Protected → Private)
  5. Nested types

Static members appear before instance members within each group.

## Key Principles

- **API Design:** Mirror Love2D API structure and ease of use while being idiomatic C#.
- **Clarity over premature optimization:** Maintainable code first.
- **XML docs:** Write XML summaries for all public API; use `inheritdoc` where appropriate.
- **Logging:** Use `Night.Log.LogManager.GetLogger("Category")` with levels `Info`, `Debug`, `Warn`, `Error`, `Fatal`.

## Mapping Native SDL3 to SDL3-CS

SDL3-CS bindings live in `lib/SDL3-CS/SDL3-CS/SDL/`.

**Naming rules:**
- Remove `SDL_` prefix, convert remainder to PascalCase: `SDL_CreateWindow` → `SDL.CreateWindow()`
- Enums/structs follow the same pattern: `SDL_WindowFlags` → `SDL.WindowFlags`
- Constants map to enum members: `SDL_INIT_VIDEO` → `SDL.InitFlags.Video`

**Finding a binding:**
1. Identify the SDL subsystem (Video, Events, Keyboard, etc.)
2. Navigate to the matching subdirectory (e.g., `SDL/Video/video/`)
3. Check `PInvoke.cs` for functions, individual `.cs` files for enums/structs
4. The `SDL` class is `partial`—members span many files but compose into `SDL3.SDL`

**Key C# idioms:**
- Many SDL functions returning `0`/error become `bool` (`true` = success); use `SDL.GetError()` on failure
- `const char*` inputs → `string`; output `char*` → `string` or `IntPtr` + `Marshal.PtrToStringUTF8()`
- Opaque handles (`SDL_Window*`, `SDL_Renderer*`) → `IntPtr`
- C enums with bitmasks → C# enums with `[Flags]`
- `SDL_Event*` in C → `out SDL.Event` or `ref SDL.Event` in C#

**SDL extension libraries (SDL3_image, SDL3_ttf):**
- If `SDL3.Image.LoadTexture()` returns an object but SDL property queries fail, examine the binding source directly in `lib/SDL3-CS/SDL3-CS/Image/PInvoke.cs`
- Prefer loading to `SDL_Surface` first (dimensions are accessible), then creating texture via `SDL.CreateTextureFromSurface()`; free the surface after
- Always call `SDL.GetError()` for error details—extension-specific error functions rarely exist
