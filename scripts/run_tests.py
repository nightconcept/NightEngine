#!/usr/bin/env python3
"""NightEngine test runner.

Wraps ``dotnet test`` with environment, filtering, and result surfacing that
works on headed workstations, headless CI, and developer laptops where SDL
video may not be available.

Usage examples:
    python scripts/run_tests.py                   # default: headless automated tests
    python scripts/run_tests.py --headed           # run with real SDL video driver
    python scripts/run_tests.py --all              # include manual/skipped tests
    python scripts/run_tests.py --filter Filesystem  # dotnet filter expression
    python scripts/run_tests.py --find Timer       # list tests matching substring
    python scripts/run_tests.py --group Filesystem  # run a single test group
    python scripts/run_tests.py --failures-only    # re-run only previously failed tests
    python scripts/run_tests.py --dry-run          # show command without executing
"""

from __future__ import annotations

import argparse
import os
import re
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import NamedTuple

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

ROOT = Path(__file__).resolve().parent.parent
SOLUTION = ROOT / "night-mono.slnx"
RESULTS_DIR = ROOT / "test-results"
FAILURES_FILE = ROOT / ".last-test-failures"

# xUnit trait values
TRAIT_AUTOMATED = "Automated"
TRAIT_MANUAL = "Manual"

# Exit codes
EXIT_OK = 0
EXIT_FAIL = 1
EXIT_PARTIAL = 2  # some passed, some skipped

# ANSI helpers (disabled when not a tty)
_USE_COLOR = sys.stdout.isatty()


def _c(code: int, text: str) -> str:
    return f"\033[{code}m{text}\033[0m" if _USE_COLOR else text


RED = lambda t: _c(31, t)
GREEN = lambda t: _c(32, t)
YELLOW = lambda t: _c(33, t)
CYAN = lambda t: _c(36, t)
BOLD = lambda t: _c(1, t)
DIM = lambda t: _c(2, t)


# ---------------------------------------------------------------------------
# Result types
# ---------------------------------------------------------------------------

class TestResult(NamedTuple):
    name: str
    outcome: str  # Passed, Failed, NotExecuted, Error


class RunSummary(NamedTuple):
    total: int
    passed: int
    failed: int
    skipped: int
    results: list[TestResult]


# ---------------------------------------------------------------------------
# TRX parsing
# ---------------------------------------------------------------------------

NS = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"


def parse_trx(trx_path: Path) -> RunSummary:
    """Parse a ``.trx`` results file into a RunSummary."""
    tree = ET.parse(trx_path)
    root = tree.getroot()

    # Counters from ResultSummary
    counters = root.find(f".//{NS}Counters")
    if counters is None:
        counters = root.find(".//Counters")

    if counters is not None:
        total = int(counters.get("total", "0"))
        passed = int(counters.get("passed", "0"))
        failed = int(counters.get("failed", "0"))
        skipped = total - passed - failed
    else:
        total = passed = failed = skipped = 0

    # Individual test results (try namespaced first, then plain)
    results: list[TestResult] = []
    for ur in root.iter(f"{NS}UnitTestResult"):
        name = ur.get("testName", "")
        outcome = ur.get("outcome", "")
        results.append(TestResult(name=name, outcome=outcome))
    if not results:
        for ur in root.iter("UnitTestResult"):
            name = ur.get("testName", "")
            outcome = ur.get("outcome", "")
            results.append(TestResult(name=name, outcome=outcome))

    return RunSummary(total=total, passed=passed, failed=failed, skipped=skipped, results=results)


def print_summary(summary: RunSummary) -> None:
    """Print a human-friendly summary."""
    if summary.failed > 0:
        print(BOLD(RED(f"\n✗ {summary.failed} test(s) FAILED out of {summary.total}")))
    elif summary.skipped > 0 and summary.passed == summary.total - summary.skipped:
        print(BOLD(YELLOW(f"\n⚠ {summary.passed} passed, {summary.skipped} skipped out of {summary.total}")))
    else:
        print(BOLD(GREEN(f"\n✓ All {summary.passed} tests passed")))

    failed_tests = [r for r in summary.results if r.outcome == "Failed"]
    if failed_tests:
        print(RED("  Failed:"))
        for r in failed_tests:
            print(RED(f"    • {r.name}"))

    skipped_tests = [r for r in summary.results if r.outcome in ("NotExecuted", "NotRunnable")]
    if skipped_tests:
        print(YELLOW("  Skipped:"))
        for r in skipped_tests:
            # Try to extract reason from inner text
            reason = ""
            err_info = r  # We don't have the reason in the flat model; keep it simple
            print(YELLOW(f"    • {r.name}"))


