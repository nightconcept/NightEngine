--- A small JSON reader and writer. Objects are written with sorted keys, so output is stable.
local json = {}

json.null = setmetatable({}, {
    __tostring = function()
        return "null"
    end,
})

local escapes =
    { ['"'] = '\\"', ["\\"] = "\\\\", ["\b"] = "\\b", ["\f"] = "\\f", ["\n"] = "\\n", ["\r"] = "\\r", ["\t"] = "\\t" }

local function quote(s)
    return '"'
        .. s:gsub('[%c"\\]', function(c)
            return escapes[c] or string.format("\\u%04x", c:byte())
        end)
        .. '"'
end

local function number(n)
    if n ~= n or n == math.huge or n == -math.huge then
        error("JSON has no NaN or infinity")
    end
    if n == math.floor(n) and math.abs(n) < 2 ^ 53 then
        return string.format("%d", n)
    end
    return string.format("%.17g", n)
end

--- Is `t` an array: keys 1..n with no gaps? An empty table is an array unless it has `json.object` as metatable.
local function is_array(t)
    local n = 0
    for _ in pairs(t) do
        n = n + 1
    end
    return n == #t
end

json.object = {}

function json.encode(v)
    local t = type(v)
    if v == nil or v == json.null then
        return "null"
    elseif t == "boolean" then
        return tostring(v)
    elseif t == "number" then
        return number(v)
    elseif t == "string" then
        return quote(v)
    elseif t == "table" then
        if getmetatable(v) ~= json.object and is_array(v) then
            local parts = {}
            for i = 1, #v do
                parts[i] = json.encode(v[i])
            end
            return "[" .. table.concat(parts, ",") .. "]"
        end
        local keys = {}
        for k in pairs(v) do
            keys[#keys + 1] = tostring(k)
        end
        table.sort(keys)
        local parts = {}
        for i, k in ipairs(keys) do
            local value = v[k]
            if value == nil then
                value = v[tonumber(k)]
            end
            parts[i] = quote(k) .. ":" .. json.encode(value)
        end
        return "{" .. table.concat(parts, ",") .. "}"
    end
    error("cannot write a " .. t .. " as JSON")
end

local function decoder(s)
    local pos = 1
    local value

    local function fail(what)
        error(string.format("bad JSON at %d: %s", pos, what), 0)
    end

    local function space()
        pos = s:find("[^ \t\r\n]", pos) or #s + 1
    end

    local function str()
        local out, i = {}, pos + 1
        while true do
            local j = s:find('["\\]', i)
            if not j then
                fail("unclosed string")
            end
            out[#out + 1] = s:sub(i, j - 1)
            if s:sub(j, j) == '"' then
                pos = j + 1
                return table.concat(out)
            end
            local e = s:sub(j + 1, j + 1)
            if e == "u" then
                local code = tonumber(s:sub(j + 2, j + 5), 16)
                if not code then
                    fail("bad \\u escape")
                end
                if code < 0x80 then
                    out[#out + 1] = string.char(code)
                elseif code < 0x800 then
                    out[#out + 1] = string.char(0xC0 + math.floor(code / 64), 0x80 + code % 64)
                else
                    out[#out + 1] =
                        string.char(0xE0 + math.floor(code / 4096), 0x80 + math.floor(code / 64) % 64, 0x80 + code % 64)
                end
                i = j + 6
            else
                local map = { b = "\b", f = "\f", n = "\n", r = "\r", t = "\t" }
                out[#out + 1] = map[e] or e
                i = j + 2
            end
        end
    end

    function value()
        space()
        local c = s:sub(pos, pos)
        if c == "{" then
            local obj = {}
            pos = pos + 1
            space()
            if s:sub(pos, pos) == "}" then
                pos = pos + 1
                return obj
            end
            while true do
                space()
                if s:sub(pos, pos) ~= '"' then
                    fail("expected a key")
                end
                local k = str()
                space()
                if s:sub(pos, pos) ~= ":" then
                    fail("expected :")
                end
                pos = pos + 1
                obj[k] = value()
                space()
                c = s:sub(pos, pos)
                pos = pos + 1
                if c == "}" then
                    return obj
                elseif c ~= "," then
                    fail("expected , or }")
                end
            end
        elseif c == "[" then
            local arr = {}
            pos = pos + 1
            space()
            if s:sub(pos, pos) == "]" then
                pos = pos + 1
                return arr
            end
            while true do
                local v = value()
                arr[#arr + 1] = v
                space()
                c = s:sub(pos, pos)
                pos = pos + 1
                if c == "]" then
                    return arr
                elseif c ~= "," then
                    fail("expected , or ]")
                end
            end
        elseif c == '"' then
            return str()
        elseif s:sub(pos, pos + 3) == "true" then
            pos = pos + 4
            return true
        elseif s:sub(pos, pos + 4) == "false" then
            pos = pos + 5
            return false
        elseif s:sub(pos, pos + 3) == "null" then
            pos = pos + 4
            return json.null
        end
        local num = s:match("^-?%d+%.?%d*[eE]?[-+]?%d*", pos)
        if not num or num == "" then
            fail("unexpected " .. (c == "" and "end" or c))
        end
        pos = pos + #num
        return tonumber(num)
    end

    return function()
        local v = value()
        space()
        if pos <= #s then
            fail("text after the value")
        end
        return v
    end
end

--- Read JSON text. A null is `json.null`, so arrays keep their length.
function json.decode(s)
    return decoder(s)()
end

return json
