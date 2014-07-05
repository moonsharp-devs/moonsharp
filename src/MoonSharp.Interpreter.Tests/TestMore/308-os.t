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

=head1 Lua Operating System Library

=head2 Synopsis

    % prove 308-os.t

=head2 Description

Tests Lua Operating System Library

See "Lua 5.1 Reference Manual", section 5.8 "Operating System Facilities",
L<http://www.lua.org/manual/5.1/manual.html#5.8>.

See "Programming in Lua", section 22 "The Operating System Library".

=cut

--]]

require 'Test.More'

plan(37)

local lua = (platform and platform.lua) or arg[-1]

clk = os.clock()
type_ok(clk, 'number', "function clock")
ok(clk <= os.clock())

d = os.date('!*t', 0)
is(d.year, 1970, "function date")
is(d.month, 1)
is(d.day, 1)
is(d.hour, 0)
is(d.min, 0)
is(d.sec, 0)
is(d.wday, 5)
is(d.yday, 1)
is(d.isdst, false)

is(os.date('!%d/%m/%y %H:%M:%S', 0), '01/01/70 00:00:00', "function date")

like(os.date('%H:%M:%S'), '^%d%d:%d%d:%d%d', "function date")

is(os.difftime(1234, 1200), 34, "function difftime")
is(os.difftime(1234), 1234)

r = os.execute()
is(r, 1, "function execute")

cmd = lua .. [[ -e "print '# hello from external Lua'; os.exit(2)"]]
if platform and platform.osname == 'MSWin32' then
    is(os.execute(cmd), 2, "function execute & exit")
else
    is(os.execute(cmd), 512, "function execute & exit")
end

cmd = lua .. [[ -e "print 'reached'; os.exit(); print 'not reached';"]]
r, f = pcall(io.popen, cmd)
if r then
    is(f:read'*l', 'reached', "function exit")
    is(f:read'*l', nil)
    f:close()
else
    skip("io.popen not supported", 2)
end

is(os.getenv('__IMPROBABLE__'), nil, "function getenv")

user = os.getenv('LOGNAME') or os.getenv('USERNAME')
type_ok(user, 'string', "function getenv")

local f = io.open('file.rm', 'w')
f:write("file to remove")
f:close()
r = os.remove("file.rm")
is(r, true, "function remove")

r, msg = os.remove('file.rm')
is(r, nil, "function remove")
like(msg, '^file.rm: No such file or directory')

local f = io.open('file.old', 'w')
f:write("file to rename")
f:close()
os.remove('file.new')
r = os.rename('file.old', 'file.new')
is(r, true, "function rename")
os.remove('file.new') -- clean up

r, msg = os.rename('file.old', 'file.new')
is(r, nil, "function rename")
like(msg, '^file.old: No such file or directory')

is(os.setlocale('C', 'all'), 'C', "function setlocale")
is(os.setlocale(), 'C')

is(os.setlocale('unk_loc', 'all'), nil, "function setlocale (unknown locale)")

like(os.time(), '^%d+%.?%d*$', "function time")

like(os.time(nil), '^%d+%.?%d*$', "function time")

like(os.time({
    sec = 0,
    min = 0,
    hour = 0,
    day = 1,
    month = 1,
    year = 2000,
    isdst = 0,
}), '^946%d+$', "function time")

if platform and platform.intsize == 8 then
    todo("pb on 64bit platforms")
    -- os.time returns nil when C mktime returns < 0
    -- this test needs a out of range value on any platform
end
is(os.time({
    sec = 0,
    min = 0,
    hour = 0,
    day = 1,
    month = 1,
    year = 1000,
    isdst = 0,
}), nil, "function time -> nil")

error_like(function () os.time{} end,
           "^[^:]+:%d+: field 'day' missing in date table",
           "function time (missing field)")

fname = os.tmpname()
type_ok(fname, 'string', "function tmpname")
ok(fname ~= os.tmpname())

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
