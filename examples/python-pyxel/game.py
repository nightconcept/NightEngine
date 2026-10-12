"""Run with the engine Python package installed: python game.py."""

from pathlib import Path
from types import SimpleNamespace

import pyxel

from nightengine import A, Game, Scene
from nightengine.host.app import App, AppConfig
from nightengine.host.audio import AudioManager


class Counter(Scene):
    def update(self, game, inp):
        if inp.hit(A):
            game.count += 1


class Example(Game):
    def __init__(self, seed=0, target=None):
        super().__init__(seed, target)
        self.count = 0
        self.push(Counter())


def draw(game):
    pyxel.cls(1)
    pyxel.text(20, 20, f"NightEngine: press Space ({game.count})", 7)


if __name__ == "__main__":
    renderer = SimpleNamespace(setup=lambda: None, draw=draw)
    App(
        AppConfig(
            "NightEngine Example", 320, 180, {A: (pyxel.KEY_SPACE,)}, Path(__file__).resolve().parent / "replays"
        ),
        Example,
        renderer,
        AudioManager({}, {}),
        snapshot=lambda game: [game.frame, game.count],
        inspect=lambda game: {"count": game.count},
    )
