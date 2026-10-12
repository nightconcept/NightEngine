"""Check that a game's justfile exposes the standard surface and nothing more.

    python -m nightengine.recipes [--root DIR]

Every game has the same public recipes, so agents never read a justfile to find a command.
Recipe bodies are the game's own. Helpers are private: `[private]` or a name that starts with `_`.
The surface is in docs/games/commands.md.
"""

import argparse
import json
import shutil
import subprocess
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Spec:
    required: bool
    variadic: bool = False  # The last parameter collects every extra argument (`*args`).
    takes_file: bool = False  # The first parameter is a recording path.


SURFACE = {
    "setup": Spec(required=True),
    "check": Spec(required=True),
    "lint": Spec(required=True),
    "fmt": Spec(required=True),
    "run": Spec(required=True, variadic=True),
    "dev": Spec(required=True, variadic=True),
    "build": Spec(required=False, variadic=True),
    "replay": Spec(required=False, takes_file=True),
    "frames": Spec(required=False, takes_file=True),
    "autoplay": Spec(required=False),
    "golden": Spec(required=False),
}


def problems(dump: dict) -> list[str]:
    """Return every way a `just --dump --dump-format json` result breaks the surface."""
    found: list[str] = []
    recipes = dump.get("recipes", {})
    public = {name: r for name, r in recipes.items() if not r.get("private")}

    for name, spec in SURFACE.items():
        if spec.required and name not in public:
            found.append(f"missing recipe `{name}`")

    for name, recipe in sorted(public.items()):
        spec = SURFACE.get(name)
        if spec is None:
            found.append(f"`{name}` is not a standard recipe: use `just dev`, or make it private (`_{name}`)")
            continue
        if not recipe.get("doc"):
            found.append(f"`{name}` needs a doc comment (`# ...` above it), which `just --list` shows")
        params = recipe.get("parameters", [])
        if spec.variadic and (not params or params[-1].get("kind") not in ("star", "plus")):
            found.append(f"`{name}` must end with a variadic parameter, for example `{name} *args`")
        if spec.takes_file and not params:
            found.append(f"`{name}` must take a recording path as its first parameter")

    dev = public.get("dev")
    if dev and dev.get("doc") and "=" not in dev["doc"]:
        found.append("the `dev` doc comment must list its keys, for example `# Start a state: level=N boss=NAME`")

    for alias, target in sorted(dump.get("aliases", {}).items()):
        if not target.get("private", alias.startswith("_")):
            found.append(f"alias `{alias}` adds to the surface: remove it")
    for module in sorted(dump.get("modules", {})):
        found.append(f"module `{module}` adds to the surface: move its recipes into private helpers")
    return found


def dump(root: Path) -> dict:
    just = shutil.which("just")
    if just is None:
        raise SystemExit("just is not installed (run `mise install`)")
    result = subprocess.run(
        [just, "--dump", "--dump-format", "json"], cwd=root, capture_output=True, text=True, check=False
    )
    if result.returncode != 0:
        raise SystemExit(result.stderr.strip() or "just --dump failed")
    return json.loads(result.stdout)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--root", type=Path, default=Path.cwd(), help="the game repository (default: here)")
    args = parser.parse_args(argv)
    found = problems(dump(args.root))
    for problem in found:
        print(f"justfile: {problem}")
    print(f"justfile: {len(found)} problems" if found else "justfile: standard surface")
    return 1 if found else 0
