"""Check parity.toml against the three implementations and write docs/concepts/parity.md.

python tools/parity.py             check, then rewrite the generated part of the parity page
python tools/parity.py --check     check only; also fail when the parity page is out of date
python tools/parity.py --release   like --check, and fail on any open parity decision

The rules are in docs/concepts/layers.md.
"""

import argparse
import re
import sys
import tomllib
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MANIFEST = ROOT / "parity.toml"
PAGE = ROOT / "docs/concepts/parity.md"
START, END = "<!-- parity:start -->", "<!-- parity:end -->"

RUNTIMES = ("python", "lua", "csharp")
NAMES = {"python": "Python/Pyxel", "lua": "Lua/LÖVE", "csharp": "C#/MonoGame"}
SOURCES = {"python": ROOT / "python/src/nightengine", "lua": ROOT / "lua", "csharp": ROOT / "csharp"}
CSHARP_CORE = SOURCES["csharp"] / "NightEngine.Core"
# Files that must belong to a capability: the cores, hosts and dev tools. Builders are in Platforms.
SCANNED = {
    "python": ("*.py", "host/*.py"),
    "lua": ("*.lua", "host/*.lua"),
    "csharp": ("NightEngine.Core/*.cs", "MonoGame/*.cs"),
}
TESTS = {"python": ROOT / "python/tests", "lua": ROOT / "tests/lua", "csharp": ROOT / "csharp/NightEngine.Tests"}
FIXTURES = ROOT / "tests/fixtures"
LAYERS = ("contract", "core", "host", "dev")
STATUSES = ("yes", "planned", "native", "none", "undecided")
OPEN = ("planned", "undecided")


def pascal(name: str) -> str:
    """The C# spelling of a canonical Python name."""
    if name.isupper():
        return "".join(word.capitalize() for word in name.split("_"))
    return "".join(word[:1].upper() + word[1:] for word in name.split("_"))


def definition(runtime: str, name: str) -> re.Pattern[str]:
    n = re.escape(name)
    assignment = rf"^\s*(?:local\s+)?[\w., ]*\b{n}\b[\w., ]*(?::[^=\n]*)?=(?!=)"
    if runtime == "python":
        return re.compile(rf"^\s*(?:async\s+)?(?:def|class)\s+{n}\b|{assignment}", re.M)
    if runtime == "lua":
        return re.compile(rf"^\s*(?:local\s+)?function\s+(?:[\w.]+[.:])?{n}\s*\(|{assignment}", re.M)
    modifiers = r"(?:public|protected|private|internal|static|override|virtual|abstract|sealed|const)"
    return re.compile(
        rf"\b(?:class|record|struct|interface|enum)\s+{n}\b|^[^\n]*\b{modifiers}\b[^\n]*\b{n}\s*(?:\(|=|\{{|;|,|=>)",
        re.M,
    )


def defines(runtime: str, path: Path, names: list[str]) -> str | None:
    text = path.read_text(encoding="utf-8")
    for name in names:
        if not definition(runtime, name).search(text):
            return name
    return None


def resolve_api(runtime: str, name: str) -> tuple[str | None, Path | None]:
    """Find a canonical API name in one runtime. Return (error, file)."""
    module, *parts = name.split(".")
    if runtime in ("python", "lua"):
        path = SOURCES[runtime] / f"{module}.{'py' if runtime == 'python' else 'lua'}"
        if not path.is_file():
            return f"{path.name} does not exist", None
        missing = defines(runtime, path, parts)
        return (f"{path.name} defines no {missing} (for {name})" if missing else None), path
    if not parts:
        return f"no C# form of module {module}", None
    first = parts[0]
    is_type = first[:1].isupper() and not first.isupper()
    owner, members = (first, parts[1:]) if is_type else (pascal(module), parts)
    members = [pascal(m) for m in members]
    declaration = re.compile(rf"\b(?:class|record|struct|interface|enum)\s+{re.escape(owner)}\b")
    for path in sorted(CSHARP_CORE.glob("*.cs")):
        if declaration.search(path.read_text(encoding="utf-8")):
            missing = defines("csharp", path, members)
            expected = ".".join([owner, *members])
            return (f"{path.name} defines no {missing} (expected {expected} for {name})" if missing else None), path
    return f"no C# type {owner} (for {name})", None


def resolve_at(runtime: str, at: str) -> tuple[str | None, Path | None]:
    """Check one host or dev location, `file:Symbol`. Return (error, file)."""
    file, _, symbol = at.partition(":")
    path = SOURCES[runtime] / file
    if not path.is_file():
        return f"{file} does not exist", None
    if not symbol:
        return None, path
    if symbol.startswith('"'):
        return (None if symbol.strip('"') in path.read_text(encoding="utf-8") else f"{file} has no text {symbol}"), path
    missing = defines(runtime, path, symbol.split("."))
    return (f"{file} defines no {missing}" if missing else None), path


