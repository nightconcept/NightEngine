"""Display pacing: how many rule steps to run in each display frame, and where the picture is between two steps.

The rules always step at `step_hz`. With vsync the display sets the pace of the loop, at whatever rate the monitor
refreshes. `Pacer.frame` turns each display frame into a number of steps (often 0 or 1) and a blend value `alpha`.
`InputLatch` holds the input of the display frames that ran no step, so a short tap still reaches the next step.
Pure: no pyxel, so tests drive it with a fake clock.
"""

import math
import statistics
from collections import deque
from dataclasses import dataclass, field

from .pointer import ENDED, Pointer, PointerPhase

STALL = 0.25  # A display frame longer than this (a drag, a breakpoint) runs one step and starts over.


@dataclass
class Pacer:
    """Call `frame(now)` once for each display frame. The newest step is never behind the display: a step runs as
    soon as its time is due before the next refresh, and `alpha` (0 to 1) is how far the picture is from the step
    before it to the newest one. `alpha` is 1 on the frame that shows the newest step exactly."""

    step_hz: int = 60
    max_steps: int = 4
    lock_tolerance: float = 0.01  # Lock when the refresh is within 1% of a multiple of step_hz.
    window: int = 120  # Display frames used to measure the refresh (median).
    _intervals: deque = field(init=False, repr=False)
    _last: float | None = field(default=None, init=False, repr=False)
    _acc: float = field(default=0.0, init=False, repr=False)  # Steps past the newest step's due time (<= 0).
    _lock: int = field(default=0, init=False, repr=False)  # Display frames for each step while locked, else 0.
    _since: int = field(default=0, init=False, repr=False)  # Display frames since the last step, while locked.

    def __post_init__(self) -> None:
        self._intervals = deque(maxlen=self.window)

    def reset(self) -> None:
        """Start over: after a pause of the window, a load, or a vsync change."""
        self._intervals.clear()
        self._last, self._acc, self._lock, self._since = None, 0.0, 0, 0

    @property
    def refresh_hz(self) -> float | None:
        """The measured refresh (the median of the last `window` intervals), or None until there are that many."""
        if len(self._intervals) < self.window:
            return None
        median = statistics.median(self._intervals)
        return 1 / median if median > 0 else None

    @property
    def locked(self) -> bool:
        """True when the refresh is a whole multiple of step_hz (60, 120, 180, 240 Hz) and steps follow frames."""
        return self._lock > 0

    def frame(self, now: float) -> tuple[int, float]:
        """Call once for each display frame with time.perf_counter(). Returns (steps to run, alpha)."""
        if self._last is None:
            self._last = now
            return 1, 1.0
        dt, self._last = now - self._last, now
        if dt > STALL:
            self.reset()
            return self.frame(now)
        self._intervals.append(dt)
        self._acc += self._snap(dt * self.step_hz)
        lock, was = self._lock_frames(), self._lock
        self._lock = lock
        if lock:
            return self._locked(lock, entering=lock != was)
        steps = max(0, math.ceil(self._acc - 1e-9))
        if steps > self.max_steps:
            steps, self._acc = self.max_steps, 0.0
        else:
            self._acc -= steps
        return steps, min(1.0, max(0.0, 1 + self._acc))

    def _snap(self, steps: float) -> float:
        """A frame within lock_tolerance of a whole fraction of a step (1, 1/2, 1/3, 1/4, or whole steps) counts as
        exactly that, so clock jitter at 59.94 or 120 Hz never makes a double step and an empty frame."""
        for k in range(1, 5):
            whole = round(steps * k)
            if whole >= 1 and abs(steps * k - whole) <= self.lock_tolerance * whole:
                return whole / k
        return steps

    def _lock_frames(self) -> int:
        refresh = self.refresh_hz
        if refresh is None:
            return 0
        k = round(refresh / self.step_hz)
        if 1 <= k <= 4 and abs(refresh - k * self.step_hz) <= self.lock_tolerance * k * self.step_hz:
            return k
        return 0

    def _locked(self, k: int, entering: bool) -> tuple[int, float]:
        if not entering:
            self._since = (self._since + 1) % k
        elif self._acc > 0:  # A step is due: start the lock on it.
            self._since = 0
        else:  # Pick up where the accumulator left the picture.
            self._since = min(k - 1, max(1, round((1 + self._acc) * k) - 1))
        alpha = (self._since + 1) / k
        self._acc = alpha - 1  # So an unlock carries on from the same place.
        return (1 if self._since == 0 else 0), alpha


class InputLatch:
    """The input of every display frame since the last step, merged for the next step. Call `add` once for each
    display frame and `take` once for each frame that runs steps."""

    def __init__(self) -> None:
        self._code = 0
        self._samples: dict[int, list[Pointer]] = {}  # Pointer id -> its samples in order (dicts keep id order).
        self._events: list[str] = []

    def add(self, code: int, pointers: tuple[Pointer, ...], events: tuple[str, ...]) -> None:
        self._code |= code
        for p in pointers:
            self._samples.setdefault(p.id, []).append(p)
        self._events.extend(events)

    def take(self) -> tuple[int, tuple[Pointer, ...], tuple[str, ...]]:
        """The merged input since the last take: the OR of the codes, the merged pointers, and every event in
        order. Then it clears.

        A pointer gives its last sample, but a PRESSED sample wins, at its own position. A pointer that was PRESSED
        and then ended (RELEASED or CANCELLED) gives both, PRESSED first."""
        pointers: list[Pointer] = []
        for samples in self._samples.values():
            press = next((p for p in samples if p.phase is PointerPhase.PRESSED), None)
            last = samples[-1]
            if press is None:
                pointers.append(last)
            else:
                pointers.append(press)
                if last.phase in ENDED and samples.index(last) > samples.index(press):
                    pointers.append(last)
        merged = (self._code, tuple(pointers), tuple(self._events))
        self._code, self._samples, self._events = 0, {}, []
        return merged
