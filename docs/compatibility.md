# Compatibility

Both implementations pass fixtures under `contract/` before release. `rng.json`
fixes RNG outputs and canonical snapshot text/hashes. `behavior.json` fixes frame
progression, held/pressed input and cue clearing. Unicycle's golden traces compare
full game state every frame on both implementations.

The contract includes button bits, pointer kinds/phases, scene stack order,
frame hook order, targets, RNG, replay JSON and trace encoding. Language calling
conventions and graphics capabilities can differ. Supported targets are explicit.

Snapshots contain nested lists of numbers, strings, booleans and null. Numbers
use the existing 17-digit encoding; hashes use CRC-32. Dictionaries are excluded.
Replay widths and recorded targets remain backward compatible. Simulation changes
require an intentional contract and fixture update.