def check(manifest: dict, release: bool) -> tuple[list[str], list[str]]:
    """Return (errors, warnings). Open decisions are warnings, or errors at release."""
    errors: list[str] = []
    warnings: list[str] = []
    claimed: dict[str, set[Path]] = {r: set() for r in RUNTIMES}
    seen: set[str] = set()

    for cap in manifest["capability"]:
        cid = cap.get("id", "?")
        layer = cid.partition(".")[0]
        if cid in seen:
            errors.append(f"{cid}: duplicate id")
        seen.add(cid)
        if layer not in LAYERS:
            errors.append(f"{cid}: layer must be one of {', '.join(LAYERS)}")
            continue
        if not cap.get("summary"):
            errors.append(f"{cid}: no summary")
        api = cap.get("api", [])
        if layer in ("contract", "core") and not api:
            errors.append(f"{cid}: a {layer} capability lists its canonical `api`")

        if layer == "contract":
            if any(r in cap for r in RUNTIMES):
                errors.append(f"{cid}: contract capabilities have no per-runtime entries")
            fixture = cap.get("fixture")
            if not fixture and not cap.get("debt"):
                errors.append(f"{cid}: a contract capability needs a fixture or a debt")
            if fixture:
                if not (FIXTURES / fixture).is_file():
                    errors.append(f"{cid}: fixture {fixture} does not exist")
                for runtime, tests in TESTS.items():
                    if not any(fixture in p.read_text(encoding="utf-8") for p in tests.rglob("*") if p.is_file()):
                        errors.append(f"{cid}: no {runtime} test reads {fixture}")
            for runtime in RUNTIMES:
                for name in api:
                    error, path = resolve_api(runtime, name)
                    if error:
                        errors.append(f"{cid}.{runtime}: {error}")
                    if path:
                        claimed[runtime].add(path)
            continue

        for runtime in RUNTIMES:
            entry = cap.get(runtime)
            if not isinstance(entry, dict):
                errors.append(f"{cid}: no {runtime} entry")
                continue
            status = entry.get("status")
            where = f"{cid}.{runtime}"
            if status not in STATUSES:
                errors.append(f"{where}: status must be one of {', '.join(STATUSES)}")
                continue
            if status != "yes" and not entry.get("reason"):
                errors.append(f"{where}: {status} needs a reason")
            if status == "none" and layer == "core" and not cap.get("specific"):
                errors.append(f"{where}: core is engine-wide; use planned, or mark the capability specific")
            if status == "yes":
                if layer == "core":
                    found = [resolve_api(runtime, name) for name in api]
                else:
                    locations = entry.get("at")
                    locations = [locations] if isinstance(locations, str) else locations or []
                    if not locations:
                        errors.append(f"{where}: yes needs `at`")
                    found = [resolve_at(runtime, at) for at in locations]
                for error, path in found:
                    if error:
                        errors.append(f"{where}: {error}")
                    if path:
                        claimed[runtime].add(path)
            if status in OPEN:
                plan = entry.get("plan")
                problem = None
                if status == "undecided":
                    problem = "undecided"
                elif not plan:
                    problem = "planned with no `plan`"
                elif not (ROOT / plan.partition("#")[0]).is_file():
                    problem = f"plan {plan} does not exist"
                if problem:
                    (errors if release else warnings).append(f"{where}: {problem}")

    ignore = manifest.get("ignore", {})
    for runtime, patterns in SCANNED.items():
        skipped = {SOURCES[runtime] / p for p in ignore.get(runtime, [])}
        for pattern in patterns:
            for path in sorted(SOURCES[runtime].glob(pattern)):
                if path not in claimed[runtime] and path not in skipped:
                    rel = path.relative_to(SOURCES[runtime]).as_posix()
                    errors.append(f"{runtime}: {rel} belongs to no capability (add one, or list it in [ignore])")
    return errors, warnings


CELL = {"yes": "Yes", "planned": "Planned", "native": "Native", "none": "None", "undecided": "**Undecided**"}


def render(manifest: dict) -> str:
    caps = manifest["capability"]
    lines = ["", "_Generated from `parity.toml` by `just parity`. Do not edit by hand._", ""]
    for layer in LAYERS:
        lines += [f"### {layer.capitalize()}", "", f"| Capability | {' | '.join(NAMES[r] for r in RUNTIMES)} |"]
        lines.append("| --- |" + " --- |" * len(RUNTIMES))
        for c in (c for c in caps if c["id"].partition(".")[0] == layer):
            cells = ["Yes"] * len(RUNTIMES) if layer == "contract" else [CELL[c[r]["status"]] for r in RUNTIMES]
            lines.append(f"| `{c['id'].partition('.')[2]}`: {c['summary']} | {' | '.join(cells)} |")
        lines.append("")
    open_items = [
        f"- `{c['id']}` on {NAMES[r]}: {c[r]['status']}. {c[r]['reason']}"
        for c in caps
        for r in RUNTIMES
        if r in c and c[r]["status"] in OPEN
    ]
    debts = [f"- `{c['id']}`: {c['debt']}" for c in caps if c.get("debt")]
    lines += ["### Catch-up work and open decisions", "", *(open_items or ["None."]), ""]
    lines += ["### Contract debts", "", *(debts or ["None."]), ""]
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--check", action="store_true", help="do not write; fail when the page is out of date")
    parser.add_argument("--release", action="store_true", help="--check, and fail on any open parity decision")
    args = parser.parse_args()
    check_only = args.check or args.release

    manifest = tomllib.loads(MANIFEST.read_text(encoding="utf-8"))
    errors, warnings = check(manifest, args.release)
    page = PAGE.read_text(encoding="utf-8")
    head, _, rest = page.partition(START)
    _, _, tail = rest.partition(END)
    updated = f"{head}{START}\n{render(manifest)}\n{END}{tail}"
    if check_only and updated != page:
        errors.append("docs/concepts/parity.md is out of date: run `just parity`")
    if not check_only and not errors:
        PAGE.write_text(updated, encoding="utf-8")

    for warning in warnings:
        print(f"parity: warning: {warning}", file=sys.stderr)
    for error in errors:
        print(f"parity: {error}", file=sys.stderr)
    print(f"parity: {len(manifest['capability'])} capabilities, {len(errors)} errors, {len(warnings)} open decisions")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
