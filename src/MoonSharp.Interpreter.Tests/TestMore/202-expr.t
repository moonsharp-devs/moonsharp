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

=head1 Lua expression

=head2 Synopsis

    % prove 202-expr.t

=head2 Description

See "Lua 5.2 Reference Manual", section 3.4 "Expressions",
L<http://www.lua.org/manual/5.2/manual.html#3.4>.

See "Programming in Lua", section 3 "Expressions".

=cut

--]]

require 'Test.More'

plan(39)

x = math.pi
is(x - x%0.01, 3.14, "modulo")

a = {}; a.x = 1; a.y = 0;
b = {}; b.x = 1; b.y = 0;
c = a
is(a == c, true, "relational op (by reference)")
is(a ~= b, true)

is('0' == 0, false, "relational op")
is(2 < 15, true)
is('2' < '15', false)

error_like(function () return 2 < '15' end,
           "compare",
           "relational op")

error_like(function () return '2' < 15 end,
           "compare",
           "relational op")

is(4 and 5, 5, "logical op")
is(nil and 13, nil)
is(false and 13, false)
is(4 or 5, 4)
is(false or 5, 5)
is(false or 'text', 'text')

is(10 or 20, 10, "logical op")
is(10 or error(), 10)
is(nil or 'a', 'a')
is(nil and 10, nil)
is(false and error(), false)
is(false and nil, false)
is(false or nil, nil)
is(10 and 20, 20)

is(not nil, true, "logical not")
is(not false, true)
is(not 0, false)
is(not not nil, false)
is(not 'text', false)
a = {}
is(not a, false)

is("Hello " .. "World", "Hello World", "concatenation")
is(0 .. 1, '01')
a = "Hello"
is(a .. " World", "Hello World")
is(a, "Hello")

is('10' + 1, 11, "coercion")
is('-5.3' * '2', -10.6)
is(10 .. 20, '1020')
is(tostring(10), '10')
is(10 .. '', '10')

error_like(function () return 'hello' + 1 end,
           "perform arithmetic",
           "no coercion")

error_like(function ()
                local function first() return 1 end
                local function limit() return end
                local function step()  return 2 end
                for i = first(), limit(), step() do
                    print(i)
                end
           end,
           "^[^:]+:%d+: 'for' limit must be a number",
           "for tonumber")

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
