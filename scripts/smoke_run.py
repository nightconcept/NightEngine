#!/usr/bin/env python3
"""NightEngine headless smoke run.

Launches the engine for a fixed number of frames using the offscreen SDL
driver and writes a structured JSON verdict.  The engine must exit with
code 0 AND have completed at least the requested number of frames for the
run to be considered passing.

On macOS the offscreen driver requires an OpenGL library.  If the engine
exits cleanly but no frames ran (loop_count == 0 or missing), the verdict
is ``passed: false`` with an explanatory ``error`` field.

Usage:
    python scripts/smoke_run.py [options]

Options:
    --project PATH   Path to .csproj to run
                     (default: src/SampleGame/SampleGame.csproj)
    --frames N       Frame limit to pass to the engine (default: 60)
    --timeout S      Wall-clock kill timeout in seconds (default: 30)
    --out PATH       Write JSON verdict to PATH instead of stdout
    --no-build       Skip dotnet build before running
"""

from __future__ import annotations

import argparse
import json
import os
import re
import signal
import subprocess
import sys
from pathlib import Path
from time import monotonic

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

ROOT = Path(__file__).resolve().parent.parent
DEFAULT_PROJECT = ROOT / "src" / "NightFrame.Sample" / "NightFrame.Sample.csproj"
LOOP_COUNT_RE = re.compile(r"Main loop ended.*LoopCount:\s*(\d+)", re.IGNORECASE)

# ANSI helpers (disabled when not a tty)
_USE_COLOR = sys.stdout.isatty()


def _c(code: int, text: str) -> str:
    return f"\033[{code}m{text}\033[0m" if _USE_COLOR else text


RED   = lambda t: _c(31, t)
GREEN = lambda t: _c(32, t)
CYAN  = lambda t: _c(36, t)

# ---------------------------------------------------------------------------
# Core
# ---------------------------------------------------------------------------

def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--project", default=str(DEFAULT_PROJECT), help="Path to .csproj")
    p.add_argument("--frames", type=int, default=60, help="Frame limit (default: 60)")
    p.add_argument("--timeout", type=int, default=30, help="Kill timeout in seconds (default: 30)")
    p.add_argument("--out", default=None, help="Write JSON verdict to this file (default: stdout)")
    p.add_argument("--no-build", action="store_true", help="Skip dotnet build step")
    return p.parse_args()


def build(project: str) -> bool:
    """Build the project. Returns True on success."""
    print(f"{CYAN('→')} Building {Path(project).name}...")
    result = subprocess.run(
        ["dotnet", "build", project, "--nologo", "-v", "q"],
        cwd=ROOT,
    )
    return result.returncode == 0


def run_engine(project: str, frames: int, timeout: int) -> tuple[int, bool, list[str]]:
    """
    Launch the engine with --frame-limit and offscreen SDL driver.

    Returns (exit_code, timed_out, output_lines).
    """
    env = {**os.environ,
           "SDL_VIDEODRIVER": "offscreen",
           "SDL_RENDER_DRIVER": "software"}

    cmd = [
        "dotnet", "run", "--project", project, "--no-build",
        "--", "--frame-limit", str(frames), "--debug",
    ]

    print(f"{CYAN('→')} Running engine: {' '.join(cmd[3:])}")
    print(f"   SDL_VIDEODRIVER=offscreen SDL_RENDER_DRIVER=software")

    timed_out = False
    lines: list[str] = []

    try:
        proc = subprocess.Popen(
            cmd,
            cwd=ROOT,
            env=env,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
        )

        try:
            stdout, _ = proc.communicate(timeout=timeout)
            lines = stdout.splitlines()
        except subprocess.TimeoutExpired:
            timed_out = True
            proc.send_signal(signal.SIGTERM)
            try:
                stdout, _ = proc.communicate(timeout=5)
                lines = stdout.splitlines()
            except subprocess.TimeoutExpired:
                proc.kill()
                stdout, _ = proc.communicate()
                lines = stdout.splitlines()

        exit_code = proc.returncode if not timed_out else -1

    except FileNotFoundError as exc:
        return -1, False, [f"Launch failed: {exc}"]

    return exit_code, timed_out, lines


def extract_loop_count(lines: list[str]) -> int | None:
    """Scan output lines for the 'Main loop ended' log entry and return LoopCount."""
    for line in reversed(lines):
        m = LOOP_COUNT_RE.search(line)
        if m:
            return int(m.group(1))
    return None


def main() -> int:
    args = parse_args()
    overall_start = monotonic()

    # Build step
    if not args.no_build:
        if not build(args.project):
            verdict = {
                "passed": False,
                "exit_code": 1,
                "timed_out": False,
                "frames_requested": args.frames,
                "loop_count": None,
                "duration_s": round(monotonic() - overall_start, 2),
                "log_tail": [],
                "error": "Build failed before engine launch.",
            }
            _write_verdict(verdict, args.out)
            return 1

    exit_code, timed_out, lines = run_engine(args.project, args.frames, args.timeout)
    duration = round(monotonic() - overall_start, 2)

    loop_count = extract_loop_count(lines)
    log_tail = lines[-20:] if len(lines) > 20 else lines

    # Determine pass/fail:
    # - Must exit cleanly (code 0)
    # - Must not have timed out
    # - Must have completed at least the requested frames
    error: str | None = None
    if timed_out:
        error = f"Engine did not exit within {args.timeout}s timeout."
    elif exit_code != 0:
        error = f"Engine exited with code {exit_code}."
    elif loop_count is None:
        error = (
            "Engine exited cleanly but 'Main loop ended' was not found in output. "
            "The game loop may not have run. On macOS this often indicates the "
            "offscreen SDL driver failed to initialize (OpenGL not available)."
        )
    elif loop_count < args.frames:
        error = (
            f"Engine ran only {loop_count} frames, expected {args.frames}. "
            "The loop may have exited early due to a rendering or initialization error."
        )

    passed = error is None

    verdict = {
        "passed": passed,
        "exit_code": exit_code,
        "timed_out": timed_out,
        "frames_requested": args.frames,
        "loop_count": loop_count,
        "duration_s": duration,
        "log_tail": log_tail,
        "error": error,
    }

    _write_verdict(verdict, args.out)

    if passed:
        print(GREEN(f"✓ Smoke run passed: {loop_count} frames in {duration}s"))
    else:
        print(RED(f"✗ Smoke run FAILED: {error}"))

    return 0 if passed else 1


def _write_verdict(verdict: dict, out: str | None) -> None:
    payload = json.dumps(verdict, indent=2)
    if out:
        Path(out).write_text(payload)
        print(f"Verdict written to {out}")
    else:
        print(payload)


if __name__ == "__main__":
    sys.exit(main())
