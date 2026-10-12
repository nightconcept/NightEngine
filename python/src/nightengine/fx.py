"""Screen effects that every game shares: shake and fade. Logic sets them; the renderer reads them."""

from dataclasses import dataclass


@dataclass
class ScreenFx:
    shake: int = 0  # Frames of shake left.
    fade: float = 0.0  # 0 clear .. 1 black, drawn over everything.
    fade_in: int = 0  # Frames left of a fade back from black.
    fade_frames: int = 20  # Length of that fade.

    def tick(self):
        if self.shake:
            self.shake -= 1
        if self.fade_in:
            self.fade_in -= 1
            self.fade = self.fade_in / self.fade_frames
