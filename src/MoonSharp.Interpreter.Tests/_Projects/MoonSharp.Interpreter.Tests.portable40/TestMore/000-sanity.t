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

=head1 Lua test suite

=head2 Synopsis

    % prove 000-sanity.t

=head2 Description

=cut

]]

function f (n)
    return n + 1
end

function g (m, p)
    return m + p
end

print('1..9')
print("ok 1 -")
print('ok', 2, "- list")
print("ok " .. 3 .. " - concatenation")
i = 4
print("ok " .. i .. " - var")
i = i + 1
print("ok " .. i .. " - var incr")
print("ok " .. i+1 .. " - expr")
j = f(i + 1)
print("ok " .. j .. " - call f")
k = g(i, 3)
print("ok " .. k .. " - call g")
local print = print
print("ok 9 - local")

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
