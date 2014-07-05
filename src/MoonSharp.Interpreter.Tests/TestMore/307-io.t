#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua Input/Output Library

=head2 Synopsis

    % prove 307-io.t

=head2 Description

Tests Lua Input/Output Library

See "Lua 5.1 Reference Manual", section 5.7 "Input and Output Facilities",
L<http://www.lua.org/manual/5.1/manual.html#5.7>.

See "Programming in Lua", section 21 "The I/O Library".

=cut

--]]

require 'Test.More'

local lua = (platform and platform.lua) or arg[-1]

plan(61)

is(getfenv(io.lines), _G, "environment")
local env = debug.getfenv(io.lines)
if jit then
    todo("LuaJIT intentional. It is an implementation-defined behavior.", 3)
end
type_ok(env.__close, 'function')
is(env[1], io.stdin)
is(env[2], io.stdout)

like(io.stdin, '^file %(0?[Xx]?%x+%)$', "variable stdin")

like(io.stdout, '^file %(0?[Xx]?%x+%)$', "variable stdout")

like(io.stderr, '^file %(0?[Xx]?%x+%)$', "variable stderr")

r, msg = io.close(io.stderr)
is(r, nil, "close (std)")
is(msg, "cannot close standard file")

is(io.flush(), true, "function flush")

os.remove('file.no')
f, msg = io.open("file.no")
is(f, nil, "function open")
like(msg, "^file.no: ")

os.remove('file.txt')
f = io.open('file.txt', 'w')
f:write("file with text\n")
f:close()
f = io.open('file.txt')
like(f, '^file %(0?[Xx]?%x+%)$', "function open")

is(io.close(f), true, "function close")

error_like(function () io.close(f) end,
           "^[^:]+:%d+: attempt to use a closed file",
           "function close (closed)")

is(io.type("not a file"), nil, "function type")
f = io.open('file.txt')
is(io.type(f), 'file')
io.close(f)
is(io.type(f), 'closed file')

is(io.stdin, io.input(), "function input")
is(io.stdin, io.input(nil))
f = io.stdin
like(io.input('file.txt'), '^file %(0?[Xx]?%x+%)$')
is(f, io.input(f))

is(io.output(), io.stdout, "function output")
is(io.output(nil), io.stdout)
f = io.stdout
like(io.output('output.new'), '^file %(0?[Xx]?%x+%)$')
is(f, io.output(f))
os.remove('output.new')

r, f = pcall(io.popen, lua .. [[ -e "print 'standard output'"]])
if r then
    is(io.type(f), 'file', "popen (read)")
    is(f:read(), "standard output")
    io.close(f)
else
    skip("io.popen not supported", 2)
end

r, f = pcall(io.popen, lua .. [[ -e "for line in io.lines() do print((line:gsub('e', 'a'))) end"]], 'w')
if r then
    is(io.type(f), 'file', "popen (write)")
    f:write("# hello\n") -- not tested : hallo
    f:close()
else
    skip("io.popen not supported", 1)
end

for line in io.lines('file.txt') do
    is(line, "file with text", "function lines(filename)")
end

f = io.tmpfile()
is(io.type(f), 'file', "function tmpfile")
f:write("some text")
f:close()

io.write() -- not tested
io.write('# text', 12, "\n") -- not tested :  # text12

r, msg = io.stderr:close()
is(r, nil, "method close (std)")
is(msg, "cannot close standard file")

f = io.open('file.txt')
is(f:close(), true, "method close")

is(io.stderr:flush(), true, "method flush")

error_like(function () f:flush() end,
           "^[^:]+:%d+: attempt to use a closed file",
           "method flush (closed)")

error_like(function () f:read() end,
           "^[^:]+:%d+: attempt to use a closed file",
           "method read (closed)")

f = io.open('file.txt')
s = f:read()
is(s:len(), 14, "method read")
is(s, "file with text")
s = f:read()
is(s, nil)
f:close()

f = io.open('file.txt')
error_like(function () f:read('*z') end,
           "^[^:]+:%d+: bad argument #1 to 'read' %(invalid %w+%)",
           "method read (invalid)")
f:close()

f = io.open('file.txt')
s1, s2 = f:read('*l', '*l')
is(s1:len(), 14, "method read *l")
is(s1, "file with text")
is(s2, nil)
f:close()

f = io.open('file.txt')
n1, n2 = f:read('*n', '*n')
is(n1, nil, "method read *n")
is(n2, nil)
f:close()

f = io.open('file.txt')
s = f:read('*a')
is(s:len(), 15, "method read *a")
is(s, "file with text\n")
f:close()

f = io.open('file.txt')
is(f:read(0), '', "method read number")
eq_array({f:read(5, 5, 15)}, {'file ', 'with ', "text\n"})
-- print(f:read(0))
f:close()

f = io.open('file.txt')
for line in f:lines() do
    is(line, "file with text", "method lines")
end
is(io.type(f), 'file')
f:close()
is(io.type(f), 'closed file')

error_like(function () f:seek('end', 0) end,
           "^[^:]+:%d+: attempt to use a closed file",
           "method seek (closed)")

f = io.open('file.txt')
error_like(function () f:seek('bad', 0) end,
           "^[^:]+:%d+: bad argument #1 to 'seek' %(invalid option 'bad'%)",
           "method seek (invalid)")

f = io.open('file.txt')
if platform and platform.osname == 'MSWin32' then
    is(f:seek('end', 0), 16, "method seek")
else
    is(f:seek('end', 0), 15, "method seek")
end
f:close()

f = io.open('file.txt')
is(f:setvbuf('no'), true, "method setvbuf 'no'")

is(f:setvbuf('full', 4096), true, "method setvbuf 'full'")

is(f:setvbuf('line', 132), true, "method setvbuf 'line'")
f:close()

os.remove('file.txt') -- clean up

f = io.open('file.out', 'w')
f:close()
error_like(function () f:write('end') end,
           "^[^:]+:%d+: attempt to use a closed file",
           "method write (closed)")

f = io.open('file.out', 'w')
is(f:write('end'), true, "method write")
f:close()

os.remove('file.out') --clean up

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
