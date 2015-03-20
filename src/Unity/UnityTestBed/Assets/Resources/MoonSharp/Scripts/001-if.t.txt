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

=head1 Lua if statement

=head2 Synopsis

    % prove 001-if.t

=head2 Description

See "Lua 5.2 Reference Manual", section 3.3.4 "Control Structures",
L<http://www.lua.org/manual/5.2/manual.html#3.3.4>.

See "Programming in Lua", section 4.3 "Control Structures".

=cut

]]

print("1..6")

if true then
    print("ok 1")
else
    print("not ok 1")
end

if not true then
    print("not ok 2")
else
    print("ok 2")
end

a = 12
b = 34
if a < b then
    print("ok 3")
else
    print("not ok 3")
end

a = 0
b = 4
if a < b then
    print("ok 4")
elseif a == b then
    print("not ok 4")
else
    print("not ok 4")
end

a = 5
b = 5
if a < b then
    print("not ok 5")
elseif a == b then
    print("ok 5")
else
    print("not ok 5")
end

a = 10
b = 6
if a < b then
    print("not ok 6")
elseif a == b then
    print("not ok 6")
else
    print("ok 6")
end

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
