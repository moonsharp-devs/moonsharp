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

=head1 Lua Input/Output Library

=head2 Synopsis

    % prove 308-io.t

=head2 Description

Tests Lua Input/Output Library

See "Lua 5.2 Reference Manual", section 6.8 "Input and Output Facilities",
L<http://www.lua.org/manual/5.2/manual.html#6.8>.

See "Programming in Lua", section 21 "The I/O Library".

=cut

--]]

require 'Test.More'

local lua = [[\git\moonsharp\src\Tools\lua52.exe]]

plan(67)

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
is(msg, "file.no: No such file or directory")

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

if jit then
    todo("LuaJIT TODO. open mode")
end
error_like(function () io.open('file.txt', 'baz') end,
           "^[^:]+:%d+: bad argument #2 to 'open' %(invalid mode%)",
           "function open (bad mode)")

		   
is(io.type("not a file"), nil, "function type")
f = io.open('file.txt')
is(io.type(f), 'file')
like(tostring(f), '^file %(0?[Xx]?%x+%)$')
io.close(f)
is(io.type(f), 'closed file')
is(tostring(f), 'file (closed)')

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
    is(io.close(f), true)
else
    skip("io.popen not supported", 3)
end

r, f = pcall(io.popen, lua .. [[ -e "for line in io.lines() do print((line:gsub('e', 'a'))) end"]], 'w')
if r then
    is(io.type(f), 'file', "popen (write)")
    f:write("# hello\n") -- not tested : hallo
    is(io.close(f), true)
else
    skip("io.popen not supported", 2)
end

for line in io.lines('file.txt') do
    is(line, "file with text", "function lines(filename)")
end

error_like(function () io.lines('file.no') end,
           "No such file or directory",
           "function lines(no filename)")

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
s1, s2 = f:read('*L', '*L')
is(s1:len(), 15, "method read *L")
is(s1, "file with text\n")
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

f = io.open('file.txt')
for two_char in f:lines(2) do
    is(two_char, "fi", "method lines (with read option)")
    break
end
f:close()

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
is(f:write('end'), f, "method write")
f:close()

os.remove('file.out') --clean up

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:

--]==]

