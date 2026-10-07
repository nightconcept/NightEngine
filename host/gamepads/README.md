# Gamepad mappings

`gamecontrollerdb.txt` is the community controller database from
[SDL_GameControllerDB](https://github.com/mdqinc/SDL_GameControllerDB) (commit `c1d5289`, 2026-10-02),
under the zlib license in `LICENSE`. SDL already knows Xbox, PlayStation, and Switch Pro pads.
This file adds about 2,000 more: arcade sticks, retro pads, adapters, and cheap clones.

`nightengine.host.gamepad.use_mappings()` points SDL's `SDL_GAMECONTROLLERCONFIG_FILE` hint at it before
`pyxel.init`, so SDL loads it when it starts the controller subsystem.

To update it, download the file again from the repo's `master` branch and change the commit above.
