"""Build targets: where the game runs, at what size, and with which controls. Pure, so rules can read it.

A game names its targets in `pyproject.toml`, one table each. Every key is optional:

    [tool.nightengine.targets.desktop]
    controls = ["keyboard", "mouse"]

    [tool.nightengine.targets.android]
    controls = ["touch"]
    width = 854        # 0 or absent: the game's own size ([tool.nightengine.web] width and height)
    height = 480

The builds write the chosen target into the app as the module `nightengine_build` (see `build_module`), so a
packaged game knows its target with no pyproject. A source run picks one with `NIGHTENGINE_TARGET` (the
`--target` option of a game's entry script sets it), and falls back to `desktop`.

`Game.step` drops pointers the target does not take (see `Target.accepts`). A target with only touch takes the
mouse as a finger, so `--target android` on a desktop shows and plays the touch layout. The App saves the target in
the recording, so a replay rebuilds the same layout on any machine.
"""

import importlib
import os
import tomllib
from dataclasses import dataclass, field
from pathlib import Path

from .pointer import Pointer, PointerKind

KEYBOARD, MOUSE, TOUCH = "keyboard", "mouse", "touch"  # Keyboard includes gamepads. Touch includes pens.
CONTROLS = (KEYBOARD, MOUSE, TOUCH)
NAMES = ("desktop", "web", "android")
ENV = "NIGHTENGINE_TARGET"
BUILD_MODULE = "nightengine_build"


@dataclass(frozen=True)
class Target:
    name: str = "desktop"
    width: int = 0  # 0: the game's own size.
    height: int = 0
    controls: frozenset[str] = field(default_factory=lambda: frozenset(CONTROLS))

    def __post_init__(self):
        unknown = set(self.controls) - set(CONTROLS)
        if unknown:
            raise ValueError(f"unknown controls {sorted(unknown)}; use {', '.join(CONTROLS)}")
        if self.width < 0 or self.height < 0 or bool(self.width) != bool(self.height):
            raise ValueError("set both width and height, or neither")
        object.__setattr__(self, "controls", frozenset(self.controls))

    @property
    def touch_only(self) -> bool:
        """A phone build: touch is the only control, so the game shows its touch layout from the start."""
        return self.controls == {TOUCH}

    def takes(self, kind: PointerKind) -> bool:
        """True if the game should see contacts of this kind. A touch-only target takes the mouse as a finger."""
        if kind == PointerKind.MOUSE:
            return MOUSE in self.controls or self.touch_only
        return TOUCH in self.controls

    def accepts(self, p: Pointer) -> bool:
        return self.takes(p.kind)

    def sized(self, width: int, height: int) -> "Target":
        """This target with the game's size filled in where it has none."""
        if self.width:
            return self
        return Target(self.name, width, height, self.controls)

    def to_dict(self) -> dict:
        return {"name": self.name, "width": self.width, "height": self.height, "controls": sorted(self.controls)}

    @classmethod
    def from_dict(cls, data: dict) -> "Target":
        return cls(data.get("name", "desktop"), data.get("width", 0), data.get("height", 0),
                   frozenset(data.get("controls", CONTROLS)))  # fmt: skip


def read_targets(root: Path) -> dict[str, Target]:
    """The `[tool.nightengine.targets.*]` tables of a game's pyproject.toml. Names it does not list get defaults."""
    data = tomllib.loads((root / "pyproject.toml").read_text())
    tables = data.get("tool", {}).get("nightengine", {}).get("targets", {})
    return {name: Target.from_dict({**table, "name": name}) for name, table in tables.items()}


def named(root: Path, name: str) -> Target:
    """One target of a game, by name."""
    if name not in NAMES:
        raise ValueError(f"unknown target {name!r}; use {', '.join(NAMES)}")
    return read_targets(root).get(name, Target(name))


def build_module(target: Target) -> str:
    """The source of `nightengine_build.py`, which a build puts next to the game's entry script."""
    return f'"""Written by the nightengine build. Do not edit."""\n\nTARGET = {target.to_dict()!r}\n'


def current(root: Path | None = None) -> Target:
    """The target this process runs as: the build's, else `NIGHTENGINE_TARGET` from the game's pyproject.toml
    (found from `root` or the working directory up), else the default desktop target."""
    try:
        built = importlib.import_module(BUILD_MODULE)
    except ImportError:
        pass
    else:
        return Target.from_dict(built.TARGET)
    name = os.environ.get(ENV) or "desktop"
    start = (root or Path.cwd()).resolve()
    for directory in (start, *start.parents):
        if (directory / "pyproject.toml").is_file():
            return named(directory, name)
    if name not in NAMES:
        raise ValueError(f"unknown target {name!r}; use {', '.join(NAMES)}")
    return Target(name)
