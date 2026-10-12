"""The shell of a bot: a tap helper, a play loop that records the run, and a command line.

A game writes `choose(game) -> code` (the steering) and passes it to `run`. See the game's own autopilot.py.
"""

import argparse
from collections.abc import Callable
from pathlib import Path
from typing import Any

from .core import Game
from .replay import Recording, digits_for
from .target import Target


class Tapper:
    """Alternates press and release so every press is a fresh edge."""

    def __init__(self):
        self.tap = False

    def press(self, button: int) -> int:
        self.tap = not self.tap
        return button if self.tap else 0


def run(
    make_game: Callable[..., Game],
    choose: Callable[[Game], int | tuple[int, tuple]],
    seed: int,
    limit: int,
    done: Callable[[Game], bool] | None = None,
    target: Target | None = None,
) -> tuple[Recording, Game]:
    """Play until `done(game)`, or until `limit` frames. Returns the recording and the final game.

    `make_game(seed, target)` builds the game. `choose(game)` returns the buttons, or `(buttons, pointers)` to touch
    or click as well."""
    game = make_game(seed, target)
    recording = Recording(seed, width=digits_for(game.input_mask), target=target)
    while game.frame < limit:
        choice = choose(game)
        code, pointers = choice if isinstance(choice, tuple) else (choice, ())
        recording.add(code, pointers)
        game.step(code, pointers)
        if done and done(game):
            break
    return recording, game


def _flag(value: str) -> bool:
    if value.lower() in ("1", "true", "yes", "on"):
        return True
    if value.lower() in ("0", "false", "no", "off"):
        return False
    raise argparse.ArgumentTypeError(f"expected 1/0, true/false, or yes/no, not {value!r}")


def cli(
    play: Callable[..., tuple[Recording, Game]],
    summary: Callable[[Game], str],
    default_out: Path,
    doc: str,
    default_seed: int = 7,
    options: dict[str, Any] | None = None,
):
    """The command line every autopilot shares: --seed and --out. Plays, saves the recording, prints a summary.

    `options` adds game flags as {name: default}: {"battles": 1} adds `--battles` (an int, from the default's type),
    and its value is passed to `play` as a keyword argument. A bool option takes 1/0, true/false, or yes/no.
    """
    options = options or {}
    parser = argparse.ArgumentParser(description=doc, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--seed", type=int, default=default_seed)
    for name, default in options.items():
        kind = _flag if isinstance(default, bool) else type(default)
        parser.add_argument(f"--{name.replace('_', '-')}", dest=name, type=kind, default=default)
    parser.add_argument("--out", default=str(default_out))
    args = parser.parse_args()
    recording, game = play(args.seed, **{name: getattr(args, name) for name in options})
    recording.save(args.out)
    print(summary(game))
    print(f"saved {args.out}")
