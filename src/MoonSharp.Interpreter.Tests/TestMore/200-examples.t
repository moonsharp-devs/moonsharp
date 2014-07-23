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

=head1 some Lua code examples

=head2 Synopsis

    % prove 200-examples.t

=head2 Description

First tests in order to check infrastructure.

=cut

--]]

require 'Test.More'

plan(5)

function factorial (n)
    if n == 0 then
        return 1
    else
        return n * factorial(n-1)
    end
end
is(factorial(7), 5040, "factorial (recursive)")

local function local_factorial (n)
    if n == 0 then
        return 1
    else
        return n * local_factorial(n-1)
    end
end
is(local_factorial(7), 5040, "factorial (recursive)")

function loop_factorial (n)
    local a = 1
    for i = 1, n, 1 do
        a = a*i
    end
    return a
end
is(loop_factorial(7), 5040, "factorial (loop)")

function iter_factorial (n)
    local function iter (product, counter)
        if counter > n then
            return product
        else
            return iter(counter*product, counter+1)
        end
    end
    return iter(1, 1)
end
is(iter_factorial(7), 5040, "factorial (iter)")

--[[

  Knuth's "man or boy" test.
  See http://en.wikipedia.org/wiki/Man_or_boy_test

]]

local function A (k, x1, x2, x3, x4, x5)
    local function B ()
        k = k - 1
        return A(k, B, x1, x2, x3, x4)
    end
    if k <= 0 then
        return x4() + x5()
    else
        return B()
    end
end

is(A(10,
        function () return 1 end,
        function () return -1 end,
        function () return -1 end,
        function () return 1 end,
        function () return 0 end),
   -67,
   "man or boy"
)

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