# ---------------------------------------------------------------------------
# Building the dotnet test command
# ---------------------------------------------------------------------------

def build_cmd(
    *,
    headed: bool = False,
    filter_expr: str | None = None,
    group: str | None = None,
    all_tests: bool = False,
    list_only: bool = False,
    extra_args: list[str] | None = None,
) -> tuple[list[str], dict[str, str]]:
    """Return (command_args, env_overrides) for ``dotnet test``."""
    cmd = ["dotnet", "test", str(SOLUTION)]

    # Build filter expression
    parts: list[str] = []
    if not all_tests:
        parts.append(f"TestType={TRAIT_AUTOMATED}")

    if group:
        parts.append(f"FullyQualifiedName~{group}")

    if filter_expr:
        parts.append(filter_expr)

    if parts:
        combined = "&".join(parts)
        cmd += ["--filter", combined]

    if list_only:
        cmd += ["--list-tests"]
    else:
        # Add TRX logger for parsing results
        cmd += [
            "--logger", "trx;LogFileName=results.trx",
            "--results-directory", str(RESULTS_DIR),
        ]

    cmd += (extra_args or [])

    env: dict[str, str] = {}
    if not headed:
        env["SDL_VIDEODRIVER"] = "dummy"

    return cmd, env


# ---------------------------------------------------------------------------
# Finding / searching tests
# ---------------------------------------------------------------------------

def list_tests(
    *,
    headed: bool = False,
    filter_expr: str | None = None,
    group: str | None = None,
    all_tests: bool = False,
) -> list[str]:
    """Run ``dotnet test --list-tests`` and return discovered test names."""
    cmd, env = build_cmd(
        headed=headed,
        filter_expr=filter_expr,
        group=group,
        all_tests=all_tests,
        list_only=True,
    )
    merged = {**os.environ, **env}
    proc = subprocess.run(cmd, capture_output=True, text=True, env=merged)
    tests: list[str] = []
    in_list = False
    for line in proc.stdout.splitlines():
        if "The following Tests are available:" in line or "The following Tests are available" in line:
            in_list = True
            continue
        if in_list:
            stripped = line.strip()
            if stripped and not stripped.startswith("===") and not stripped.startswith("---"):
                tests.append(stripped)
            elif not stripped and in_list and tests:
                break  # blank line after list = done
    return tests


def find_tests(pattern: str, *, all_tests: bool = False) -> list[str]:
    """List tests whose fully-qualified name contains *pattern* (case-insensitive)."""
    tests = list_tests(all_tests=all_tests)
    return [t for t in tests if pattern.lower() in t.lower()]


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="NightEngine test runner — wraps dotnet test with smart defaults.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""\
examples:
  %(prog)s                          run headless automated tests (default)
  %(prog)s --headed                  run with real SDL video driver
  %(prog)s --all                     include manual/headed tests
  %(prog)s --filter "FullyQualifiedName~Filesystem"  dotnet filter expression
  %(prog)s --group Timer             run Timer group
  %(prog)s --find Timer              list tests matching 'Timer'
  %(prog)s --find "Read"             list tests matching 'Read'
  %(prog)s --failures-only           re-run only previously failed tests
  %(prog)s --dry-run                 show command without executing
