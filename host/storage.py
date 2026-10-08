"""Where a game's small files live: the user's data folder on the desktop, browser storage in a page."""

from pathlib import Path

import pyxel

from ..store import FileStore, Store
from . import platform


class PageStore(Store):
    """Browser storage through the page bridge (`platform.save` and `platform.load`)."""

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


def open_store(vendor: str, app: str) -> Store:
    """In a page, browser storage. On the desktop, `pyxel.user_data_dir(vendor, app)`, which pyxel makes if needed."""
    if platform.available():
        return PageStore(f"{vendor}/{app}")
    return FileStore(Path(pyxel.user_data_dir(vendor, app)))
