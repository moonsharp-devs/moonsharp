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

=head1 Lua scope

=head2 Synopsis

    % prove 211-scope.t

=head2 Description

See "Lua 5.2 Reference Manual", section 3.5 "Visibility Rules",
L<http://www.lua.org/manual/5.2/manual.html#3.5>.

See "Programming in Lua", section 4.2 "Local Variables and Blocks".

=cut

--]]

require 'Test.More'

plan(10)

--[[ scope ]]
x = 10
do
    local x = x
    is(x, 10, "scope")
    x = x + 1
    do
        local x = x + 1
        is(x, 12)
    end
    is(x, 11)
end
is(x, 10)

--[[ scope ]]
x = 10
local i = 1

while i<=x do
    local x = i*2
--    print(x)
    i = i + 1
end

if i > 20 then
    local x
    x = 20
    nok("scope")
else
    is(x, 10, "scope")
end

is(x, 10)

--[[ scope ]]
local a, b = 1, 10
if a < b then
    is(a, 1, "scope")
    local a
    is(a, nil)
end
is(a, 1)
is(b, 10)

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
