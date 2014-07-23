#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009-2011, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua Library

=head2 Synopsis

    % prove 320-stdin.t

=head2 Description

Tests Lua Basic & IO Libraries with stdin

=cut

--]]

require 'Test.More'

local lua = (platform and platform.lua) or arg[-1]

if not pcall(io.popen, lua .. [[ -e "a=1"]]) then
    skip_all "io.popen not supported"
end

plan(12)

f = io.open('lib1.lua', 'w')
f:write[[
function norm (x, y)
    return (x^2 + y^2)^0.5
end

function twice (x)
    return 2*x
end
]]
f:close()

cmd = lua .. [[ -e "dofile(); n = norm(3.4, 1.0); print(twice(n))" < lib1.lua]]
f = io.popen(cmd)
like(f:read'*l', '^7%.088', "function dofile (stdin)")
f:close()

os.remove('lib1.lua') -- clean up

f = io.open('foo.lua', 'w')
f:write[[
function foo (x)
    return x
end
]]
f:close()

cmd = lua .. [[ -e "f = loadfile(); print(foo); f(); print(foo('ok'))" < foo.lua]]
f = io.popen(cmd)
is(f:read'*l', 'nil', "function loadfile (stdin)")
is(f:read'*l', 'ok')
f:close()

os.remove('foo.lua') -- clean up

f = io.open('file.txt', 'w')
f:write("file with text\n")
f:close()

cmd = lua .. [[ -e "print(io.read'*l'); print(io.read'*l'); print(io.type(io.stdin))" < file.txt]]
f = io.popen(cmd)
is(f:read'*l', 'file with text', "function io.read *l")
is(f:read'*l', 'nil')
is(f:read'*l', 'file')
f:close()

f = io.open('number.txt', 'w')
f:write("6.0     -3.23   15e12\n")
f:write("4.3     234     1000001\n")
f:close()

cmd = lua .. [[ -e "while true do local n1, n2, n3 = io.read('*number', '*number', '*number'); if not n1 then break end; print(math.max(n1, n2, n3)) end" < number.txt]]
f = io.popen(cmd)
is(f:read'*l', '15000000000000', "function io:read *number")
is(f:read'*l', '1000001')
f:close()

os.remove('number.txt') -- clean up

cmd = lua .. [[ -e "for line in io.lines() do print(line) end" < file.txt]]
f = io.popen(cmd)
is(f:read'*l', 'file with text', "function io.lines")
is(f:read'*l', nil)
f:close()

os.remove('file.txt') -- clean up

f = io.open('dbg.txt', 'w')
f:write("print 'ok'\n")
f:write("error 'dbg'\n")
f:write("cont\n")
f:close()

cmd = lua .. [[ -e "debug.debug()" < dbg.txt]]
f = io.popen(cmd)
is(f:read'*l', 'ok', "function debug.debug")
is(f:read'*l', nil)
f:close()

os.remove('dbg.txt') -- clean up


-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
