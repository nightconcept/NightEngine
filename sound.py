"""Sound data helpers that need no pyxel: build music channels, and check note strings and slot counts in tests.

Pyxel only finds a bad note when the window opens, so every game tests its SFX and MUSIC tables with these.
"""

import re
from collections.abc import Mapping, Sequence

MAX_SOUNDS = 64  # Pyxel sound slots. One per effect and one per music channel.
MAX_MUSICS = 8  # Pyxel music slots.

NOTE = re.compile(r"[a-g]#?\d|r")  # One note: name, optional sharp, octave digit. "r" is a rest.
VALID_NOTE = re.compile(r"[a-g]#?[0-4]|r")  # Pyxel plays octaves 0 to 4.


def ch(tone: str, volume: str, effect: str, speed: int, *bars: str) -> tuple:
    """One music channel: tone/volume/effect/speed, then bars of space-separated notes ("r" rests)."""
    return " ".join(bars), tone, volume, effect, speed


def _bad_notes(notes: str, compact: bool) -> list[str]:
    """Notes outside pyxel's range, plus any text that is not a note at all."""
    tokens = NOTE.findall(notes) if compact else notes.split()
    bad = [t for t in tokens if not VALID_NOTE.fullmatch(t)]
    rest = NOTE.sub("", notes) if compact else ""
    return bad + ([rest.strip()] if rest.strip() else [])


def note_errors(sfx: Mapping[str, Sequence], music: Mapping[str, Sequence[Sequence]]) -> list[str]:
    """List every bad note in an SFX table (name: (notes, ...)) and a MUSIC table (name: [(notes, ...), ...]).

    Effect notes are written without spaces ("c3e3g3"); music notes are separated by spaces.
    """
    errors = []
    for name, (notes, *_rest) in sfx.items():
        errors += [f"sfx {name}: bad note {n!r}" for n in _bad_notes(notes, compact=True)]
    for name, channels in music.items():
        for i, (notes, *_rest) in enumerate(channels):
            errors += [f"music {name} channel {i}: bad note {n!r}" for n in _bad_notes(notes, compact=False)]
    return errors


def slots_used(sfx: Mapping[str, Sequence], music: Mapping[str, Sequence[Sequence]]) -> int:
    """Pyxel sound slots the tables need: one per effect and one per music channel."""
    return len(sfx) + sum(len(channels) for channels in music.values())
