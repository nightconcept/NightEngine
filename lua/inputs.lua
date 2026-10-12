--- Abstract buttons. One frame of input is one small int, so runs can be recorded and replayed exactly.
local bit = require("bit")
local band, bnot, bor = bit.band, bit.bnot, bit.bor

local M = {}
M.UP, M.DOWN, M.LEFT, M.RIGHT, M.A, M.B, M.C, M.MENU = 1, 2, 4, 8, 16, 32, 64, 128
M.ALL = 255
M.REPEAT_DELAY, M.REPEAT_RATE = 14, 5

local DIRECTIONS = { { M.UP, 0, -1 }, { M.DOWN, 0, 1 }, { M.LEFT, -1, 0 }, { M.RIGHT, 1, 0 } }

--- Buttons for one frame: `held`, `pressed` on this frame, `rep` (repeat) for menu cursors, and `pointers`.
local Input = {}
Input.__index = Input
M.Input = Input

function M.input(held, pressed, rep, pointers, events)
    return setmetatable(
        { held = held or 0, pressed = pressed or 0, rep = rep or 0, pointers = pointers or {}, events = events or {} },
        Input
    )
end

function Input:down(button)
    return band(self.held, button) ~= 0
end

function Input:hit(button)
    return band(self.pressed, button) ~= 0
end

function Input:nav(button)
    return band(self.rep, button) ~= 0
end

function Input:axis()
    local function b(x)
        return band(self.held, x) ~= 0 and 1 or 0
    end
    return b(M.RIGHT) - b(M.LEFT), b(M.DOWN) - b(M.UP)
end

function Input:direction()
    for _, d in ipairs(DIRECTIONS) do
        if band(self.pressed, d[1]) ~= 0 then
            return d[2], d[3]
        end
    end
    for _, d in ipairs(DIRECTIONS) do
        if band(self.held, d[1]) ~= 0 then
            return d[2], d[3]
        end
    end
end

--- Turns raw held codes into Input objects with press edges and optional auto-repeat.
local InputTracker = {}
InputTracker.__index = InputTracker
M.InputTracker = InputTracker

function InputTracker.new(mask, rep, delay, rate)
    local t = setmetatable({}, InputTracker)
    t.mask = mask or M.ALL
    t.delay, t.rate = delay or M.REPEAT_DELAY, rate or M.REPEAT_RATE
    t.previous = 0
    t.buttons, t.held_for = {}, {}
    for i, b in ipairs(rep or {}) do
        t.buttons[i], t.held_for[i] = b, 0
    end
    return t
end

function InputTracker:feed(code, pointers, events)
    code = band(code, self.mask)
    local pressed = band(code, bnot(self.previous))
    local rep = pressed
    for i, button in ipairs(self.buttons) do
        if band(code, button) ~= 0 then
            self.held_for[i] = self.held_for[i] + 1
            local t = self.held_for[i] - self.delay
            if t >= 0 and t % self.rate == 0 then
                rep = bor(rep, button)
            end
        else
            self.held_for[i] = 0
        end
    end
    self.previous = code
    return M.input(code, pressed, rep, pointers, events)
end

return M
