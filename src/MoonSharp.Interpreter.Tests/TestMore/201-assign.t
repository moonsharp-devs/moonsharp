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

=head1 Lua assignment

=head2 Synopsis

    % prove 201-assign.t

=head2 Description

See "Lua 5.1 Reference Manual", section 2.4.3 "Assignment",
L<http://www.lua.org/manual/5.1/manual.html#2.4.3>.

See "Programming in Lua", section 4.1 "Assignment".

=cut

--]]

require 'Test.More'

plan(35)

is(b, nil, "global variable")
b = 10
is(b, 10)
b = nil
is(b, nil)

a = {}
i = 3
i, a[i] = i+1, 20
is(i, 4, "check eval")
is(a[3], 20)

x = 1.
y = 2.
x, y = y, x -- swap
is(x, 2, "check swap")
is(y, 1)

a, b, c = 0, 1
is(a, 0, "check padding")
is(b, 1)
is(c, nil)
a, b = a+1, b+1, a+b
is(a, 1)
is(b, 2)
a, b, c = 0
is(a, 0)
is(b, nil)
is(c, nil)

function f() return 1, 2 end
a, b, c, d = f()
is(a, 1, "adjust with function")
is(b, 2)
is(c, nil)
is(d, nil)

function f() print('# f') end
a = 2
a, b, c = f(), 3
is(a, nil, "padding with function")
is(b, 3)
is(c, nil)

local my_i = 1
is(my_i, 1, "local variable")
local my_i = 2
is(my_i, 2)

local i = 1
local j = i
is(i, 1, "local variable")
is(j, 1)
j = 2
is(i, 1)
is(j, 2)

local function f(x) return 2*x end
is(f(2), 4, "param & result of function")
a = 2
a = f(a)
is(a, 4)
local b = 2
b = f(b)
is(b, 4)

local n1 = 1
local n2 = 2
local n3 = 3
local n4 = 4
n1,n2,n3,n4 = n4,n3,n2,n1
is(n1, 4, "assignment list swap values")
is(n2, 3)
is(n3, 2)
is(n4, 1)

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