""",
    )

    parser.add_argument(
        "--headed",
        action="store_true",
        default=False,
        help="Run with the real SDL video driver (no SDL_VIDEODRIVER=dummy). "
             "Needed for manual/visual tests on a headed display.",
    )
    parser.add_argument(
        "--all",
        action="store_true",
        default=False,
        help="Run all tests including manual ones (no TestType filter).",
    )

    filter_group = parser.add_mutually_exclusive_group()
    filter_group.add_argument(
        "--filter",
        dest="filter_expr",
        metavar="EXPR",
        help="Additional dotnet test filter expression (e.g. 'FullyQualifiedName~Timer').",
    )
    filter_group.add_argument(
        "--group",
        metavar="NAME",
        help="Run a single test group by name fragment (e.g. Filesystem, Timer).",
    )
    filter_group.add_argument(
        "--find",
        metavar="PATTERN",
        help="List tests whose name contains PATTERN, then exit.",
    )

    parser.add_argument(
        "--failures-only",
        action="store_true",
        default=False,
        help="Re-run only tests that failed in the last run.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        default=False,
        help="Print the dotnet test command without executing.",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        default=False,
        help="Pass --verbosity normal to dotnet test.",
    )
    parser.add_argument(
        "--no-build",
        action="store_true",
        default=False,
        help="Skip the build step.",
    )
    parser.add_argument(
        "extra",
        nargs="*",
        help="Extra arguments forwarded to dotnet test.",
    )

    args = parser.parse_args(argv)

    # --find mode: just list and exit
    if args.find:
        matches = find_tests(args.find, all_tests=args.all)
        if not matches:
            print(YELLOW(f"No tests matching '{args.find}'"))
            return EXIT_OK
        print(CYAN(f"Tests matching '{args.find}' ({len(matches)}):"))
        for t in matches:
            print(f"  {t}")
        return EXIT_OK

    # --failures-only: read last failures and build filter
    if args.failures_only:
        if not FAILURES_FILE.exists():
            print(RED("No .last-test-failures file found. Run tests first."))
            return EXIT_FAIL
        failures = [l.strip() for l in FAILURES_FILE.read_text().splitlines() if l.strip()]
        if not failures:
            print(GREEN("No previously failed tests recorded."))
            return EXIT_OK
        name_filters = "|".join(f"FullyQualifiedName={f}" for f in failures)
        args.filter_expr = name_filters

    cmd, env = build_cmd(
        headed=args.headed,
        filter_expr=args.filter_expr,
        group=args.group,
        all_tests=args.all,
        extra_args=args.extra,
    )

    if args.verbose:
        cmd += ["--verbosity", "normal"]
    if args.no_build:
        cmd += ["--no-build"]

    merged_env = {**os.environ, **env}

    print(CYAN("Command: ") + " ".join(cmd))
    if env:
        env_str = " ".join(f"{k}={v}" for k, v in env.items())
        print(CYAN("Env:     ") + env_str)
    print()

    if args.dry_run:
        return EXIT_OK

    # Ensure build is current (unless --no-build)
    if not args.no_build:
        print(BOLD("Building..."))
        build_proc = subprocess.run(
            ["dotnet", "build", str(SOLUTION)],
            env=merged_env,
            capture_output=True,
            text=True,
        )
        if build_proc.returncode != 0:
            print(RED("Build failed:"))
            print(build_proc.stdout)
            print(build_proc.stderr)
            return EXIT_FAIL
        print(GREEN("Build succeeded.") + "\n")

    # Clean previous results
    if RESULTS_DIR.exists():
        shutil.rmtree(RESULTS_DIR)
    RESULTS_DIR.mkdir(exist_ok=True)

    # Run tests (stdout goes to terminal for live output)
    print(BOLD("Running tests...") + "\n")
    proc = subprocess.run(cmd, env=merged_env)

    # Parse TRX results
    trx_file = RESULTS_DIR / "results.trx"
    if trx_file.exists():
        summary = parse_trx(trx_file)
    else:
        # Fallback: use exit code
        if proc.returncode == 0:
            summary = RunSummary(total=1, passed=1, failed=0, skipped=0, results=[])
        else:
            summary = RunSummary(total=1, passed=0, failed=1, skipped=0, results=[])

    print_summary(summary)

    # Save failures for --failures-only
    failed_names = [r.name for r in summary.results if r.outcome == "Failed"]
    if failed_names:
        FAILURES_FILE.write_text("\n".join(failed_names) + "\n")
    else:
        # Write empty file so --failures-only knows we ran
        FAILURES_FILE.write_text("")

    if summary.failed > 0:
        return EXIT_FAIL
    if summary.skipped > 0:
        return EXIT_PARTIAL
    return EXIT_OK


if __name__ == "__main__":
    sys.exit(main())