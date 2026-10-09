"""Build a game into static web files: `game.pyxapp`, `debug.pyxapp` (the pointer check), and the page.

The game describes itself in its pyproject.toml:

    [tool.nightengine.web]
    title = "Nightrunner"
    width = 640
    height = 360
    entry = "game.py"                          # optional, the default
    packages = ["nightrunner", "nightengine"]  # their .py files, without tests/
    assets = ["assets"]                        # every file under these dirs

`--target` picks the build target (`web` unless given; see target.py). The build writes it into the app, and the
page takes the target's size.

The page loads the Pyxel runtime from a CDN, at the version installed in the game's environment.
`build --offline` copies the runtime into the build instead (see runtime.py), so the page needs no network.
"""

import argparse
import functools
import http.server
import shutil
import tempfile
import tomllib
import webbrowser
import zipfile
from dataclasses import dataclass
from importlib.metadata import version
from pathlib import Path

from .. import target as targets
from . import runtime

STATIC = Path(__file__).resolve().parent / "static"
SKIP_DIRS = {"__pycache__", "tests", "docs", ".git"}
DEBUG_ENTRY = "debug.py"


@dataclass(frozen=True)
class WebConfig:
    title: str
    width: int
    height: int
    entry: str = "game.py"
    packages: tuple[str, ...] = ()
    assets: tuple[str, ...] = ()


def read_config(root: Path) -> WebConfig:
    data = tomllib.loads((root / "pyproject.toml").read_text())
    table = data.get("tool", {}).get("nightengine", {}).get("web")
    if table is None:
        raise ValueError(f"{root / 'pyproject.toml'} has no [tool.nightengine.web] table")
    table = {**table, "packages": tuple(table.get("packages", ())), "assets": tuple(table.get("assets", ()))}
    return WebConfig(**table)


def package_files(root: Path, package: str) -> list[Path]:
    """The .py files of a package directory, recursively, without caches, tests, or docs."""
    base = root / package
    if not base.is_dir():
        raise FileNotFoundError(f"package directory {base} not found")
    return sorted(p for p in base.rglob("*.py") if not SKIP_DIRS & set(p.relative_to(root).parts))


def asset_files(root: Path, directory: str) -> list[Path]:
    base = root / directory
    if not base.is_dir():
        raise FileNotFoundError(f"asset directory {base} not found")
    return sorted(p for p in base.rglob("*") if p.is_file() and not SKIP_DIRS & set(p.relative_to(root).parts))


def game_files(root: Path, config: WebConfig) -> list[Path]:
    """The entry script, the packages' .py files, and the assets: everything a .pyxapp of the game holds."""
    files = [root / config.entry]
    for package in config.packages:
        files += package_files(root, package)
    for directory in config.assets:
        files += asset_files(root, directory)
    return files


def target_script(config: WebConfig, target: targets.Target) -> dict[str, str]:
    """The `nightengine_build` module, next to the entry script so it can be imported, with the target sized."""
    path = (Path(config.entry).parent / f"{targets.BUILD_MODULE}.py").as_posix()
    return {path: targets.build_module(target.sized(config.width, config.height))}


def write_app(path: Path, root: Path, entry: str, files: list[Path], scripts: dict[str, str] | None = None):
    """A .pyxapp: a zip with everything under app/ and the startup script named in app/.pyxapp_startup_script."""
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("app/.pyxapp_startup_script", entry)
        for name, text in (scripts or {}).items():
            archive.writestr(f"app/{name}", text)
        for file in files:
            archive.write(file, "app/" + file.relative_to(root).as_posix())


def runtime_version(installed: str) -> str:
    """The Pyxel version the page loads: the installed one without a local label. A fork such as pyxel-ne
    (`2.9.9+ne.2`) has no runtime on the CDN, so the page loads stock Pyxel at the same base version."""
    return installed.split("+", 1)[0]


