"""Record a run as seed plus per-frame input, and replay it exactly.

A recording is JSON: {"seed", "inputs", "pointers", "width"}. "inputs" holds `width` hex digits per frame (the
buttons, see inputs.py). "width" is 2 unless the game reads more than 8 bits (`Game.input_mask`), and is absent
when 2. "pointers" is optional and sparse: {"<frame>": [[id, x, y, start_x, start_y, phase letter], ...]} for the frames
that had touch or mouse contacts (see pointer.py). Files without it load as button-only runs. "events" is optional and
sparse: {"<frame>": ["text", ...]} for the frames with host events (see `Input.events`). "target" is optional:
{"name", "width", "height", "controls"} (see target.py). Files without it replay with the game's default target.
"""

import json
from collections.abc import Callable
from dataclasses import dataclass, field
from pathlib import Path
from typing import TYPE_CHECKING

from .pointer import Pointer, decode, encode
from .target import Target

if TYPE_CHECKING:
    from .core import Game


def digits_for(mask: int) -> int:
    """Hex digits that hold every bit of `mask`: 2 for the 8 engine buttons, more for a game with extra bits."""
    return max(2, (mask.bit_length() + 3) // 4)


@dataclass
class Recording:
    seed: int
    frames: list[int] = field(default_factory=list)
    pointers: dict[int, tuple[Pointer, ...]] = field(default_factory=dict)  # Frame -> contacts. Sparse.
    width: int = 2  # Hex digits per frame (see digits_for).
    target: Target | None = None  # Where the run was played, so a replay rebuilds the same layout.
    events: dict[int, tuple[str, ...]] = field(default_factory=dict)  # Frame -> host events. Sparse.

    def at(self, frame: int) -> tuple[Pointer, ...]:
        """The pointers of one frame (empty for most frames)."""
        return self.pointers.get(frame, ())

    def events_at(self, frame: int) -> tuple[str, ...]:
        """The host events of one frame (empty for most frames)."""
        return self.events.get(frame, ())

    def add(self, code: int, pointers: tuple[Pointer, ...] = (), events: tuple[str, ...] = ()):
        """Append one frame of input."""
        if pointers:
            self.pointers[len(self.frames)] = tuple(pointers)
        if events:
            self.events[len(self.frames)] = tuple(events)
        self.frames.append(code)

    def to_json(self) -> str:
        data = {"seed": self.seed, "inputs": "".join(f"{code:0{self.width}x}" for code in self.frames)}
        if self.width != 2:
            data["width"] = self.width
        if self.pointers:
            data["pointers"] = {str(f): [encode(p) for p in ps] for f, ps in sorted(self.pointers.items())}
        if self.events:
            data["events"] = {str(f): list(es) for f, es in sorted(self.events.items())}
        if self.target:
            data["target"] = self.target.to_dict()
        return json.dumps(data, separators=(",", ":"))

    @classmethod
    def from_json(cls, text: str) -> "Recording":
        data = json.loads(text)
        digits, width = data["inputs"], data.get("width", 2)
        frames = [int(digits[i : i + width], 16) for i in range(0, len(digits), width)]
        pointers = {int(f): tuple(decode(row) for row in rows) for f, rows in data.get("pointers", {}).items()}
        target = Target.from_dict(data["target"]) if "target" in data else None
        events = {int(f): tuple(es) for f, es in data.get("events", {}).items()}
        return cls(data["seed"], frames, pointers, width, target, events)

    def save(self, path: Path | str):
        path = Path(path)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(self.to_json() + "\n")

    @classmethod
    def load(cls, path: Path | str) -> "Recording":
        return cls.from_json(Path(path).read_text())


def play(make_game: Callable[..., "Game"], recording: Recording, frames: int | None = None) -> "Game":
    """Run a recording headless and return the final state. `make_game(seed, target)` builds the game."""
    game = make_game(recording.seed, recording.target)
    for i, code in enumerate(recording.frames[:frames]):
        game.step(code, recording.at(i), recording.events_at(i))
    return game
