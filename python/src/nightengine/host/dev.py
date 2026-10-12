"""In-process developer controls, also available through Pyodide's current App."""

from pathlib import Path

import pyxel

from ..trace import digest


def positive_frames(value):
    if type(value) is not int or value < 1:
        raise ValueError("frames must be a positive integer")
    return value


class Controls:
    def __init__(self, app, inspect=None, snapshot=None):
        self.app, self.inspect, self.snapshot = app, inspect, snapshot
        self.paused, self.steps = False, 0
        self.held, self.held_frames = 0, 0

    def state(self):
        game = self.app.game
        scene = game.scene
        out = dict(self.inspect(game)) if self.inspect else {}
        out.update(frame=game.frame, scene=getattr(scene, "name", type(scene).__name__), paused=self.paused)
        if self.snapshot:
            out["hash"] = self.hash()
        return out

    def hash(self):
        if self.snapshot is None:
            raise ValueError("a snapshot callback is required for state hashes")
        return digest(self.snapshot(self.app.game))

    def pause(self):
        self.paused = True
        return self.app.game.frame

    def resume(self):
        self.paused, self.steps = False, 0
        return self.app.game.frame

    def step(self, n=1):
        self.steps += positive_frames(n)
        self.paused = True
        return self.app.game.frame + self.steps

    def hold(self, code, frames=1):
        frames = positive_frames(frames)
        if type(code) is not int or code < 0 or code & ~self.app.game.input_mask:
            raise ValueError("code must contain only this game's input bits")
        if self.app.replay is not None:
            raise ValueError("cannot inject input into a replay")
        self.held, self.held_frames = code, frames
        return True

    def advance(self):
        """Allow one ordinary host update; paused window updates still draw and handle quit."""
        if self.paused and not self.steps:
            return False
        if self.steps:
            self.steps -= 1
        return True

    def buttons(self):
        if not self.held_frames:
            return 0
        self.held_frames -= 1
        return self.held

    def shot(self, path="shot.png"):
        self.app.draw()
        path = Path(path)
        path.parent.mkdir(parents=True, exist_ok=True)
        pyxel.screen.save(str(path), 1)
        return str(path)

    def save(self, path=None):
        return self.app.save(path)