def build(root: Path, out: Path, config: WebConfig | None = None, offline: bool = False, target: str = "web") -> Path:
    root, out = root.resolve(), out.resolve()
    if out == root or out in root.parents or out == STATIC or STATIC in out.parents:
        raise ValueError("The build output must be its own directory, such as dist/")
    config = config or read_config(root)
    chosen = targets.named(root, target).sized(config.width, config.height)
    out.mkdir(parents=True, exist_ok=True)
    write_app(out / "game.pyxapp", root, config.entry, game_files(root, config), target_script(config, chosen))
    engine = package_files(root, "nightengine") if (root / "nightengine").is_dir() else []
    diagnostic = "from nightengine.host.diagnostic import main\n\nmain()\n"
    write_app(out / "debug.pyxapp", root, DEBUG_ENTRY, engine, {DEBUG_ENTRY: diagnostic})
    for path in STATIC.iterdir():
        if path.is_file():
            shutil.copyfile(path, out / path.name)
    pyxel_version = runtime_version(version("pyxel"))
    if offline:
        runtime.vendor(out, pyxel_version)
        pyxel_js, note = "pyxel/pyxel.js", "Everything runs from this build. No internet access is needed."
    else:
        pyxel_js = runtime.PYXEL_CDN.format(version=pyxel_version) + "pyxel.js"
        note = "The browser runtime needs internet access."
    page = out / "index.html"
    fills = {"PYXEL_JS": pyxel_js, "RUNTIME_NOTE": note, "TITLE": config.title, "WIDTH": chosen.width,
             "HEIGHT": chosen.height}  # fmt: skip
    text = page.read_text()
    for key, value in fills.items():
        text = text.replace("{{" + key + "}}", str(value))
    page.write_text(text)
    return out


def serve(
    root: Path,
    host: str = "127.0.0.1",
    port: int = 8000,
    open_browser: bool = True,
    offline: bool = False,
    target: str = "web",
):
    """Build into a temp dir and serve it. Rebuild by restarting."""
    with tempfile.TemporaryDirectory(prefix="nightengine-web-") as temporary:
        directory = build(root, Path(temporary), offline=offline, target=target)
        handler = functools.partial(http.server.SimpleHTTPRequestHandler, directory=str(directory))
        with http.server.ThreadingHTTPServer((host, port), handler) as server:
            shown = "localhost" if host in ("0.0.0.0", "127.0.0.1") else host
            url = f"http://{shown}:{server.server_port}/"
            print(f"Game: {url}\nPointer check: {url}?app=debug", flush=True)
            if host == "0.0.0.0":
                print(f"LAN: http://<this computer's LAN IP>:{server.server_port}/", flush=True)
            if open_browser:
                webbrowser.open(url)
            try:
                server.serve_forever()
            except KeyboardInterrupt:
                print("\nStopped")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="python -m nightengine.web", description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)  # fmt: skip
    commands = parser.add_subparsers(dest="command", required=True)
    b = commands.add_parser("build", help="write static files (the runtime loads from a CDN unless --offline)")
    b.add_argument("--out", type=Path, default=Path("dist"))
    s = commands.add_parser("serve", help="build and serve locally")
    s.add_argument("--host", default="127.0.0.1", help="0.0.0.0 to test on a phone on the same network")
    s.add_argument("--port", type=int, default=8000)
    s.add_argument("--no-open", action="store_true")
    for p in (b, s):
        p.add_argument("--offline", action="store_true", help="copy the Pyxel and Pyodide runtime into the build")
        p.add_argument("--root", type=Path, default=Path("."), help="the game's root (holds pyproject.toml)")
        p.add_argument("--target", default="web", choices=targets.NAMES, help="the build target (see target.py)")
    args = parser.parse_args(argv)
    if args.command == "build":
        print(f"Built {build(args.root, args.out, offline=args.offline, target=args.target)}")
    else:
        serve(args.root, args.host, args.port, not args.no_open, args.offline, args.target)
    return 0
