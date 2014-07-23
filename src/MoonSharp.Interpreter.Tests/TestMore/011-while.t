#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009-2010, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua while statement

=head2 Synopsis

    % prove 011-while.t

=head2 Description

See "Lua 5.2 Reference Manual", section 3.3.4 "Control Structures",
L<http://www.lua.org/manual/5.2/manual.html#3.3.4>.

See "Programming in Lua", section 4.3 "Control Structures".

=cut

]]

print("1..11")

a = {}
local i = 1
while a[i] do
    i = i + 1
end
if i == 1 then
    print("ok 1 - while empty")
else
    print("not ok 1 - " .. i)
end

a = {"ok 2 - while ", "ok 3", "ok 4"}
local i = 1
while a[i] do
    print(a[i])
    i = i + 1
end

a = {"ok 5 - with break", "ok 6", "stop", "more"}
local i = 1
while a[i] do
    if a[i] == 'stop' then break end
    print(a[i])
    i = i + 1
end
if i == 3 then
    print("ok 7 - break")
else
    print("not ok 7 - " .. i)
end

x = 3
local i = 1
while i<=x do
    print("ok " .. 7+i)
    i = i + 1
end
if i == 4 then
    print("ok 11")
else
    print("not ok 11 - " .. i)
end

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
