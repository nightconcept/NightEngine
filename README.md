# nightengine

A small, game-agnostic engine on top of Pyxel 2.x. A game is a thin layer on the engine: its data, its rules,
its scenes, its art, and its draw functions. Games include this repo as a git submodule at `nightengine/`.
The same game runs on the desktop and in a phone or desktop browser: buttons, touch, and mouse are one recorded
input stream, and `nightengine.web` builds the browser version.

## Engine rules

- The engine knows no game. It imports only the standard library, `pyxel` (in `host/` only), and itself.
- Only `nightengine/host/` imports `pyxel` and the browser module `js`. The core never imports `nightengine.host`.
- The core is deterministic. It uses no `random` module state except `Game.rng`, and no wall-clock time.
- A difference between two games becomes a parameter. It never becomes an `if game == ...`.
- `nightengine/tests/test_rules.py` checks these rules with `ast`.

## Core interface

All names below import from `nightengine`, except where a module is named.

### Input (`inputs.py`)

- Buttons: `UP, DOWN, LEFT, RIGHT, A, B, C, MENU = 1, 2, 4, 8, 16, 32, 64, 128`. `ALL = 255`.
  One frame of input is one int made of these bits.
- `Input(held, pressed, repeat, pointers=())`:
  - `down(b)`: held now. `hit(b)`: pressed on this frame. `nav(b)`: pressed, or a repeat tick.
  - `axis() -> (dx, dy)`: the held direction. Diagonals are allowed and opposite keys cancel.
  - `direction() -> (dx, dy) | None`: one direction only, the newest press first. For tile movement.
- `InputTracker(mask=ALL, repeat=(), delay=14, rate=5).feed(code, pointers=()) -> Input`.
  A button in `repeat` ticks in `Input.repeat` after `delay` frames held, then every `rate` frames.

### Pointers (`pointer.py`)

Touch, pen, and mouse contacts, as game input. The host samples them once per frame and passes them to
`Game.step` with the buttons, so they are recorded and replayed like buttons.

- `Pointer(id, x, y, start_x, start_y, phase, kind=TOUCH)`: whole logical pixels. The mouse is id 0. `active` is
  False on the ending frame. `PointerKind`: `TOUCH`, `PEN`, or `MOUSE`, so a game can tell a finger from a click.
- `PointerPhase`: `PRESSED` (one frame), then `HELD` or `MOVED` each frame, then `RELEASED` (a normal end) or
  `CANCELLED` (the system took it). A cancelled contact is never a tap.
- Helpers on `inp.pointers`: `primary(ps)`, `pressed(ps)`, `released(ps)` (a tap or click ended here),
  `in_rect(ps, x, y, w, h)`. `encode(p)` and `decode(row)` are the recording format. A row has a kind letter at the
  end unless it is touch, so older files load as touch.
- Every action a pointer can do should also work with buttons, so gamepads and keyboards are never locked out.

### Targets (`target.py`)

A target is where the game runs: `desktop`, `web`, or `android`. It sets the screen size and the controls, so a
build for a phone and a build for a computer can differ without an `if` in the game.

- `Target(name="desktop", width=0, height=0, controls={KEYBOARD, MOUSE, TOUCH})`. A size of 0 means the game's own
  size (`sized(width, height)` fills it in). `KEYBOARD` includes gamepads, and `TOUCH` includes pens.
- `touch_only`: touch is the only control (a phone). `accepts(p)` and `takes(kind)`: whether the game sees a contact.
  A target without `MOUSE` drops mouse contacts, and one without `TOUCH` drops touch and pen. A touch-only target
  takes the mouse as a finger, so a desktop can play the phone layout.
- A game lists its targets in `pyproject.toml`. Every key is optional, and a target that is not listed takes every
  control:

  ```toml
  [tool.nightengine.targets.desktop]
  controls = ["keyboard", "mouse"]

  [tool.nightengine.targets.android]
  controls = ["touch"]
  width = 854     # optional: another size than [tool.nightengine.web]
  height = 480
  ```

- Each build writes its target into the app as the module `nightengine_build` (`build_module`). `current()` reads
  that module. Without it (a source run), `current()` reads `NIGHTENGINE_TARGET` and the nearest `pyproject.toml`,
  and falls back to `desktop`. `read_targets(root)` and `named(root, name)` read the tables.

### Scenes (`scene.py`)

- `Scene`: set `overlay = True` if the scenes below must still be drawn. Override `update(game, inp)`
  and `music(game) -> str | None`.
