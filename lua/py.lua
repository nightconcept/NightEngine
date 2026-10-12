--- Python's number rules, for code ported from the Pyxel side. Lua's own `//` and `%` differ or are missing.
local py = {}
local floor, ceil = math.floor, math.ceil

--- a // b
function py.idiv(a, b)
    return floor(a / b)
end

--- int(x): toward zero.
function py.int(x)
    if x >= 0 then
        return floor(x)
    end
    return ceil(x)
end

--- round(x): halves go to the even neighbour.
function py.round(x)
    local f = floor(x)
    local d = x - f
    if d > 0.5 or (d == 0.5 and f % 2 == 1) then
        return f + 1
    end
    return f
end

--- bool(a) - bool(b)
function py.sign(a, b)
    return (a and 1 or 0) - (b and 1 or 0)
end

function py.clamp(x, lo, hi)
    return math.max(lo, math.min(hi, x))
end

--- A shallow copy, like copy.copy.
function py.copy(t)
    local out = {}
    for k, v in pairs(t) do
        out[k] = v
    end
    return setmetatable(out, getmetatable(t))
end

--- Index of the first value >= x in a sorted array (1-based), like bisect_left + 1.
function py.bisect_left(list, x)
    local lo, hi = 1, #list + 1
    while lo < hi do
        local mid = floor((lo + hi) / 2)
        if list[mid] < x then
            lo = mid + 1
        else
            hi = mid
        end
    end
    return lo
end

function py.contains(list, x)
    for _, v in ipairs(list) do
        if v == x then
            return true
        end
    end
    return false
end

function py.count(list, x)
    local n = 0
    for _, v in ipairs(list) do
        if v == x then
            n = n + 1
        end
    end
    return n
end

return py
