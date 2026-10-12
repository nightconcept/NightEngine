"""Plays sound effects and music from a game's SFX and MUSIC tables. Music uses channels 0-2, effects channel 3.

Logic code emits cue names (`Game.step` returns them); a cue is one entry in the SFX table.
A track is chosen by name from `Scene.music()`; a track is one entry in the MUSIC table.
Tables look like this (see `nightengine.sound.ch` and `note_errors`):

    SFX   = {name: (notes, tone, volume, effect, speed)}   in priority order, highest first
    MUSIC = {name: [ch(...), ch(...)]}                     one entry per channel
"""

from collections.abc import Collection, Mapping, Sequence

import pyxel


class AudioManager:
    """`priority=True`: of the cues in a frame play the one earliest in SFX, unless a more important effect is still
    playing. `priority=False`: play the first cue of the frame if it is known, and never check what is playing.
    `layers` (with `priority=False`) plays up to that many different known cues of a frame together, the first on
    `sfx_channel` and each next one on the channel below it. Use it in a game without music."""

    def __init__(
        self,
        sfx: Mapping[str, Sequence],
        music: Mapping[str, Sequence[Sequence]],
        once: Collection[str] = (),
        sfx_channel: int = 3,
        priority: bool = True,
        layers: int = 1,
    ):
        if layers > 1 and (priority or layers > sfx_channel + 1):
            raise ValueError("layers needs priority=False and a free channel for each layer")
        self.sfx, self.music, self.once = sfx, music, once
        self.sfx_channel, self.priority, self.layers = sfx_channel, priority, layers
        self.sfx_ids: dict[str, int] = {}
        self.music_ids: dict[str, int] = {}
        self.rank = {name: i for i, name in enumerate(sfx)}
        self.track: str | None = None
        self.playing: str | None = None  # The effect last started on the effect channel.
        self.gains: list[float] | None = None  # Each channel's gain after setup: what volume 1 means.

    def setup(self):
        """Write every effect and music channel into pyxel's sound slots."""
        self.gains = [c.gain for c in pyxel.channels]
        index = 0
        for name, (notes, tone, vol, fx, speed) in self.sfx.items():
            pyxel.sounds[index].set(notes, tone, vol, fx, speed)
            self.sfx_ids[name] = index
            index += 1
        for m, (name, channels) in enumerate(self.music.items()):
            ids = []
            for notes, tone, vol, fx, speed in channels:
                pyxel.sounds[index].set(notes.replace(" ", ""), tone, vol, fx, speed)
                ids.append(index)
                index += 1
            pyxel.musics[m].set(*[[i] for i in ids])
            self.music_ids[name] = m

    def effect_channels(self) -> range:
        """The channels that play effects: `sfx_channel`, and the ones below it that `layers` uses."""
        return range(self.sfx_channel - self.layers + 1, self.sfx_channel + 1)

    def set_volume(self, music: float, sfx: float):
        """Set the music and effect volumes, each 0 to 1. 1 is the gain each channel had after `setup()` (pyxel's
        default is 0.125). The effect channels take `sfx`; every other channel takes `music`."""
        if self.gains is None:
            self.gains = [c.gain for c in pyxel.channels]
        effects = self.effect_channels()
        for i, channel in enumerate(pyxel.channels):
            if i < len(self.gains):
                channel.gain = self.gains[i] * max(0.0, min(1.0, sfx if i in effects else music))

    def update(self, track: str | None, cues: list[str]):
        """Call once per frame with the current track and the cues the game returned."""
        if track != self.track:
            self.track = track
            for ch in range(self.sfx_channel):
                pyxel.stop(ch)
            if track in self.music_ids:
                pyxel.playm(self.music_ids[track], loop=track not in self.once)
        if self.priority:
            self.play_by_priority(cues)
        elif self.layers == 1:
            for cue in cues[:1]:
                if cue in self.sfx_ids:
                    pyxel.play(self.sfx_channel, self.sfx_ids[cue])
        else:
            known = [c for c in dict.fromkeys(cues) if c in self.sfx_ids]
            for layer, cue in enumerate(known[: self.layers]):
                pyxel.play(self.sfx_channel - layer, self.sfx_ids[cue])

    def play_by_priority(self, cues: list[str]):
        cues = [c for c in cues if c in self.sfx_ids]
        if not cues:
            return
        cue = min(cues, key=self.rank.__getitem__)
        busy = self.playing and pyxel.play_pos(self.sfx_channel) is not None
        if busy and self.rank[cue] > self.rank[self.playing]:
            return
        pyxel.play(self.sfx_channel, self.sfx_ids[cue])
        self.playing = cue
