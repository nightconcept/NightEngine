--- Cue playback with duplicate suppression and bounded interruptible channels.
local M = {}
function M.new(names, layers, directory)
    local A = {}
    local NAMES, LAYERS = names, layers or 4
    local directory = directory or "assets/sfx"
    local sounds, channels = {}, {}

    function A.setup()
        for _, name in ipairs(NAMES) do
            sounds[name] = love.audio.newSource(directory .. "/" .. name .. ".wav", "static")
        end
    end

    function A.play(cues)
        local seen, layer = {}, 0
        for _, cue in ipairs(cues) do
            if sounds[cue] and not seen[cue] and layer < LAYERS then
                seen[cue] = true
                layer = layer + 1
                if channels[layer] then
                    channels[layer]:stop()
                end
                channels[layer] = sounds[cue]:clone()
                channels[layer]:play()
            end
        end
    end

    return A
end
return M
