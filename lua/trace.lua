--- Per-frame state traces, the same as trace.py: canonical text of a snapshot, and its CRC-32.
--
-- A snapshot is nested arrays of numbers, strings, booleans, and `trace.null` (Python's None).
local bit = require("bit")
local bxor, band, rshift = bit.bxor, bit.band, bit.rshift

local M = {}
M.null = setmetatable({}, {
    __tostring = function()
        return "null"
    end,
})

local function canon(v, out)
    local t = type(v)
    if v == M.null or v == nil then
        out[#out + 1] = "null"
    elseif t == "boolean" then
        out[#out + 1] = v and "true" or "false"
    elseif t == "number" then
        out[#out + 1] = v == 0 and "0" or string.format("%.17g", v) -- -0 is written as 0.
    elseif t == "string" then
        out[#out + 1] = '"' .. v:gsub("\\", "\\\\"):gsub('"', '\\"') .. '"'
    elseif t == "table" then
        out[#out + 1] = "["
        for i = 1, #v do
            if i > 1 then
                out[#out + 1] = ","
            end
            canon(v[i], out)
        end
        out[#out + 1] = "]"
    else
        error("a snapshot holds arrays, numbers, strings, booleans, and trace.null, not " .. t)
    end
end

function M.canon(v)
    local out = {}
    canon(v, out)
    return table.concat(out)
end

local CRC = {}
for i = 0, 255 do
    local c = i
    for _ = 1, 8 do
        if band(c, 1) ~= 0 then
            c = bxor(rshift(c, 1), 0xEDB88320)
        else
            c = rshift(c, 1)
        end
    end
    CRC[i] = c
end

--- The CRC-32 of a string (zlib.crc32).
function M.crc32(s)
    local c = 0xFFFFFFFF
    for i = 1, #s do
        c = bxor(rshift(c, 8), CRC[band(bxor(c, s:byte(i)), 0xFF)])
    end
    return bxor(c, 0xFFFFFFFF) % 4294967296
end

function M.digest(v)
    return string.format("%08x", M.crc32(M.canon(v)))
end

--- Step `game` through a recording and hash `snapshot(game)` before each frame and after the last.
function M.trace(game, recording, snapshot)
    local hashes = { M.digest(snapshot(game)) }
    for i, code in ipairs(recording.frames) do
        game:step(code, recording:at(i - 1))
        hashes[#hashes + 1] = M.digest(snapshot(game))
    end
    return hashes
end

function M.parse(text)
    local hashes = {}
    for h in text:gmatch("%S+") do
        hashes[#hashes + 1] = h
    end
    return hashes
end

--- The first frame (from 0) where two traces differ, or nil.
function M.first_difference(a, b)
    for i = 1, math.min(#a, #b) do
        if a[i] ~= b[i] then
            return i - 1
        end
    end
    if #a ~= #b then
        return math.min(#a, #b)
    end
end

return M
