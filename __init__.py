"""nightengine: a small, game-agnostic engine on top of Pyxel.

This file re-exports the pure core. It never imports `nightengine.host`, so importing the package never imports pyxel.
"""

from .autopilot import Tapper
from .bindings import Binding, Bindings
from .canvas import Canvas, mirror, noise
from .content import ContentError, check_refs, read_dir, read_json, records
from .core import Game
from .fx import ScreenFx
from .inputs import ALL, DOWN, LEFT, MENU, RIGHT, UP, A, B, Button, C, Input, InputTracker, declare
from .pointer import Pointer, PointerKind, PointerPhase
from .registry import Registry
from .replay import Recording
from .scene import Scene, SceneStack
from .store import FileStore, MemoryStore, Store, load_json
from .systems import System, Systems
from .target import KEYBOARD, MOUSE, TOUCH, Target
from .testing import Driver

__version__ = "0.0.1"

__all__ = [
    "A", "ALL", "B", "C", "DOWN", "KEYBOARD", "LEFT", "MENU", "MOUSE", "RIGHT", "TOUCH", "UP",
    "Binding", "Bindings", "Button", "Canvas", "ContentError", "Driver", "FileStore", "Game", "Input", "InputTracker",
    "MemoryStore", "Pointer", "PointerKind", "PointerPhase",
    "Recording", "Registry",
    "Scene", "SceneStack", "ScreenFx", "Store", "System", "Systems", "Tapper", "Target",
    "check_refs", "declare", "load_json", "mirror", "noise", "read_dir", "read_json", "records",
]  # fmt: skip
