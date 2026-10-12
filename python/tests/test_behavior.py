"""Frame and input behavior shared with the Lua implementation."""

import json
import unittest
from pathlib import Path

from nightengine import A, Game, Scene


class Counter(Scene):
    def update(self, game, inp):
        if inp.down(A):
            game.count += 1
        if inp.hit(A):
            game.cue("act")


class BehaviorTest(unittest.TestCase):
    def test_shared_frame_input_and_cue_contract(self):
        fixture = json.loads((Path(__file__).resolve().parents[2] / "contract/behavior.json").read_text())
        game = Game(7)
        game.count = 0
        game.push(Counter())
        states = [[game.frame, game.count, list(game.cues)]]
        for code in fixture["codes"]:
            game.step(code)
            states.append([game.frame, game.count, list(game.cues)])
        self.assertEqual(states, fixture["states"])