- `SceneStack`: `top`, `push(s)`, `pop(s)` (removes `s` if it is there), `replace(*scenes)` (in place),
  `find(cls)` (the topmost instance, or `None`), `visible()` (from the topmost opaque scene up through overlays),
  and list access: `stack[-1]`, `len`, iteration, `in`, `index(s)`.

### Game (`core.py`)

`Game` is the base class. A game subclasses it. Class attributes configure the input tracker:
`input_mask`, `repeat_buttons`, `repeat_delay`, `repeat_rate`.

- `Game(seed=0, target=None)` sets `seed`, `rng` (`random.Random(seed)`), `target` (`Target()` if None), `frame`,
  `tracker`, `scenes`, `fx`, and `cues`. Rules may read `target`, for example to start in a touch layout.
- `scene` is the top scene. `push(s)` and `pop(s)` use the stack. `music` asks the top scene for a track.
- `cue(name)` asks for a sound effect this frame.
- `screen_size` is the size the game wants now, or None (the default) for the target's. A game overrides it for a
  setting such as a wide screen. The host resizes the screen after the frame that changes it (`pyxel.resize`), so a
  replay changes size on the same frame.
- `before_scene(inp)` and `after_scene()` do nothing. A game overrides them.
- `quit_requested`: a scene sets it (for a Quit menu item). `App` saves the run and closes the window.
- `step(code, pointers=()) -> list[str]` runs one frame in this order: clear cues, drop the pointers the target does
  not accept, feed input, `fx.tick()`, `before_scene`,
  `scene.update(game, inp)`, `after_scene`, `frame += 1`. It returns the cues.
- `ScreenFx` (`fx.py`) holds `shake`, `fade`, `fade_in`, and `fade_frames`. `tick()` counts the shake down.
  It also counts `fade_in` down and sets `fade = fade_in / fade_frames`.

### Content (`content.py`, `registry.py`)

- `read_json(path)` drops the `_doc` key. `read_dir(dir, pattern="*.json")` returns `{stem: data}`.
- `records(data, cls, convert=None, key="id", rename=None)` builds one frozen record per table entry.
  `convert` maps a JSON field to a function for nested values. `rename` maps a JSON name to a field name.
  A bad field raises `ContentError` that names the record.
- `check_refs(table, field, valid, label) -> list[str]` lists each record whose `field` (a string, or a list of
  strings) names something not in `valid`.
- `Registry(kind)`: `register(name)` is a decorator. `reg[name]` raises a `KeyError` that names the kind and the
  known names. Also `in`, `names()`, and `missing(names)`.

### Other core modules

- `replay.py`: `Recording(seed, frames, pointers={}, width=2, target=None)` with `add(code, pointers)`, `at(frame)`, `save(path)`,
  `load(path)`, `to_json()`, and `from_json(text)`. The file is `{"seed", "inputs", "pointers", "width"}`: `width` hex
  digits per frame (2, or `digits_for(Game.input_mask)` for a game that uses bits above the 8 buttons; the key is
  absent when 2), and a sparse `{"<frame>": [[id, x, y, start_x, start_y, "P"], ...]}` for frames with contacts
  (absent when there were none). `"target"` holds the `Target` the run was played on (absent when None).
  `play(make_game, recording, frames=None) -> Game` replays it headless with `make_game(seed, target)`.
- `canvas.py`: `Canvas` (pixels in memory, no pyxel), `noise(x, y, seed)`, `mirror(rows)`.
- `palette.py`: `ENDESGA32` (33 entries: transparent black, then 32 colours), `KEY`, and the colour names
  `RUST` to `SKINSHADE`.
- `sound.py`: `ch(...)` builds a music channel. `note_errors(sfx, music) -> list[str]` and
  `slots_used(sfx, music) -> int` check sound tables without pyxel. Call them from a test.
- `systems.py`: `Systems(*systems).update(scene, game, inp)` runs each `system.update(...)` in order.
  A system that returns `True` ends the frame.
- `autopilot.py`: `Tapper.press(b)`, `run(make_game, choose, seed, limit, done, target=None)`, `cli(play, summary, default_out, doc, options={})`.
  `choose(game)` returns the buttons, or `(buttons, pointers)`.
  `options` adds game flags as `{name: default}` (for example `{"battles": 1}` adds `--battles`); `play` gets them as keywords.
- `testing.py`: `Driver(game)` has `step(code, frames, pointers=())`, `press(b)`, `tap(x, y)`, and
  `run_until(predicate, limit, code)`.

