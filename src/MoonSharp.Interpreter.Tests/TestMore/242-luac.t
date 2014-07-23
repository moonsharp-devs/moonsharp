#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2010-2013, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua Stand-alone

=head2 Synopsis

    % prove t/242-luac.t

=head2 Description

See "Lua 5.2 Reference Manual", section 7 "Lua Stand-alone",
L<http://www.lua.org/manual/5.2/manual.html#7>.

=cut

--]]

require 'Test.More'

if jit then
    skip_all("LuaJIT")
end

local lua = (platform and platform.lua) or arg[-1]
local luac = (platform and platform.luac) or lua .. 'c'

if not pcall(io.popen, lua .. [[ -e "a=1"]]) then
    skip_all "io.popen not supported"
end

plan(14)
diag(luac)

f = io.open('hello.lua', 'w')
f:write([[
local a = false
b = a + 1
pi = 3.14
s = "all escaped \1\a\b\f\n\r\t\v\\\""
local t = { "a", "b", "c", "d" }
local f = table.concat
local function f () while true do print(a) end end
s = nil

print 'Hello World'
]])
f:close()

cmd = luac .. [[ -v 2>&1]]
f = io.popen(cmd)
like(f:read'*l', '^Lua', "-v")
f:close()

cmd = luac .. [[ -u 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "^[^:]+: unrecognized option '%-u'", "unknown option")
like(f:read'*l', "^usage:")
f:close()

cmd = luac .. [[ -p hello.lua 2>&1]]
f = io.popen(cmd)
is(f:read'*l', nil)
f:close()

cmd = luac .. [[ -p no_file.lua 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "^[^:]+: cannot open no_file.lua", "no file")
f:close()

cmd = luac .. [[ -v -l -l hello.lua]]
f = io.popen(cmd)
like(f:read'*l', '^Lua', "-v")
is(f:read'*l', '')
like(f:read'*l', "^main")
f:close()

cmd = luac .. [[ -l luac.out]]
f = io.popen(cmd)
is(f:read'*l', '')
like(f:read'*l', "^main")
f:close()

f = io.open('luac.out', 'w')
f:write "\x1bLua\x52\x00"
f:close()
cmd = luac .. [[ luac.out 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "truncated precompiled chunk")
f:close()

f = io.open('luac.out', 'w')
f:write "\x1bFoo\x52\x00\xde\xad\xbe\xef\x00\x19\x93\r\n\x1a\nCode"
f:close()
cmd = luac .. [[ luac.out 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "not a precompiled chunk")
f:close()

f = io.open('luac.out', 'w')
f:write "\x1bLua\x51\x00\xde\xad\xbe\xef\x00\x19\x93\r\n\x1a\nCode"
f:close()
cmd = luac .. [[ luac.out 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "version mismatch in precompiled chunk")
f:close()

f = io.open('luac.out', 'w')
f:write "\x1bLua\x52\x00\xde\xad\xbe\xef\x00\x19\x93\r\n\x1a\nCode"
f:close()
cmd = luac .. [[ luac.out 2>&1]]
f = io.popen(cmd)
like(f:read'*l', "incompatible precompiled chunk")
f:close()

os.remove('hello.lua') -- clean up
os.remove('luac.out') -- clean up

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
