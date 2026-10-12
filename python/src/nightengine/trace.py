"""Per-frame state traces: the check that the Python and Lua versions of a game run the same.

A game writes `snapshot(game)`: its state as nested lists of numbers, strings, booleans, and None. `canon` turns
it into one line of text, the same in both languages (`lua/trace.lua`): numbers as `%.17g`, so 3 and
3.0 are the same, and -0.0 is 0. A trace is one CRC-32 of that text per frame, before each step and after the
last.

`python -m nightengine.trace` writes contract/rng.json, the rng and canon test vectors for the Lua side.
"""

import json
import zlib
from collections.abc import Callable, Sequence
from pathlib import Path
from typing import Any


def canon(value: Any) -> str:
    """The text of a snapshot value. Dicts are not allowed: give them as sorted lists of pairs."""
    if value is None:
        return "null"
    if value is True:
        return "true"
    if value is False:
        return "false"
    if isinstance(value, int | float):
        return format(float(value) or 0.0, ".17g")  # -0.0 is written as 0.
    if isinstance(value, str):
        return '"' + value.replace("\\", "\\\\").replace('"', '\\"') + '"'
    if isinstance(value, Sequence):
        return "[" + ",".join(canon(v) for v in value) + "]"
    raise TypeError(f"a snapshot holds lists, numbers, strings, booleans, and None, not {type(value).__name__}")


def digest(value: Any) -> str:
    return f"{zlib.crc32(canon(value).encode()):08x}"


def trace(game, steps, snapshot: Callable[[Any], Any]) -> list[str]:
    """Step `game` through `steps` ((code, pointers) pairs) and hash its snapshot before each step and after."""
    hashes = [digest(snapshot(game))]
    for code, pointers in steps:
        game.step(code, pointers)
        hashes.append(digest(snapshot(game)))
    return hashes


def recording_steps(recording):
    return ((code, recording.at(i)) for i, code in enumerate(recording.frames))


def save(path: Path | str, hashes: list[str]):
    Path(path).write_text("\n".join(hashes) + "\n")


def load(path: Path | str) -> list[str]:
    return Path(path).read_text().split()


def first_difference(a: list[str], b: list[str]) -> int | None:
    """The first frame where two traces differ, or None if they are the same."""
    for i, (x, y) in enumerate(zip(a, b, strict=False)):
        if x != y:
            return i
    return None if len(a) == len(b) else min(len(a), len(b))


def vectors() -> dict:
    """The rng test vectors in contract/rng.json."""
    from .rng import Rng

    seeds: list[int | str] = [0, 1, 7, 1997, 2**31 - 1, 2**32 + 5, -3, "1997:40", ""]
    out = []
    for seed in seeds:
        rng = Rng(seed)
        ints = [rng.randint(-5, 150) for _ in range(8)]
        bits = [rng.getrandbits(32) for _ in range(4)]
        out.append({"seed": seed, "randint(-5,150)": ints, "getrandbits(32)": bits})
    text = canon([["abc", 1.5, 3, -0.0, True, None, ['q"x\\']]])
    return {"rng": out, "canon": {"text": text, "hash": digest([["abc", 1.5, 3, -0.0, True, None, ['q"x\\']]])}}


if __name__ == "__main__":
    path = Path(__file__).resolve().parents[3] / "contract" / "rng.json"
    path.parent.mkdir(exist_ok=True)
    path.write_text(json.dumps(vectors(), indent=1) + "\n")
    print(path)
