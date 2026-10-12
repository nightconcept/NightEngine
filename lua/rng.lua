--- The engine's random numbers: the same numbers as nightengine/rng.py (xoshiro128**, splitmix32 seeding,
-- FNV-1a for string seeds). Every step is a 32-bit operation done exactly in doubles.
local bit = require("bit")
local bxor, rshift, lshift, rol = bit.bxor, bit.rshift, bit.lshift, bit.rol

local TWO32 = 4294967296

--- A bit op result (signed) as an unsigned 32-bit number.
local function u(x)
    return x % TWO32
end

--- a * b mod 2^32, for a and b in [0, 2^32).
local function mul32(a, b)
    local al, ah = a % 65536, (a - a % 65536) / 65536
    return (al * b + ((ah * b) % 65536) * 65536) % TWO32
end

local function fnv1a(s)
    local h = 0x811C9DC5
    for i = 1, #s do
        h = mul32(u(bxor(h, s:byte(i))), 0x01000193)
    end
    return h
end

local function mix(z)
    z = mul32(u(bxor(z, rshift(z, 16))), 0x85EBCA6B)
    z = mul32(u(bxor(z, rshift(z, 13))), 0xC2B2AE35)
    return u(bxor(z, rshift(z, 16)))
end

local Rng = {}
Rng.__index = Rng
Rng.fnv1a, Rng.mul32 = fnv1a, mul32

--- `seed` is a number (any integer; only its low 32 bits count) or a string.
function Rng.new(seed)
    if type(seed) == "string" then
        seed = fnv1a(seed)
    end
    local x = (seed or 0) % TWO32
    local s = {}
    for i = 1, 4 do
        x = (x + 0x9E3779B9) % TWO32
        s[i] = mix(x)
    end
    if s[1] == 0 and s[2] == 0 and s[3] == 0 and s[4] == 0 then
        s[1] = 1
    end
    return setmetatable({ s = s }, Rng)
end

--- The next 32-bit number.
function Rng:next()
    local s = self.s
    local s0, s1, s2, s3 = s[1], s[2], s[3], s[4]
    local result = (u(rol((s1 * 5) % TWO32, 7)) * 9) % TWO32
    local t = u(lshift(s1, 9))
    s2 = u(bxor(s2, s0))
    s3 = u(bxor(s3, s1))
    s1 = u(bxor(s1, s2))
    s0 = u(bxor(s0, s3))
    s2 = u(bxor(s2, t))
    s3 = u(rol(s3, 11))
    s[1], s[2], s[3], s[4] = s0, s1, s2, s3
    return result
end

function Rng:getrandbits(k)
    assert(k >= 1 and k <= 32, "k must be 1 to 32")
    return math.floor(self:next() / 2 ^ (32 - k))
end

--- A number from `low` to `high`, both included.
function Rng:randint(low, high)
    local n = high - low + 1
    assert(n >= 1 and n <= TWO32, "empty or too wide range")
    local limit = TWO32 - TWO32 % n
    while true do
        local r = self:next()
        if r < limit then
            return low + r % n
        end
    end
end

function Rng:random()
    return self:next() / TWO32
end

return Rng
