#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009-2013, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua Stand-alone

=head2 Synopsis

    % prove 241-standalone.t

=head2 Description

See "Lua 5.2 Reference Manual", section 7 "Lua Stand-alone",
L<http://www.lua.org/manual/5.2/manual.html#7>.

=cut

--]]

require 'Test.More'

local lua = (platform and platform.lua) or arg[-1]
local luac = (platform and platform.luac) or lua .. 'c'

if not pcall(io.popen, lua .. [[ -e "a=1"]]) then
    skip_all "io.popen not supported"
end

plan(28)
diag(lua)

f = io.open('hello.lua', 'w')
f:write([[
print 'Hello World'
]])
f:close()

cmd = lua .. " hello.lua"
f = io.popen(cmd)
is(f:read'*l', 'Hello World', "file")
f:close()

cmd = lua .. " no_file.lua 2>&1"
f = io.popen(cmd)
like(f:read'*l', "^[^:]+: cannot open no_file.lua", "no file")
f:close()

if jit then
    os.execute(lua .. " -b hello.lua hello.luac")
else
    os.execute(luac .. " -s -o hello.luac hello.lua")
end
cmd = lua .. " hello.luac"
f = io.popen(cmd)
is(f:read'*l', 'Hello World', "bytecode")
f:close()
os.remove('hello.luac') -- clean up

if jit then
    skip("LuaJIT intentional. cannot combine sources", 2)
else
    os.execute(luac .. " -s -o hello2.luac hello.lua hello.lua")
    cmd = lua .. " hello2.luac"
    f = io.popen(cmd)
    is(f:read'*l', 'Hello World', "combine 1")
    is(f:read'*l', 'Hello World', "combine 2")
    f:close()
    os.remove('hello2.luac') -- clean up
end

cmd = lua .. " < hello.lua"
f = io.popen(cmd)
is(f:read'*l', 'Hello World', "redirect")
f:close()

cmd = lua .. [[ -e"a=1" -e "print(a)"]]
f = io.popen(cmd)
is(f:read'*l', '1', "-e")
f:close()

cmd = lua .. [[ -e "error('msg')"  2>&1]]
f = io.popen(cmd)
is(f:read'*l', lua .. [[: (command line):1: msg]], "error")
is(f:read'*l', "stack traceback:", "backtrace")
f:close()

cmd = lua .. [[ -e "error(setmetatable({}, {__tostring=function() return 'MSG' end}))"  2>&1]]
f = io.popen(cmd)
is(f:read'*l', lua .. [[: MSG]], "error with object")
if jit then
    todo("LuaJIT intentional.", 1)
end
is(f:read'*l', nil, "not backtrace")
f:close()

cmd = lua .. [[ -e "error{}"  2>&1]]
f = io.popen(cmd)
if jit then
    todo("LuaJIT TODO.", 1)
end
is(f:read'*l', lua .. [[: (no error message)]], "error")
is(f:read'*l', nil, "not backtrace")
f:close()

cmd = lua .. [[ -e"a=1" -e "print(a)" hello.lua]]
f = io.popen(cmd)
is(f:read'*l', '1', "-e & script")
is(f:read'*l', 'Hello World')
f:close()

cmd = lua .. [[ -e "?syntax error?" 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "lua", "-e bad")
f:close()

cmd = lua .. [[ -e 2>&1]]
f = io.popen(cmd)
if jit then
    skip("LuaJIT.", 1)
else
    like(f:read'*l', "^[^:]+: '%-e' needs argument", "no file")
end
like(f:read'*l', "^usage: ", "no file")
f:close()

cmd = lua .. [[ -v 2>&1]]
f = io.popen(cmd)
like(f:read'*l', '^Lua', "-v")
f:close()

cmd = lua .. [[ -v hello.lua 2>&1]]
f = io.popen(cmd)
like(f:read'*l', '^Lua', "-v & script")
is(f:read'*l', 'Hello World')
f:close()

cmd = lua .. [[ -E hello.lua 2>&1]]
f = io.popen(cmd)
is(f:read'*l', 'Hello World')
f:close()

cmd = lua .. [[ -u 2>&1]]
f = io.popen(cmd)
if jit then
    skip("LuaJIT.", 1)
else
    like(f:read'*l', "^[^:]+: unrecognized option '%-u'", "unknown option")
end
like(f:read'*l', "^usage: ", "no file")
f:close()

cmd = lua .. [[ -lTest.More -e "print(type(ok))"]]
f = io.popen(cmd)
is(f:read'*l', 'function', "-lTest.More")
f:close()

cmd = lua .. [[ -l Test.More -e "print(type(ok))"]]
f = io.popen(cmd)
is(f:read'*l', 'function', "-l Test.More")
f:close()

cmd = lua .. [[ -l socket -e "print(1)" 2>&1]]
f = io.popen(cmd)
isnt(f:read'*l', nil, "-l socket")
f:close()

cmd = lua .. [[ -l no_lib hello.lua 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "^[^:]+: module 'no_lib' not found:", "-l no lib")
f:close()

os.remove('hello.lua') -- clean up

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
