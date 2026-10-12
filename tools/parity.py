"""Check parity.toml against the three implementations and write docs/concepts/parity.md.

python tools/parity.py           check, then rewrite the generated part of the parity page
python tools/parity.py --check   check only; also fail when the parity page is out of date

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

# The ladder, lowest first. A game moves up it, so a capability on a runtime must reach the ones above.
RUNTIMES = ("python", "lua", "csharp")
NAMES = {"python": "Python/Pyxel", "lua": "Lua/LÖVE", "csharp": "C#/MonoGame"}
SOURCES = {
    "python": ROOT / "python/src/nightengine",
    "lua": ROOT / "lua",
    "csharp": ROOT / "csharp",
}
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


def definition(runtime: str, name: str) -> re.Pattern[str]:
    n = re.escape(name)
    if runtime == "python":
        return re.compile(rf"^\s*(?:async\s+)?(?:def|class)\s+{n}\b|^{n}\s*[:=]", re.M)
    if runtime == "lua":
        function = rf"^\s*(?:local\s+)?function\s+(?:[\w.]+[.:])?{n}\s*\("
        return re.compile(rf"{function}|^\s*(?:local\s+)?(?:[\w.]+\.)?{n}\s*=", re.M)
    modifiers = r"(?:public|protected|private|internal|static|override|virtual|abstract|sealed|const)"
    return re.compile(
        rf"\b(?:class|record|struct|interface|enum)\s+{n}\b|^[^\n]*\b{modifiers}\b[^\n]*\b{n}\s*(?:\(|=|\{{|;|,|=>)",
        re.M,
    )


def check_at(runtime: str, at: str) -> tuple[str | None, Path | None]:
    """Return (error, file) for one `file:Symbol` location."""
    file, _, symbol = at.partition(":")
    path = SOURCES[runtime] / file
    if not path.is_file():
        return f"{file} does not exist", None
    if not symbol:
        return None, path
    text = path.read_text(encoding="utf-8")
    if symbol.startswith('"'):
        return (None if symbol.strip('"') in text else f"{file} has no text {symbol}"), path
    for part in symbol.split("."):
        if not definition(runtime, part).search(text):
            return f"{file} defines no {part}", path
    return None, path


def version_tuple(text: str) -> tuple[int, ...]:
    return tuple(int(x) for x in re.findall(r"\d+", text)[:3])


def check(manifest: dict) -> tuple[list[str], list[str]]:
    errors: list[str] = []
    claimed: dict[str, set[Path]] = {r: set() for r in RUNTIMES}
    engine = tomllib.loads((ROOT / "python/pyproject.toml").read_text(encoding="utf-8"))["project"]["version"]
    seen: set[str] = set()

    for cap in manifest["capability"]:
        cid = cap.get("id", "?")
        if cid in seen:
            errors.append(f"{cid}: duplicate id")
        seen.add(cid)
        layer = cid.partition(".")[0]
        if layer not in LAYERS:
            errors.append(f"{cid}: layer must be one of {', '.join(LAYERS)}")
        if not cap.get("summary"):
            errors.append(f"{cid}: no summary")
        fixture = cap.get("fixture")
        if layer == "contract" and not fixture and not cap.get("debt"):
            errors.append(f"{cid}: a contract capability needs a fixture or a debt")
        if fixture:
            if not (FIXTURES / fixture).is_file():
                errors.append(f"{cid}: fixture {fixture} does not exist")
            for runtime, tests in TESTS.items():
                if not any(fixture in p.read_text(encoding="utf-8") for p in tests.rglob("*") if p.is_file()):
                    errors.append(f"{cid}: no {runtime} test reads {fixture}")

        statuses = {}
        for runtime in RUNTIMES:
            entry = cap.get(runtime)
            if not isinstance(entry, dict):
                errors.append(f"{cid}: no {runtime} entry")
                continue
            status = entry.get("status")
            statuses[runtime] = status
            if status not in STATUSES:
                errors.append(f"{cid}.{runtime}: status must be one of {', '.join(STATUSES)}")
                continue
            if status == "yes":
                locations = entry.get("at")
                locations = [locations] if isinstance(locations, str) else locations or []
                if not locations:
                    errors.append(f"{cid}.{runtime}: yes needs `at`")
                for at in locations:
                    error, path = check_at(runtime, at)
                    if error:
                        errors.append(f"{cid}.{runtime}: {error}")
                    if path:
                        claimed[runtime].add(path)
            elif not entry.get("reason"):
                errors.append(f"{cid}.{runtime}: {status} needs a reason")
            if status == "undecided":
                since = entry.get("since")
                if not since:
                    errors.append(f"{cid}.{runtime}: undecided needs `since`")
                elif version_tuple(engine) > version_tuple(since):
                    errors.append(f"{cid}.{runtime}: undecided since {since}; decide before release {engine}")
            if layer == "contract" and status != "yes":
                errors.append(f"{cid}.{runtime}: every runtime implements the contract")

        if not cap.get("specific"):
            for low, runtime in enumerate(RUNTIMES):
                if statuses.get(runtime) != "yes":
                    continue
                for higher in RUNTIMES[low + 1 :]:
                    if statuses.get(higher) == "none":
                        errors.append(
                            f"{cid}.{higher}: ladder rule: {runtime} has it, so {higher} must be "
                            "yes, planned, native or undecided (or mark the capability specific)"
                        )

    ignore = manifest.get("ignore", {})
    for runtime, patterns in SCANNED.items():
        skipped = {SOURCES[runtime] / p for p in ignore.get(runtime, [])}
        for pattern in patterns:
            for path in sorted(SOURCES[runtime].glob(pattern)):
                if path not in claimed[runtime] and path not in skipped:
                    rel = path.relative_to(SOURCES[runtime]).as_posix()
                    errors.append(f"{runtime}: {rel} belongs to no capability (add one, or list it in [ignore])")

    debts = [f"`{c['id']}`: {c['debt']}" for c in manifest["capability"] if c.get("debt")]
    return errors, debts


CELL = {"yes": "Yes", "planned": "Planned", "native": "Native", "none": "None", "undecided": "**Undecided**"}


def render(manifest: dict, debts: list[str]) -> str:
    lines = ["", "_Generated from `parity.toml` by `just parity`. Do not edit by hand._", ""]
    caps = manifest["capability"]
    for layer in LAYERS:
        rows = [c for c in caps if c["id"].partition(".")[0] == layer]
        lines += [f"### {layer.capitalize()}", "", f"| Capability | {' | '.join(NAMES[r] for r in RUNTIMES)} |"]
        lines.append("| --- |" + " --- |" * len(RUNTIMES))
        for c in rows:
            cells = [CELL[c[r]["status"]] for r in RUNTIMES]
            lines.append(f"| `{c['id'].partition('.')[2]}`: {c['summary']} | {' | '.join(cells)} |")
        lines.append("")
    open_items = [
        f"- `{c['id']}` on {NAMES[r]}: {c[r]['status']}. {c[r]['reason']}"
        for c in caps
        for r in RUNTIMES
        if c[r]["status"] in ("planned", "undecided")
    ]
    lines += ["### Open decisions and catch-up work", "", *(open_items or ["None."]), ""]
    lines += ["### Contract debts", "", *(f"- {d}" for d in debts or ["None."]), ""]
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--check", action="store_true", help="do not write; fail when the page is out of date")
    args = parser.parse_args()

    manifest = tomllib.loads(MANIFEST.read_text(encoding="utf-8"))
    errors, debts = check(manifest)
    page = PAGE.read_text(encoding="utf-8")
    head, _, rest = page.partition(START)
    _, _, tail = rest.partition(END)
    updated = f"{head}{START}\n{render(manifest, debts)}\n{END}{tail}"
    if args.check and updated != page:
        errors.append("docs/concepts/parity.md is out of date: run `just parity`")
    if not args.check and not errors:
        PAGE.write_text(updated, encoding="utf-8")

    for error in errors:
        print(f"parity: {error}", file=sys.stderr)
    capabilities = len(manifest["capability"])
    print(f"parity: {capabilities} capabilities, {len(errors)} errors, {len(debts)} contract debts")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
