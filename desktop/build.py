"""Build a game into a desktop program with `pyxel app2exe` (PyInstaller), for the system this runs on.

It reads the game's `[tool.nightengine.web]` table (entry, packages, assets) and `[project] version`, writes
`<slug>.pyxapp`, runs `pyxel app2exe` on it, and zips the program folder to
`<out>/<slug>-<version>-<system>-<machine>.zip`. PyInstaller cannot cross-build: a Windows .exe needs Windows.
The program runs as the `desktop` target (see target.py).
PyInstaller must be importable: `uv run --with pyinstaller python -m nightengine.desktop build`.
"""

import argparse
import importlib.util
import platform
import re
import shutil
import subprocess
import sys
import tomllib
from pathlib import Path

from .. import target as targets
from ..web import build as web


def slug(title: str) -> str:
    return re.sub(r"[^a-z0-9]+", "-", title.lower()).strip("-") or "game"


def target() -> str:
    """windows-amd64, linux-x86_64, macos-arm64, ..."""
    system = {"win32": "windows", "darwin": "macos"}.get(sys.platform, sys.platform)
    return f"{system}-{platform.machine().lower()}"


def build(root: Path, out: Path) -> Path:
    if importlib.util.find_spec("PyInstaller") is None:
        raise RuntimeError("PyInstaller is not installed. Run with: uv run --with pyinstaller ...")
    root, out = root.resolve(), out.resolve()
    config = web.read_config(root)
    version = tomllib.loads((root / "pyproject.toml").read_text()).get("project", {}).get("version", "0.0.1")
    name = slug(config.title)
    work = out / "work"
    if work.exists():
        shutil.rmtree(work)
    work.mkdir(parents=True)
    script = web.target_script(config, targets.named(root, "desktop"))
    web.write_app(work / f"{name}.pyxapp", root, config.entry, web.game_files(root, config), script)
    subprocess.run([sys.executable, "-m", "pyxel", "app2exe", f"{name}.pyxapp"], cwd=work, check=True)
    archive = out / f"{name}-{version}-{target()}"
    return Path(shutil.make_archive(str(archive), "zip", work / "dist"))


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="python -m nightengine.desktop", description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)  # fmt: skip
    commands = parser.add_subparsers(dest="command", required=True)
    b = commands.add_parser("build", help="build the program for this system and zip it")
    b.add_argument("--out", type=Path, default=Path("dist/desktop"))
    b.add_argument("--root", type=Path, default=Path("."), help="the game's root (holds pyproject.toml)")
    args = parser.parse_args(argv)
    print(f"Built {build(args.root, args.out)}")
    return 0