## Host interface

`nightengine.host` is the only part that imports `pyxel`. Import its modules by name:
`from nightengine.host.renderer import Renderer`. Tests may import them too. They never open a window.

- `keys.read_buttons(keys) -> int`: `keys` maps an engine button to the pyxel keys and gamepad buttons that press it.
- `gamepad`: controllers, Xbox first. SDL names every pad's buttons after the Xbox pad, so one layout serves Xbox,
  PlayStation, Switch Pro, and every pad in the bundled database.
  - `XBOX`: the shared layout. D-pad to `UP`/`DOWN`/`LEFT`/`RIGHT`, A to `A` (confirm, act), B to `B` (cancel, jump),
    X to `C`, Start to `MENU`.
  - `xbox(pad=1, layout=None, shift=0)` builds a key table for pad 1-4. `layout` adds to or replaces `XBOX` entries
    (`{B: ("B", "Y")}`); `shift` moves the buttons up for a second player's bits. `merge(*tables)` joins tables:
    `keys = merge(KEYBOARD, xbox(1))`.
  - `stick(pad, shift)` reads the left stick as d-pad buttons (`DEADZONE` 0.4), and `read_sticks(pairs)` reads
    every `(pad, shift)`. `axis(pad, name)` gives one axis in -1..1.
  - `use_mappings(path=DB)` points SDL's `SDL_GAMECONTROLLERCONFIG_FILE` hint at `host/gamepads/gamecontrollerdb.txt`
    (the zlib-licensed [SDL_GameControllerDB](https://github.com/mdqinc/SDL_GameControllerDB)), before `pyxel.init`.
    A file the player set in the environment wins. See `host/gamepads/README.md` to update it.
- `assets`:
  - `Region(bank, u, v, w, h)` is a frozen record of where a piece of art sits in an image bank.
  - `copy(canvas, bank, u, v) -> Region` writes one canvas into a bank.
  - `Shelf(bank, v=0, width=256).add(canvas) -> Region` packs canvases left to right, and wraps to a new row.
  - `pack(named, shelf)` and `pack_variants(named, shelf)` pack a dictionary of canvases (or lists of canvases).
    They return the same names with regions.
- `ui`:
  - `setup(font_path, color, shadow, scratch=(2, 200))` loads the font and sets the default text colours.
    `scratch` is the `(bank, v)` of a free 14-pixel row that `big_text` draws into.
  - `text(x, y, s, col=None, shadow=DEFAULT)`, `width(s)`, `center(y, s, col, shadow, cx=None)`, `wrap(s, max_w)`,
    `big_text(s, cx, y, scale, col, shadow)`. A `shadow` of `None` means no shadow.
  - Three sprite anchors, each with `flip=False` and extra `pyxel.blt` keywords:
    `blt` draws from the top-left, `blt_center` draws centred on `(x, y)`, `blt_feet` is centred on `x` with the
    bottom row on `y`.
  - `solid(region, x, y, col, flip=False, alpha=1.0, anchor="center" | "feet")` draws a one-colour silhouette.
  - `fade(alpha, col=0)` covers the screen. Games add their own widgets (panels, windows, gauges) in their own `ui`.
- `audio.AudioManager(sfx, music, once=(), sfx_channel=3, priority=True)`: `setup()` loads the tables into pyxel;
  `update(track, cues)` runs once per frame. The order of `sfx` is the priority order, highest first.
  - `priority=True`: play the most important cue of the frame, unless a more important effect still plays.
  - `priority=False`: play the first cue of the frame if it is known. Do not check what is playing.
  - A track in `once` does not loop.
  - `layers=N` (with `priority=False`) plays up to N different cues of one frame together, on `sfx_channel` and the
    channels below it. For a game without music.
- `renderer`:
  - `Renderer(draw, palette, bake, font, ui_colors, shake=no_shake, fade=False, scratch=(2, 200))`.
    `draw` is a dictionary from scene class to `draw(scene, game, t)`. The renderer keeps that dictionary,
    so a game can add entries after it builds the renderer. `@renderer.on(SceneClass)` adds one too.
  - `setup()` sets the palette, calls `bake()`, and sets up `ui`. `draw(game)` sets the camera from `shake`,
    draws `game.scenes.visible()`, resets the camera, and draws `ui.fade(game.fx.fade)` last if `fade=True`.
  - `draw_scene(scene, game, t)` draws one scene. A draw function can call it to draw the scenes below a transition.
  - Shake functions: `shake_xy` (sideways, plus up and down for a strong shake), `shake_x` (sideways only),
    `no_shake` (for a game that shakes inside its own draw functions).
- `app`: `AppConfig(title, width, height, keys, replays, fps=60, label_xy=(4, 4), integer_scale=True, mouse=False,
  sticks=((1, 0),), pad_mappings=gamepad.DB)`. `sticks` lists the `(pad, shift)` left sticks read as the d-pad.
  `App` loads the controller database before `pyxel.init` (`pad_mappings=None` skips it).
  (`integer_scale` applies on the desktop only: in a browser the game fills its canvas, so touches match the picture)
  and `App(config, make_game, renderer, audio, seed=None, replay=None, target=None)`. `App` picks the target
  (`choose_target`: a replay's own, else `target`, else `target.current()`), opens the window at the target's size,
  starts `platform`, and calls `make_game(seed, target)`. Each frame it reads the buttons and `platform.sample()`,
  records both, steps the game (a replay feeds the recorded ones), and resizes the screen if `game.screen_size`
  changed (`fit`). The recording keeps the target.
  `mouse=True` shows the system cursor when the target takes the mouse.
  `q` quits and saves the run to `replays/last.json`. The run is also saved when the window closes (`atexit`) and
  when the game sets `quit_requested`.
- `platform`: where the game runs.
  - `init(width, height, mouse=True)` (App calls it). With `mouse=False` the desktop mouse is not read.
  - `resize(width, height)`: a new logical size. In a page, `bridge.resize` maps later contacts to it and the page
    refits the canvas and its `data-width` and `data-height`. `available()` is True in a page built by `nightengine.web`.
  - `sample() -> tuple[Pointer, ...]`: in the browser, touch, pen, and mouse through `window.nightBridge`;
    on the desktop, the mouse as pointer 0 (`MouseTracker`). Each contact has its `kind`.
  - `screen`: `width`, `height`, `viewport_width`, `viewport_height`, `orientation`, and `safe_area`
    (`top`, `right`, `bottom`, `left` insets in logical pixels: keep text and touch targets out of them).
  - `platform()` (`android` or `desktop`), `is_touch_device()`.
  - `save(key, data)` and `load(key)`: browser storage. False and None on the desktop. A blocked read raises `OSError`.
- `diagnostic`: a pointer and safe-area check for a device. The web build serves it at `?app=debug`.
- `frames.frames_main(title, width, height, renderer, make_game, summary, modes=None, every=120)` is the
  command line of a game's `frames.py`. It renders a recording every `--every` frames, at the recording's target
  size and then at each size the game asks for, or `--atlas` for the image banks.
  Each entry in `modes` becomes a `--name` flag. A mode is `mode(out, scale)`, called after the window and art are set up.

## Web build (`web/`)

`python -m nightengine.web build --out dist` (or `serve`, with `--host 0.0.0.0` for a phone on the same
network) runs from a game's root and reads its `pyproject.toml`:

```toml
[tool.nightengine.web]
title = "Nightrunner"
width = 640
height = 360
entry = "game.py"                          # the default
packages = ["nightrunner", "nightengine"]  # their .py files, without tests/ and docs/
assets = ["assets"]                        # every file under these dirs
```

`--target` picks the target (default `web`; `android` shows the phone build in a browser). The build writes the
target into `game.pyxapp`, and the page takes the target's size.

It writes `game.pyxapp`, `debug.pyxapp` (the diagnostic), and the page: `index.html`, `launcher.mjs`,
`pointer.mjs` (the bridge), and `style.css`. The page loads the Pyxel runtime from a CDN, at the version installed in
the game's environment. `web/tests/pointer.test.mjs` tests the bridge: `node --test nightengine/web/tests/pointer.test.mjs`.

`--offline` copies the runtime into the build (`web/runtime.py`): `pyxel/` (pyxel.js, the Pyxel wheel, and its
images) and `pyodide/v<version>/` (the Pyodide core), about 18 MB. The page then needs no network, which an APK
needs. The files download once into `$NIGHTENGINE_CACHE` (default `~/.cache/nightengine`).

### Browser checks (`web/harness/`)

Playwright checks that run a real build in Chromium. A game adds `@playwright/test` to its `package.json` and a
`playwright.config.mjs`:

```js
import { harnessConfig } from './nightengine/web/harness/config.mjs';
export default harnessConfig({build: 'dist/web-offline', gameTests: 'tests/browser'});
```

`npx playwright test` serves the build, runs the engine's checks (`harness/tests/`: the game runs without errors, a
tap is recorded as a pointer, the pointer check page runs), then the game's. A game's specs import
`harness/harness.mjs`: `launch(page)` starts the game, `python(page, code)` runs Python in the page with `app` bound to
the running `App` (`nightengine.host.app.current`), `tap(page, x, y)` touches a logical pixel, `point` converts one to
page coordinates, and `touch(cdp, type, points)` sends raw multitouch. On macOS, `/Applications/Chromium.app` is used
if present. `NIGHTENGINE_BROWSER` picks another browser.

## Android (`android/`)

`python -m nightengine.android build --out dist/android` makes a debug-signed APK: a full-screen WebView that runs the
game's offline web build from the APK's assets. It needs JDK 17 or later and the Android SDK (`ANDROID_HOME`, with
`platforms;android-35`). Gradle is downloaded into the cache unless `gradle` is on the PATH. `project` writes the Gradle
project only, to open in Android Studio. The game adds a table next to `[tool.nightengine.web]`:

```toml
[tool.nightengine.android]
app_id = "dev.example.mygame"
orientation = "landscape"            # landscape | portrait | any
icon = "android/icon.png"            # optional, square PNG
keystore = "android/debug.keystore"  # optional: `python -m nightengine.android keystore android/debug.keystore`
```

- The app runs as the `android` target (`[tool.nightengine.targets.android]`).
- The page is served at `https://appassets.androidplatform.net/` (`MainActivity.java`), so wasm and ES modules load.
- The Back button is engine button B for one frame (`platform.buttons()`). `Game.quit_requested` closes the app
  (`platform.quit_page()`); in a browser tab it does nothing.
- Commit the keystore. Android installs an update only over an APK signed with the same key.
- A debug build allows WebView remote debugging (`chrome://inspect`, or `adb forward` to `webview_devtools_remote_<pid>`).
- The page's `?shell=app` layout hides the header and fills the screen. Pyxel never draws below 1x, so on a screen
  smaller than the game, `launcher.mjs` keeps the game's pixels and shrinks them with a CSS transform.

## Desktop (`desktop/`)

`uv run --with pyinstaller python -m nightengine.desktop build --out dist/desktop` runs `pyxel app2exe` (PyInstaller,
a program folder) for the system it runs on and zips it as `<slug>-<version>-<system>-<machine>.zip`. PyInstaller
cannot cross-build: the Windows program needs a Windows machine or runner. The program runs as the `desktop` target.

## Adding content

Every game grows the same way: new data, then a new named behaviour if the data needs one.

1. **Add a JSON record** to the game's `data/*.json` (an enemy, an item, a room, a stage).
   `records()` turns the table into typed records. `read_dir()` loads a folder of files.
2. **If the record needs new behaviour, register a handler.** Write one function and decorate it with the game's
   decorator, such as `@move("name")` or `@brain("name")`.
   Each is `Registry.register` on the game's own `Registry`. A record names its handler as a string.
3. **Add art by name.** Add a canvas to the game's art dictionary. `host.assets.pack` packs it into an image bank.
   A new sound is one entry in the game's `SFX` or `MUSIC` table.
4. **Let the content test check the references.** Use `check_refs` for each field that names another record.
   Use `Registry.missing` for each field that names a handler. Use `note_errors` for sound tables.
   A typo in JSON then fails with one clear message.

A new scene is a `Scene` subclass plus one draw function, registered with `@renderer.on(SceneClass)`.

Each game's `docs/repo/content.md` keeps its own record formats and points to this section.

## Use in a game

Add the engine as a submodule at `nightengine/`, so `import nightengine` works from the game's root:

```sh
git submodule add https://forge.solivan.dev/nightconcept/NightEngine.git nightengine
git submodule update --init --recursive   # after a fresh clone of the game
```

- Add `"nightengine"`, `"nightengine.host"`, and `"nightengine.web"` to the game's setuptools `packages`, and
  `pyxel>=2.6,<3` to its dependencies (pin an exact version to fix the web runtime too).
- A game's `Game` subclass takes `(seed=0, target=None)` and passes both to `super().__init__`. List the game's
  targets in `[tool.nightengine.targets.*]` (see Targets).
- Run the engine tests from the game root: `python -m unittest discover -s nightengine/tests -t .`
- Change the engine in this repo, then move each game to the new commit with `git -C nightengine pull` and a
  commit of the submodule pointer in the game.
