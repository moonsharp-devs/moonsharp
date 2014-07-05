
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--

local error = error
local pairs = pairs
local type = type
local _G = _G
local debug = require 'debug'

local tb  = require 'Test.Builder'.new()
local out = require 'Test.Builder.Tester.File'.new 'out'
local err = require 'Test.Builder.Tester.File'.new 'err'

_ENV = nil
local m = {}

-- for remembering that we're testing and where we're testing at
local testing = false
local testing_num

-- remembering where the file handles were originally connected
local original_output_handle
local original_failure_handle
local original_todo_handle

local function _start_testing ()
    -- remember what the handles were set to
    original_output_handle  = tb:output()
    original_failure_handle = tb:failure_output()
    original_todo_handle    = tb:todo_output()

    -- switch out to our own handles
    tb:output(out)
    tb:failure_output(err)
    tb:todo_output(err)

    -- clear the expected list
    out:reset()
    err:reset()

    -- remeber that we're testing
    testing = true
    testing_num = tb:current_test()
    tb:current_test(0)

    -- look, we shouldn't do the ending stuff
    tb.no_ending = true
end

function m.test_out (...)
    if not testing then
        _start_testing()
    end
    out:expect(...)
end

function m.test_err (...)
    if not testing then
        _start_testing()
    end
    err:expect(...)
end

function m.test_fail (offset)
    offset = offset or 0
    if not testing then
        _start_testing()
    end
    local info = debug.getinfo(2)
    local prog = info.short_src
    local line = info.currentline + offset
    err:expect("#     Failed test (" .. prog .. " at line " .. line .. ")")
end

function m.test_diag (...)
    local arg = {...}
    if not testing then
        _start_testing()
    end
    for i = 1, #arg do
        err:expect("# " .. arg[i])
    end
end

function m.test_test (args)
    local mess
    if type(args) == 'table' then
        mess = args[1]
    else
        mess = args
        args = {}
    end

    if not testing then
        error "Not testing.  You must declare output with a test function first."
    end

    -- okay, reconnect the test suite back to the saved handles
    tb:output(original_output_handle)
    tb:failure_output(original_failure_handle)
    tb:todo_output(original_todo_handle)

    -- restore the test no, etc, back to the original point
    tb:current_test(testing_num)
    testing = false

    -- check the output we've stashed
    local pass = (args.skip_out or out:check())
             and (args.skip_err or err:check())
    tb:ok(pass, mess)
    if not pass then
        -- print out the diagnostic information about why this
        -- test failed
        if not out:check() then
            tb:diag(out:complaint())
        end
        if not err:check() then
            tb:diag(err:complaint())
        end
    end
end

function m.line_num ()
    return debug.getinfo(2).currentline
end

for k, v in pairs(m) do  -- injection
    _G[k] = v
end

return m

--
-- Copyright (c) 2009-2012 Francois Perrad
--
-- This library is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--
