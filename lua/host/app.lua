--- The LÖVE host: a fixed frame loop, input, sound, recording, replays, PNG frames, and the dev bridge.
--
-- love . [--seed N] [--target NAME] [--replay FILE] [--frames FILE OUT [EVERY]] [--dev] [key=value ...]
--
-- Every frame of input goes through Game:step, and a live run is saved to `last.json` in the save folder.
-- With --dev or NIGHTENGINE_DEV=1 the love2d-mcp bridge listens on 127.0.0.1:12345, and `run_lua` code gets `ne`.
-- Each key=value pair (`just dev`) reaches the game as a "dev key=value" event on frame 0 (see Input:dev).
local bit = require("bit")
local inputs = require("nightengine.inputs")
local replay = require("nightengine.replay")
local targets = require("nightengine.target")
local config, input, R, audio
local FPS
local lg = love.graphics

local App = {}

function App.configure(c)
    config, input, R, audio = c, c.input, c.renderer, c.audio
    FPS = c.fps or 60
    return App
end

local function read_file(path)
    local f = io.open(path, "rb")
    if f then
        local s = f:read("*a")
        f:close()
        return s
    end
    return assert(love.filesystem.read(path))
end

local function write_file(path, data)
    local f = assert(io.open(path, "wb"))
    f:write(data)
    f:close()
end

