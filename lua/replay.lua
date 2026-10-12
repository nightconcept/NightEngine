--- Record a run as seed plus per-frame input, and replay it exactly. The same JSON as replay.py.
local json = require("nightengine.json")
local pointer = require("nightengine.pointer")
local target = require("nightengine.target")

local M = {}

function M.digits_for(mask)
    local bits = 0
    while mask >= 2 ^ bits do
        bits = bits + 1
    end
    return math.max(2, math.floor((bits + 3) / 4))
end

local Recording = {}
Recording.__index = Recording
M.Recording = Recording

--- `frames` is a list of codes. `pointers` maps a frame number (from 0) to a list of contacts.
function M.new(seed, frames, pointers, width, target_, events)
    return setmetatable({
        seed = seed,
        frames = frames or {},
        pointers = pointers or {},
        width = width or 2,
        target = target_,
        events = events or {},
    }, Recording)
end

local EMPTY = {}

--- The contacts of frame `i` (from 0).
function Recording:at(i)
    return self.pointers[i] or EMPTY
end

function Recording:events_at(i)
    return self.events[i] or EMPTY
end

function Recording:add(code, pointers, events)
    if events and #events > 0 then
        self.events[#self.frames] = events
    end
    if pointers and #pointers > 0 then
        self.pointers[#self.frames] = pointers
    end
    self.frames[#self.frames + 1] = code
end

function Recording:len()
    return #self.frames
end

--- The file text, byte for byte like Recording.to_json in Python.
function Recording:to_json()
    local digits = {}
    local fmt = "%0" .. self.width .. "x"
    for i, code in ipairs(self.frames) do
        digits[i] = string.format(fmt, code)
    end
    local parts = { '{"seed":' .. json.encode(self.seed), '"inputs":"' .. table.concat(digits) .. '"' }
    if self.width ~= 2 then
        parts[#parts + 1] = '"width":' .. self.width
    end
    local frames = {}
    for f in pairs(self.pointers) do
        frames[#frames + 1] = f
    end
    if #frames > 0 then
        table.sort(frames)
        local rows = {}
        for i, f in ipairs(frames) do
            local encoded = {}
            for j, p in ipairs(self.pointers[f]) do
                encoded[j] = json.encode(pointer.encode(p))
            end
            rows[i] = '"' .. f .. '":[' .. table.concat(encoded, ",") .. "]"
        end
        parts[#parts + 1] = '"pointers":{' .. table.concat(rows, ",") .. "}"
    end
    local event_frames = {}
    for f in pairs(self.events) do
        event_frames[#event_frames + 1] = f
    end
    if #event_frames > 0 then
        table.sort(event_frames)
        local rows = {}
        for i, f in ipairs(event_frames) do
            rows[i] = '"' .. f .. '":' .. json.encode(self.events[f])
        end
        parts[#parts + 1] = '"events":{' .. table.concat(rows, ",") .. "}"
    end
    if self.target then
        local t = self.target
        local controls = {}
        for i, c in ipairs(t:control_list()) do
            controls[i] = json.encode(c)
        end
        parts[#parts + 1] = string.format(
            '"target":{"name":%s,"width":%d,"height":%d,"controls":[%s]}',
            json.encode(t.name),
            t.width,
            t.height,
            table.concat(controls, ",")
        )
    end
    return table.concat(parts, ",") .. "}"
end

function M.from_json(text)
    local data = json.decode(text)
    local digits, width = data.inputs, data.width or 2
    local frames = {}
    for i = 1, #digits, width do
        frames[#frames + 1] = tonumber(digits:sub(i, i + width - 1), 16)
    end
    local pointers = {}
    for f, rows in pairs(data.pointers or {}) do
        local list = {}
        for j, row in ipairs(rows) do
            list[j] = pointer.decode(row)
        end
        pointers[tonumber(f)] = list
    end
    local t = data.target and target.from_dict(data.target) or nil
    local events = {}
    for f, es in pairs(data.events or {}) do
        events[tonumber(f)] = es
    end
    return M.new(data.seed, frames, pointers, width, t, events)
end

--- Run a recording headless and return the game. `make_game(seed, target)` builds it.
function M.play(make_game, recording, frames)
    local game = make_game(recording.seed, recording.target)
    for i, code in ipairs(recording.frames) do
        if frames and i > frames then
            break
        end
        game:step(code, recording:at(i - 1), recording:events_at(i - 1))
    end
    return game
end

return M
