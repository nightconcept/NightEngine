-- Run Lua specs with plain LuaJIT:  luajit nightengine/lua/test.lua [--path DIR]... [--filter TEXT] DIR...
-- Each --path DIR adds DIR/?.lua and DIR/?/init.lua to package.path. This file's own folder is always there.
local here = arg[0]:match("^(.*)[/\\]") or "."
local dirs, filter = {}, nil
local paths = { here }
local i = 1
while i <= #arg do
    if arg[i] == "--path" then
        paths[#paths + 1] = arg[i + 1]
        i = i + 2
    elseif arg[i] == "--filter" then
        filter = arg[i + 1]
        i = i + 2
    else
        dirs[#dirs + 1] = arg[i]
        i = i + 1
    end
end
for _, p in ipairs(paths) do
    package.path = p .. "/?.lua;" .. p .. "/?/init.lua;" .. package.path
end
if #dirs == 0 then
    dirs = { here .. "/spec" }
end
os.exit(require("nightengine.testing").run(dirs, filter) == 0 and 0 or 1)
