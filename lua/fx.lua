--- Screen effects that every game shares: shake and fade. Logic sets them; the renderer reads them.
local ScreenFx = {}
ScreenFx.__index = ScreenFx

function ScreenFx.new()
    return setmetatable({ shake = 0, fade = 0.0, fade_in = 0, fade_frames = 20 }, ScreenFx)
end

function ScreenFx:tick()
    if self.shake ~= 0 then
        self.shake = self.shake - 1
    end
    if self.fade_in ~= 0 then
        self.fade_in = self.fade_in - 1
        self.fade = self.fade_in / self.fade_frames
    end
end

return ScreenFx
