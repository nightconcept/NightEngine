#!/usr/bin/env python3
"""NightEngine quality gate runner.

Runs each gate stage as a subprocess, stops on first failure, and writes a
machine-readable verdict to ``test-results/gate.json``.

Exit codes:
    0  all stages passed
    1  one or more stages failed
"""

from __future__ import annotations

import json
import subprocess
import sys
from datetime import datetime
from pathlib import Path
from time import monotonic

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

ROOT = Path(__file__).resolve().parent.parent
RESULTS_DIR = ROOT / "test-results"
VERDICT_FILE = RESULTS_DIR / "gate.json"

STAGES = [
    ("setup",  "dotnet tool restore"),
    ("clean",  "dotnet clean Night.slnx"),
    ("format", "dotnet format --verbosity diagnostic Night.slnx"),
    ("build",  "dotnet build Night.slnx"),
    ("test",   "python scripts/run_tests.py"),
    ("docs",   "dotnet docfx docs/docfx.json"),
    ("api-doc","python scripts/update_api_doc.py"),
]

# ANSI helpers (disabled when not a tty)
_USE_COLOR = sys.stdout.isatty()


def _c(code: int, text: str) -> str:
    return f"\033[{code}m{text}\033[0m" if _USE_COLOR else text


RED    = lambda t: _c(31, t)
GREEN  = lambda t: _c(32, t)
YELLOW = lambda t: _c(33, t)
CYAN   = lambda t: _c(36, t)

# ---------------------------------------------------------------------------
# Runner
# ---------------------------------------------------------------------------

def run_stage(name: str, command: str) -> dict:
    """Run a single gate stage and return its result dict."""
    print(f"  {CYAN('→')} {name}: {command}")
    start = monotonic()
    result = subprocess.run(
        command,
        shell=True,
        cwd=ROOT,
    )
    duration = round(monotonic() - start, 2)
    passed = result.returncode == 0
    marker = GREEN("✓") if passed else RED("✗")
    print(f"  {marker} {name} {'passed' if passed else 'FAILED'} ({duration}s)")
    return {
        "name": name,
        "command": command,
        "exit_code": result.returncode,
        "passed": passed,
        "duration_s": duration,
    }


def main() -> int:
    print(CYAN("=== NightEngine Gate ==="))
    overall_start = monotonic()
    timestamp = datetime.now().astimezone().isoformat()

    RESULTS_DIR.mkdir(exist_ok=True)

    completed_stages: list[dict] = []
    first_failure: str | None = None

    for name, command in STAGES:
        stage_result = run_stage(name, command)
        completed_stages.append(stage_result)
        if not stage_result["passed"]:
            first_failure = name
            break

    total_duration = round(monotonic() - overall_start, 2)
    passed = first_failure is None

    verdict = {
        "passed": passed,
        "first_failure": first_failure,
        "timestamp": timestamp,
        "total_duration_s": total_duration,
        "stages": completed_stages,
    }

    VERDICT_FILE.write_text(json.dumps(verdict, indent=2))

    print()
    if passed:
        print(GREEN(f"Gate passed in {total_duration}s. Verdict: {VERDICT_FILE}"))
    else:
        print(RED(f"Gate FAILED at stage '{first_failure}' ({total_duration}s). Verdict: {VERDICT_FILE}"))

    return 0 if passed else 1


if __name__ == "__main__":
    sys.exit(main())