local function parse(args)
    local opts, i = { pairs = {} }, 1
    while i <= #args do
        local a = args[i]
        if a == "--dev" then
            opts.dev = true
            i = i + 1
        elseif a:match("^[%w_%-]+=") then -- A `just dev` pair, key=value.
            opts.pairs[#opts.pairs + 1] = a
            i = i + 1
        elseif a == "--seed" or a == "--target" or a == "--replay" then
            opts[a:sub(3)] = args[i + 1]
            i = i + 2
        elseif a == "--frames" then
            opts.frames, opts.out, opts.every = args[i + 1], args[i + 2], tonumber(args[i + 3]) or 60
            i = i + 4
        else
            i = i + 1
        end
    end
    return opts
end

local mobile = love.system.getOS() == "Android" or love.system.getOS() == "iOS"

function App.load(args)
    local opts = parse(args)
    input.setup()
    audio.setup()
    R.setup()
    local target
    if opts.replay or opts.frames then
        App.replay = replay.from_json(read_file(opts.replay or opts.frames))
        target = App.replay.target
        App.seed = App.replay.seed
    else
        App.seed = tonumber(opts.seed) or love.math.random(0, 2 ^ 31 - 1)
    end
    target = (target or targets.current(opts.target, config.targets)):sized(config.width, config.height)
    App.target = target
    App.game = config.make_game(App.seed, target)
    if not App.replay then
        App.recording =
            replay.new(App.seed, nil, nil, replay.digits_for(App.game.input_mask or config.input_mask or 127), target)
        for _, pair in ipairs(opts.pairs) do -- Frame 0 gets them. A replay has them in its recording.
            App.post(inputs.dev_event(pair))
        end
    end
    App.canvas = nil
    App.fit()
    if opts.frames then
        App.export(opts.out, opts.every)
        love.event.quit()
        return
    end
    App.paused, App.steps, App.held = false, 0, { code = 0, frames = 0 }
    App.dev = require("nightengine.host.controls").new(App, config)
    if opts.dev or os.getenv("NIGHTENGINE_DEV") == "1" then -- The bridge is excluded from release builds.
        App.bridge()
    end
end

--- Make the canvas the size the game wants.
function App.fit()
    local w, h = App.game:screen_size()
    if not w then
        w, h = App.target.width, App.target.height
    end
    if not App.canvas or App.canvas:getWidth() ~= w or App.canvas:getHeight() ~= h then
        App.canvas = lg.newCanvas(w, h)
        App.canvas:setFilter("nearest", "nearest")
        if not mobile and not App.sized then
            local _, _, flags = love.window.getMode()
            local dw, dh = love.window.getDesktopDimensions(flags.display)
            local scale = math.max(1, math.min(math.floor(dw * 0.9 / w), math.floor(dh * 0.85 / h)))
            love.window.setMode(w * scale, h * scale, { resizable = true, minwidth = w, minheight = h })
        end
    end
end

--- Where the canvas goes in the window: a whole-number scale on a computer, the biggest fit on a phone.
function App.placement()
    local ww, wh = lg.getDimensions()
    local w, h = App.canvas:getDimensions()
    local scale = math.min(ww / w, wh / h)
    if not mobile and scale >= 1 then
        scale = math.floor(scale)
    end
    return math.floor((ww - w * scale) / 2), math.floor((wh - h * scale) / 2), scale
end

function App.to_game(x, y)
    local ox, oy, scale = App.placement()
    local w, h = App.canvas:getDimensions()
    local gx = math.max(0, math.min(w - 1, math.floor((x - ox) / scale)))
    local gy = math.max(0, math.min(h - 1, math.floor((y - oy) / scale)))
    return gx, gy
end

function App.post(event)
    App.pending = App.pending or {}
    App.pending[#App.pending + 1] = event
end

function App.save(path)
    if App.recording and #App.recording.frames > 0 then
        local data = App.recording:to_json() .. "\n"
        if path then
            write_file(path, data)
        else
            assert(love.filesystem.write("last.json", data))
        end
    end
end

function App.quit()
    App.save()
    love.event.quit()
end

--- One frame of the game.
function App.tick()
    local game = App.game
    local code, pointers, events
    if App.replay then
        if game.frame >= #App.replay.frames then
            return
        end
        code, pointers, events =
            App.replay.frames[game.frame + 1], App.replay:at(game.frame), App.replay:events_at(game.frame)
    else
        code = input.buttons()
        if App.held.frames > 0 then
            code = bit.bor(code, App.held.code)
            App.held.frames = App.held.frames - 1
        end
        pointers = input.pointers(App.to_game, not mobile)
        events, App.pending = App.pending or {}, {}
        App.recording:add(code, pointers, events)
    end
    local cues = game:step(code, pointers, events)
    App.fit()
    audio.play(cues)
    if game.quit_requested then
        App.quit()
    end
end

function App.update()
    if App.bridge_update then
        App.bridge_update()
    end
    if App.paused then
        if App.steps > 0 then
            App.steps = App.steps - 1
            App.tick()
        end
        return
    end
    App.tick()
end

function App.render()
    lg.setCanvas(App.canvas)
    lg.origin()
    R.draw(App.game)
    if App.replay and not App.exporting then
        local over = App.game.frame >= #App.replay.frames
        if config.replay_label then
            config.replay_label(over)
        end
    end
    lg.setCanvas()
end

function App.draw()
    App.render()
    lg.clear(0, 0, 0, 1)
    lg.setColor(1, 1, 1, 1)
    local ox, oy, scale = App.placement()
    lg.draw(App.canvas, ox, oy, 0, scale, scale)
    if App.capture then
        App.capture()
    end
end

function App.keypressed(key)
    input.keypressed(key)
    if key == "q" or (key == "f4" and love.keyboard.isDown("lalt", "ralt")) then
        App.quit()
    end
end

--- Save PNG files of a replay: every `every` frames, and the last.
function App.export(out, every)
    assert(type(out) == "string" and out ~= "", "frame output directory is required")
    assert(
        type(every) == "number" and every >= 1 and every == math.floor(every),
        "frame interval must be a positive integer"
    )
    local command
    if package.config:sub(1, 1) == "\\" then -- Windows: cmd.exe mkdir creates parents but fails on a folder.
        local path = out:gsub("/", "\\")
        command = 'if not exist "' .. path .. '" mkdir "' .. path .. '"'
    else
        command = "mkdir -p '" .. out:gsub("'", "'\"'\"'") .. "'"
    end
    assert(os.execute(command) == 0, "could not create frame output directory")
    local game, rec = App.game, App.replay
    App.exporting = true
    local function shot(name)
        App.fit()
        App.render()
        write_file(out .. "/" .. name .. ".png", App.canvas:newImageData():encode("png"):getString())
    end
    for i, code in ipairs(rec.frames) do
        if game.frame % every == 0 then
            shot(string.format("frame%05d", game.frame))
        end
        game:step(code, rec:at(i - 1), rec:events_at(i - 1))
    end
    shot("final")
    print("frames in " .. out)
end

-- The dev bridge (love2d-mcp) ------------------------------------------------------------

function App.bridge()
    local bridge = require("nightengine.host.dev.mcp_bridge")
    bridge.init(12345)
    input.is_down = bridge.isDown -- Injected keys use normal input and recording.
    local ne = App.dev
    bridge.setEnv({ ne = ne, require = require })
    bridge.setObjectGetter(function()
        return { game = ne.state() }
    end)
    App.bridge_update = bridge.update
    App.capture = bridge.captureIfPending
end

--- Fixed simulation timing, independent of the display refresh rate.
function App.run()
    App.load(love.arg.parseGameArguments(arg))
    love.timer.step()
    local lag = 0
    return function()
        love.event.pump()
        for name, a, b, c, d, e, f in love.event.poll() do
            if name == "quit" then
                App.save()
                return a or 0
            elseif name == "keypressed" then
                App.keypressed(a)
            end
            love.handlers[name](a, b, c, d, e, f)
        end
        lag = math.min(lag + love.timer.step(), 0.25)
        local stepped = false
        while lag >= 1 / FPS do
            lag = lag - 1 / FPS
            App.update()
            stepped = true
        end
        if stepped and lg.isActive() then
            App.draw()
            lg.present()
        end
        love.timer.sleep(0.001)
    end
end

return App
