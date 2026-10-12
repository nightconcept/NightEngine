--- Pointers: touch, mouse, and pen contacts as game input (see pointer.py). Plain records, recorded and replayed.
--
-- A contact goes PRESSED, then HELD or MOVED each frame, then RELEASED or CANCELLED. The mouse is id 0.
local M = {}

M.PRESSED, M.HELD, M.MOVED, M.RELEASED, M.CANCELLED = "PRESSED", "HELD", "MOVED", "RELEASED", "CANCELLED"
M.TOUCH, M.PEN, M.MOUSE = "touch", "pen", "mouse"

local PHASES = { P = M.PRESSED, H = M.HELD, M = M.MOVED, R = M.RELEASED, C = M.CANCELLED }
local KINDS = { t = M.TOUCH, p = M.PEN, m = M.MOUSE }

local Pointer = {}
Pointer.__index = Pointer
M.Pointer = Pointer

function M.new(id, x, y, start_x, start_y, phase, kind)
    return setmetatable(
        { id = id, x = x, y = y, start_x = start_x, start_y = start_y, phase = phase, kind = kind or M.TOUCH },
        Pointer
    )
end

--- True while the contact is down (not on its RELEASED or CANCELLED frame).
function Pointer:active()
    return self.phase ~= M.RELEASED and self.phase ~= M.CANCELLED
end

--- The oldest active contact, or nil.
function M.primary(pointers)
    for _, p in ipairs(pointers) do
        if p:active() then
            return p
        end
    end
end

--- A contact that started this frame, or nil.
function M.pressed(pointers)
    for _, p in ipairs(pointers) do
        if p.phase == M.PRESSED then
            return p
        end
    end
end

--- A contact that ended normally this frame, or nil. A cancelled contact is never a tap.
function M.released(pointers)
    for _, p in ipairs(pointers) do
        if p.phase == M.RELEASED then
            return p
        end
    end
end

--- True if an active contact is inside the rectangle.
function M.in_rect(pointers, x, y, w, h)
    for _, p in ipairs(pointers) do
        if p:active() and x <= p.x and p.x < x + w and y <= p.y and p.y < y + h then
            return true
        end
    end
    return false
end

--- A recording row: {id, x, y, start_x, start_y, phase letter}, then a kind letter if not touch.
function M.encode(p)
    local row = { p.id, p.x, p.y, p.start_x, p.start_y, p.phase:sub(1, 1) }
    if p.kind ~= M.TOUCH then
        row[7] = p.kind:sub(1, 1)
    end
    return row
end

function M.decode(row)
    return M.new(row[1], row[2], row[3], row[4], row[5], PHASES[row[6]], row[7] and KINDS[row[7]] or M.TOUCH)
end

return M
