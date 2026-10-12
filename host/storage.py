"""Where a game's small files live: the OS's data folder on the desktop, browser storage in a page.

Nothing here calls pyxel, so a game can read its settings before `pyxel.init` (the window scale goes to init).
"""

import os
import sys
from pathlib import Path

from ..store import FileStore, Store
from . import platform


class PageStore(Store):
    """Browser storage through the page bridge (`platform.save` and `platform.load`). Reads before `platform.init`
    find no bridge and give None."""

    def __init__(self, prefix: str):
        self.prefix = prefix

    def read(self, key: str) -> str | None:
        return platform.load(f"{self.prefix}/{key}")

    def write(self, key: str, text: str):
        platform.save(f"{self.prefix}/{key}", text)

    def move_aside(self, key: str):
        text = self.read(key)
        if text is not None:
            platform.save(f"{self.prefix}/{key}.bad", text)
            platform.save(f"{self.prefix}/{key}", "null")  # Loads as JSON null: the game uses its defaults.


def data_dir(vendor: str, app: str, system: str | None = None, env: dict | None = None, home: Path | None = None) -> Path:
    """The game's folder in the OS's data folder, the same place as SDL_GetPrefPath and Godot's custom user dir:
    `%APPDATA%\\<vendor>\\<app>` on Windows, `~/Library/Application Support/<vendor>/<app>` on macOS, and
    `$XDG_DATA_HOME/<vendor>/<app>` (`~/.local/share`), in lower case, elsewhere. It is made on the first write."""
    system = sys.platform if system is None else system
    env = os.environ if env is None else env
    home = Path.home() if home is None else home
    if system == "win32":
        return Path(env.get("APPDATA") or home / "AppData" / "Roaming") / vendor / app
    if system == "darwin":
        return home / "Library" / "Application Support" / vendor / app
    base = env.get("XDG_DATA_HOME")
    root = Path(base) if base and Path(base).is_absolute() else home / ".local" / "share"
    return root / vendor.lower() / app.lower()


def open_store(vendor: str, app: str) -> Store:
    """In a page, browser storage. On the desktop, a `FileStore` in `data_dir(vendor, app)`. Safe before `pyxel.init`."""
    if sys.platform == "emscripten":
        return PageStore(f"{vendor}/{app}")
    return FileStore(data_dir(vendor, app))
