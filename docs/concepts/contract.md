---
sidebar:
  order: 2
---

# Compatibility contract

Both implementations pass fixtures under `tests/fixtures/` before release. `rng.json`
fixes RNG outputs and canonical snapshot text/hashes. `behavior.json` fixes frame
progression, held/pressed input and cue clearing.

The contract includes button bits, pointer kinds/phases, scene stack order,
frame hook order, targets, RNG, replay JSON and trace encoding. Language calling
conventions and graphics capabilities can differ. Supported targets are explicit.

Snapshots contain nested lists of numbers, strings, booleans and null. Numbers
use the existing 17-digit encoding; hashes use CRC-32. Dictionaries are excluded.
Replay widths and recorded targets remain backward compatible. Simulation changes
require an intentional contract and fixture update.

`events.json` fixes sparse recorded host events in both languages. Python/Pyxel
and Lua/LÖVE hosts feed events on playback and accept queued live events.
Nightrunner retains Python's seeded `random.Random` in its game adapter to preserve
its existing recordings. The shared engine default remains the portable RNG.
Python-specific host facilities include runtime rebinding, OS/browser settings
storage, device hints and pyxel-ne display pacing; see [API reference](../reference/nightrunner-api.md).
