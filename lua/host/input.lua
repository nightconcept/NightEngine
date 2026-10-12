--- Keys, gamepads, touch, and the mouse, read once a frame into the game's input: one int and a list of pointers.
local bit = require("bit")
local bor = bit.bor
local inputs = require("nightengine.inputs")
local pointer = require("nightengine.pointer")
local M = {}
function M.new(config)
    local I = {}
    local KEYS, PAD_MENU, PAD_RIDER = config.keys or {}, config.pad_menu or {}, config.pad_players or {}
    local DEADZONE = config.deadzone or 0.4
    local UP, DOWN, LEFT, RIGHT = inputs.UP, inputs.DOWN, inputs.LEFT, inputs.RIGHT
    I.is_down = love.keyboard.isDown -- LovePilot swaps in its own, which adds the keys it injects.
    local pulse = 0 -- Buttons pressed for one frame by a key event (the Android Back button is Escape).

    function I.setup()
        if love.filesystem.getInfo("assets/gamecontrollerdb.txt") then
            pcall(love.joystick.loadGamepadMappings, "assets/gamecontrollerdb.txt")
        end
    end

    function I.keypressed(key)
        pulse = bor(pulse, (config.pulses or {})[key] or 0)
    end

    local function pads()
        local code = 0
        local list = {}
        for _, j in ipairs(love.joystick.getJoysticks()) do
            if j:isGamepad() then
                list[#list + 1] = j
            end
        end
        for n, j in ipairs(list) do
            if n == 1 then
                for _, b in ipairs(PAD_MENU) do
                    if j:isGamepadDown(b[2]) then
                        code = bor(code, b[1])
                    end
                end
                local x, y = j:getGamepadAxis("leftx"), j:getGamepadAxis("lefty")
                code = bor(code, x <= -DEADZONE and LEFT or x >= DEADZONE and RIGHT or 0)
                code = bor(code, y <= -DEADZONE and UP or y >= DEADZONE and DOWN or 0)
            end
            if PAD_RIDER[n] then
                for _, b in ipairs(PAD_RIDER[n]) do
                    if j:isGamepadDown(b[2]) then
                        code = bor(code, b[1])
                    end
                end
            end
        end
        return code
    end

    function I.buttons()
        local code = pulse
        pulse = 0
        for _, entry in ipairs(KEYS) do
            for _, key in ipairs(entry[2]) do
                if I.is_down(key) then
                    code = bor(code, entry[1])
                    break
                end
            end
        end
        return bor(code, pads())
    end

    -- Pointers -------------------------------------------------------------------------

    local contacts = {} -- touch id -> {id, x, y, start_x, start_y}
    local next_id = 1
    local mouse = nil

    --- Contacts this frame, in game pixels. `to_game(x, y)` maps window pixels. Touch ids count up from 1.
    function I.pointers(to_game, use_mouse)
        local out, seen = {}, {}
        for _, t in ipairs(love.touch.getTouches()) do
            local x, y = to_game(love.touch.getPosition(t))
            local c = contacts[t]
            seen[t] = true
            if not c then
                c = { id = next_id, x = x, y = y, start_x = x, start_y = y }
                next_id = next_id + 1
                contacts[t] = c
                out[#out + 1] = pointer.new(c.id, x, y, x, y, pointer.PRESSED)
            else
                local phase = (x == c.x and y == c.y) and pointer.HELD or pointer.MOVED
                c.x, c.y = x, y
                out[#out + 1] = pointer.new(c.id, x, y, c.start_x, c.start_y, phase)
            end
        end
        for t, c in pairs(contacts) do
            if not seen[t] then
                contacts[t] = nil
                out[#out + 1] = pointer.new(c.id, c.x, c.y, c.start_x, c.start_y, pointer.RELEASED)
            end
        end
        table.sort(out, function(a, b)
            return a.id < b.id
        end)
        if use_mouse then
            local down = love.mouse.isDown(1)
            local x, y = to_game(love.mouse.getPosition())
            if down and not mouse then
                mouse = { x = x, y = y, start_x = x, start_y = y }
                table.insert(out, 1, pointer.new(0, x, y, x, y, pointer.PRESSED, pointer.MOUSE))
            elseif mouse and not down then
                table.insert(
                    out,
                    1,
                    pointer.new(0, x, y, mouse.start_x, mouse.start_y, pointer.RELEASED, pointer.MOUSE)
                )
                mouse = nil
            elseif mouse then
                local phase = (x == mouse.x and y == mouse.y) and pointer.HELD or pointer.MOVED
                mouse.x, mouse.y = x, y
                table.insert(out, 1, pointer.new(0, x, y, mouse.start_x, mouse.start_y, phase, pointer.MOUSE))
            end
        end
        return out
    end

    return I
end
return M
