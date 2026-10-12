"""Render frames of a recording, or a sheet of all baked art, to PNG files without a game loop.

A game's frames.py calls `frames_main(...)`. Its command line:

python -m <game>.frames replays/auto-7.json frames/ --every 120
python -m <game>.frames --atlas frames/

A game can add modes, such as `modes={"rooms": rooms}`. Each becomes a `--name` flag. A mode is a function
`mode(out: Path, scale: int)` that runs after the window and the art are set up.
"""

import argparse
from collections.abc import Callable
from pathlib import Path

import pyxel

from ..core import Game
from ..replay import Recording
from ..target import Target
from .app import screen_size
from .renderer import Renderer


def atlas(title: str, renderer: Renderer, out: Path):
    """Save each image bank, scaled up, so the generated art can be reviewed."""
    pyxel.init(256, 256, title=f"{title} atlas")
    renderer.setup()
    for bank in range(3):
        pyxel.cls(0)
        pyxel.blt(0, 0, bank, 0, 0, 256, 256)
        pyxel.screen.save(str(out / f"atlas-bank{bank}"), 3)
        print(out / f"atlas-bank{bank}.png")


def frames_main(
    title: str,
    width: int,
    height: int,
    renderer: Renderer,
    make_game: Callable[..., Game],
    summary: Callable[[Game], str],
    modes: dict[str, Callable[[Path, int], None]] | None = None,
    every: int = 120,
):
    modes = modes or {}
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("recording", nargs="?")
    parser.add_argument("out")
    parser.add_argument("--every", type=int, default=every, help="frames between snapshots")
    parser.add_argument("--start", type=int, default=0)
    parser.add_argument("--scale", type=int, default=2)
    parser.add_argument("--atlas", action="store_true", help="save the image banks instead")
    for name, mode in modes.items():
        parser.add_argument(f"--{name}", action="store_true", help=(mode.__doc__ or name).strip().split("\n")[0])
    args = parser.parse_args()
    out = Path(args.out).resolve()
    out.mkdir(parents=True, exist_ok=True)
    if args.atlas:
        atlas(title, renderer, out)
        return
    for name, mode in modes.items():
        if getattr(args, name):
            pyxel.init(width, height, title=f"{title} {name}")
            renderer.setup()
            mode(out, args.scale)
            return
    recording = Recording.load(args.recording)
    target = (recording.target or Target()).sized(width, height)
    pyxel.init(target.width, target.height, title=f"{title} frames")
    renderer.setup()
    game = make_game(recording.seed, recording.target and target)

    def draw():
        size = screen_size(game, target)
        if size != (pyxel.width, pyxel.height):
            pyxel.resize(*size)
        renderer.draw(game)

    for i, code in enumerate(recording.frames):
        if game.frame >= args.start and (game.frame - args.start) % args.every == 0:
            draw()
            pyxel.screen.save(str(out / f"frame{game.frame:05}"), args.scale)
        game.step(code, recording.at(i))
    draw()
    pyxel.screen.save(str(out / "final"), args.scale)
    print(summary(game))
