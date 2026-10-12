# APIs consolidated from Nightrunner

These Python APIs are part of the shared engine on main. Source lives in
`python/src/nightengine`; tests live in `python/tests`. The portable RNG is the
engine default. Nightrunner selects seeded `random.Random` in its game adapter
to preserve its existing recordings and simulation hashes.

## Buttons and bindings

`inputs.Button(name, keys=(), pad=())` describes an action. `declare(*buttons)`
assigns bits above the eight engine buttons. A game's `Game.buttons` joins its
`input_mask`; input edges and recording width include these bits.
`host.keys.bindings(buttons)` and `host.gamepad.xbox(..., buttons=...)` build the
default keyboard and controller tables.

`Binding` and `Bindings` store named primary controls and fixed aliases.
`Bindings.assign` trades occupied controls, respects locked entries and reserved
names, and returns a new binding set. JSON loads over the game's defaults.
Set `game.bindings` to the current set; the host rebuilds its key table when
that object changes. While `game.listen` is true, the host reports a
`press key:<NAME>` or `press pad:<NAME>` event. `App.device` provides keyboard,
pad or pointer hints for rendering; game rules do not read it.

## Host events and settings storage

`Input.events` contains recorded text for one simulation frame.
`Game.step(code, pointers=(), events=())` and `Recording.add` accept events.
`Recording.events_at(frame)` returns a sparse frame's events; older JSON without
`events` remains valid. Python and Lua use the same replay contract.
`Driver.event(*events)` sends events for one frame. `Driver.step(..., events=())`
sends them on its first frame only.

`App.post(event)` queues a live event. A game's `App.boot()` can return events
for live frame zero, such as saved settings. Replays use recorded events and
never call `boot`. Lua's host also accepts `App.post(event)` and replays events.

`Store`, `FileStore`, `MemoryStore` and `load_json` provide storage outside game
rules. FileStore atomically writes `<key>.json`; corrupt JSON returns the default
and is moved aside to `<key>.bad.json`. `Game.write(key, text)` queues a write.
The Pyxel host drains writes after each live step and clears requests during
replays without saving them.

`AppConfig.vendor` enables the game's store. `host.storage.open_store` works
before the window opens. It uses `data_dir(vendor, app)` on desktop platforms
and browser storage in a page. `AppConfig.display_scale` goes to `pyxel.init`.

## Display pacing and developer controls

`App.set_vsync(on)` and `AppConfig.vsync` use the pyxel-ne fork's `NE_PACING` and
`vsync` support. Stock Pyxel keeps its normal timer. `Pacer.frame(now)` returns
simulation steps and interpolation alpha for each display frame. `InputLatch`
retains buttons, pointer edges and events across frames with no step. Later
steps carry held contacts and omit repeated events. Each simulation step records
one input. `Renderer.alpha` and `Renderer.smooth` support optional interpolation.

Engine developer controls remain available as `app.dev`. Pause and manual steps
reset display pacing; holds count simulation frames. Paused screenshots render
the current state. `AudioManager.set_volume(music, sfx)` scales original channel
gains. Headless frame exports consume recorded events.

## Package and verify a game

Add the GitHub engine as a submodule tracking main and pin a verified commit.
Declare `nightengine` as a dependency and configure uv's source as
`nightengine = { path = "nightengine/python", editable = true }`.
Setuptools lists the game's own packages; the installed engine owns its package.

From a game root, run engine tests with
`python -m unittest discover -s nightengine/python/tests -t nightengine/python`.
Browser helpers live in `nightengine/python/src/nightengine/web/harness`.
The web builder strips a fork's local version label so Pyxel 2.9.9+ne.2 loads
stock 2.9.9 in the browser. Desktop builds retain the installed fork.

See [Python/Pyxel](../runtimes/python-pyxel.md) for the rest of the API and
[observability](observability.md) for the developer tools.
