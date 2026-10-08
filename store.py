"""Small saved files: settings, saves, hi-scores. No pyxel here: `host.storage.open_store` picks the place.

A store keeps text by key. `FileStore` writes one file `<key>.json` in a folder, atomically. `MemoryStore` is for
tests. `load_json` reads a key as JSON and never fails: a missing key gives the default, and a key that is not JSON
gives the default and is moved aside to `<key>.bad`, so the player's data is never lost.
"""

import json
import os
from pathlib import Path
from typing import Any


class Store:
    """Text by key. A subclass overrides `read`, `write`, and `move_aside`."""

    def read(self, key: str) -> str | None:
        """The text under `key`, or None when there is none."""
        raise NotImplementedError

    def write(self, key: str, text: str):
        raise NotImplementedError

    def move_aside(self, key: str):
        """Keep a bad file as `<key>.bad`, out of the way of the next read."""
        raise NotImplementedError


class MemoryStore(Store):
    def __init__(self, data: dict[str, str] | None = None):
        self.data = dict(data or {})

    def read(self, key: str) -> str | None:
        return self.data.get(key)

    def write(self, key: str, text: str):
        self.data[key] = text

    def move_aside(self, key: str):
        if key in self.data:
            self.data[f"{key}.bad"] = self.data.pop(key)


class FileStore(Store):
    """One file `<key>.json` for each key in `folder`. The folder is made on the first write."""

    def __init__(self, folder: Path | str):
        self.folder = Path(folder)

    def path(self, key: str) -> Path:
        return self.folder / f"{key}.json"

    def read(self, key: str) -> str | None:
        try:
            return self.path(key).read_text(encoding="utf-8")
        except FileNotFoundError:
            return None

    def write(self, key: str, text: str):
        """Write a temp file, then replace the old file with it, so a crash never leaves half a file."""
        self.folder.mkdir(parents=True, exist_ok=True)
        tmp = self.folder / f".{key}.json.tmp"
        tmp.write_text(text, encoding="utf-8")
        os.replace(tmp, self.path(key))

    def move_aside(self, key: str):
        if self.path(key).is_file():
            os.replace(self.path(key), self.folder / f"{key}.bad.json")


def load_json(store: Store | None, key: str, default: Any = None) -> Any:
    """`key` read as JSON. A missing key, JSON null, no store, or a store that cannot be read gives `default`. A key
    that is not JSON gives `default` and is moved aside (`<key>.bad`)."""
    if store is None:
        return default
    try:
        text = store.read(key)
    except OSError:
        return default
    if text is None:
        return default
    try:
        value = json.loads(text)
        return default if value is None else value
    except ValueError:
        try:
            store.move_aside(key)
        except OSError:
            pass
        return default
