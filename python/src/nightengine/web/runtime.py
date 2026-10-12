"""The Pyxel web runtime as local files, for a build that works offline (an APK, a kiosk, a LAN with no internet).

`vendor(out, pyxel_version)` writes `out/pyxel/` (pyxel.js, its stylesheet, images, import hook, and the Pyxel
wheel) and `out/pyodide/v<version>/` (the Pyodide core). The copy of pyxel.js loads Pyodide from there instead of
the CDN. The path keeps the `v<version>/` folder because pyxel.js reads the Pyodide version from it.
Files are downloaded once into a cache: `$NIGHTENGINE_CACHE`, or `~/.cache/nightengine`.
Only the standard library is used, so the build never imports pyxel.
"""

import os
import re
import shutil
import urllib.request
from pathlib import Path

PYXEL_CDN = "https://cdn.jsdelivr.net/gh/kitao/pyxel@{version}/wasm/"
PYXEL_FILES = ("pyxel.js", "pyxel.css", "import_hook.py")
# The Pyodide core. The Pyxel wheel needs no other Pyodide package.
PYODIDE_FILES = ("pyodide.js", "pyodide.asm.mjs", "pyodide.asm.wasm", "python_stdlib.zip", "pyodide-lock.json")


def cache_dir() -> Path:
    return Path(os.environ.get("NIGHTENGINE_CACHE") or Path.home() / ".cache" / "nightengine")


def fetch(url: str, path: Path) -> Path:
    """Download `url` to `path` unless it is already there."""
    if path.is_file():
        return path
    path.parent.mkdir(parents=True, exist_ok=True)
    partial = path.with_name(path.name + ".part")
    with urllib.request.urlopen(url, timeout=120) as response, partial.open("wb") as file:
        shutil.copyfileobj(response, file)
    partial.replace(path)
    return path


def pyxel_assets(pyxel_js: str) -> tuple[str, str, list[str]]:
    """From pyxel.js: the Pyodide URL, the wheel file name, and the image paths it loads."""
    pyodide = re.search(r'PYODIDE_URL\s*=\s*"([^"]+)"', pyxel_js)
    wheel = re.search(r'PYXEL_WHEEL_PATH\s*=\s*"([^"]+)"', pyxel_js)
    if not pyodide or not wheel:
        raise ValueError("pyxel.js has no PYODIDE_URL or PYXEL_WHEEL_PATH: this Pyxel version is not supported")
    images = sorted(set(re.findall(r"images/[\w.]+", pyxel_js)))
    return pyodide.group(1), wheel.group(1), images


def local_pyodide(pyodide_url: str) -> str:
    """Where the build keeps pyodide.js, relative to the page: pyodide/v<version>/pyodide.js."""
    version = re.search(r"/(v[\d.]+)/", pyodide_url)
    if not version:
        raise ValueError(f"no Pyodide version in {pyodide_url}: this Pyxel version is not supported")
    return f"pyodide/{version.group(1)}/pyodide.js"


def local_pyxel_js(pyxel_js: str, pyodide_url: str) -> str:
    """pyxel.js with Pyodide loaded from the build."""
    return pyxel_js.replace(f'"{pyodide_url}"', f'"{local_pyodide(pyodide_url)}"')


def vendor(out: Path, pyxel_version: str) -> None:
    cache = cache_dir() / f"pyxel-{pyxel_version}"
    base = PYXEL_CDN.format(version=pyxel_version)
    pyxel_js = fetch(base + "pyxel.js", cache / "pyxel" / "pyxel.js").read_text()
    pyodide_url, wheel, images = pyxel_assets(pyxel_js)
    for name in (*PYXEL_FILES, wheel, *images):
        source = fetch(base + name, cache / "pyxel" / name)
        target = out / "pyxel" / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
    (out / "pyxel" / "pyxel.js").write_text(local_pyxel_js(pyxel_js, pyodide_url))
    pyodide_base = pyodide_url.rsplit("/", 1)[0] + "/"
    for name in PYODIDE_FILES:
        source = fetch(pyodide_base + name, cache / "pyodide" / name)
        target = out / local_pyodide(pyodide_url).rsplit("/", 1)[0] / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, target)
