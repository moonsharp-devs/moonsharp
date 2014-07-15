
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--

local debug = nil; -- require 'debug'
local io = nil; -- require 'io'
local os = nil; -- require 'os'
local error = error
local gsub = require 'string'.gsub
local match = require 'string'.match
local pairs = pairs
local pcall = pcall
local print = print
local rawget = rawget
local setmetatable = setmetatable
local tconcat = require 'table'.concat
local tonumber = tonumber
local tostring = tostring
local type = type

_ENV = nil
local m = {}

local testout = io and io.stdout
local testerr = io and (io.stderr or io.stdout)

function m.puts (f, str)
    f:write(str)
end

local function _print_to_fh (self, f, ...)
   -- if f then
   --     local msg = tconcat({...})
   --     gsub(msg, "\n", "\n" .. self.indent)
   --     m.puts(f, self.indent .. msg .. "\n")
   -- else
        print(self.indent, ...)
   -- end
end

local function _print (self, ...)
    _print_to_fh(self, self:output(), ...)
end

local function print_comment (self, f, ...)
    local arg = {...}
    for k, v in pairs(arg) do
        arg[k] = tostring(v)
    end
    local msg = tconcat(arg)
    msg = gsub(msg, "\n", "\n# ")
    msg = gsub(msg, "\n# \n", "\n#\n")
    msg = gsub(msg, "\n# $", '')
    _print_to_fh(self, f, "# ", msg)
end

function m.create ()
    local o = {
        data = setmetatable({}, { __index = m }),
    }
    setmetatable(o, {
        __index = function (t, k)
                        return rawget(t, 'data')[k]
                  end,
        __newindex = function (t, k, v)
                        rawget(o, 'data')[k] = v
                  end,
    })
    o:reset()
    return o
end

local test
function m.new ()
    test = test or m.create()
    return test
end

local function in_todo (self)
    return self.todo_upto >= self.curr_test
end

function m:child (name)
    if self.child_name then
        error("You already have a child named (" .. self.child_name .. " running")
    end
    local child = m.create()
    child.indent    = self.indent .. '    '
    child.out_file  = self.out_file
    child.fail_file = in_todo(self) and self.todo_file or self.fail_file
    child.todo_file = self.todo_file
    child.parent    = self
    self.child_name = name
    return child
end

local function plan_handled (self)
    return self.have_plan or self.no_plan or self._skip_all
end

function m:subtest (name, func)
    if type(func) ~= 'function' then
        error("subtest()'s second argument must be a function")
    end
    self:diag('Subtest: ' .. name)
    local child = self:child(name)
    local parent = self.data
    self.data = child.data
    local r, msg = pcall(func)
    child.data = self.data
    self.data = parent
    if not r and not child._skip_all then
        error(msg, 0)
    end
    if not plan_handled(child) then
        child:done_testing()
    end
    child:finalize()
end

function m:finalize ()
    if not self.parent then
        return
    end
    if self.child_name then
        error("Can't call finalize() with child (" .. self.child_name .. " active")
    end
    local parent = self.parent
    local name = parent.child_name
    parent.child_name = nil
    if self._skip_all then
        parent:skip(self._skip_all)
    elseif self.curr_test == 0 then
        parent:ok(false, "No tests run for subtest \"" .. name .. "\"", 2)
    else
        parent:ok(self.is_passing, name, 2)
    end
    self.parent = nil
end

function m:reset ()
    self.curr_test = 0
    self._done_testing = false
    self.expected_tests = 0
    self.is_passing = true
    self.todo_upto = -1
    self.todo_reason = nil
    self.have_plan = false
    self.no_plan = false
    self._skip_all = false
    self.have_output_plan = false
    self.indent = ''
    self.parent = false
    self.child_name = false
    self:reset_outputs()
end

local function _output_plan (self, max, directive, reason)
    if self.have_output_plan then
        error("The plan was already output")
    end
    local out = "1.." .. max
    if directive then
        out = out .. " # " .. directive
    end
    if reason then
        out = out .. " " .. reason
    end
    _print(self, out)
    self.have_output_plan = true
end

function m:plan (arg)
    if self.have_plan then
        error("You tried to plan twice")
    end
    if type(arg) == 'string' and arg == 'no_plan' then
        self.have_plan = true
        self.no_plan = true
        return true
    elseif type(arg) ~= 'number' then
        error("Need a number of tests")
    elseif arg < 0 then
        error("Number of tests must be a positive integer.  You gave it '" .. arg .."'.")
    else
        self.expected_tests = arg
        self.have_plan = true
        _output_plan(self, arg)
        return arg
    end
end

function m:done_testing (num_tests)
    if num_tests then
        self.no_plan = false
    end
    num_tests = num_tests or self.curr_test
    if self._done_testing then
        tb:ok(false, "done_testing() was already called")
        return
    end
    self._done_testing = true
    if self.expected_tests > 0 and num_tests ~= self.expected_tests then
        self:ok(false, "planned to run " .. self.expected_tests
                    .. " but done_testing() expects " .. num_tests)
    else
        self.expected_tests = num_tests
    end
    if not self.have_output_plan then
        _output_plan(self, num_tests)
    end
    self.have_plan = true
    -- The wrong number of tests were run
    if self.expected_tests ~= self.curr_test then
        self.is_passing = false
    end
    -- No tests were run
    if self.curr_test == 0 then
        self.is_passing = false
    end
end

function m:has_plan ()
    if self.expected_tests > 0 then
        return self.expected_tests
    end
    if self.no_plan then
        return 'no_plan'
    end
    return nil
end

function m:skip_all (reason)
    if self.have_plan then
        error("You tried to plan twice")
    end
    self._skip_all = reason
    _output_plan(self, 0, 'SKIP', reason)
    if self.parent then
        error("skip_all in child", 0)
    end
    -- os.exit(0)
end

local function _check_is_passing_plan (self)
    local plan = self:has_plan()
    if not plan or not tonumber(plan) then
        return
    end
    if plan < self.curr_test then
        self.is_passing = false
    end
end

function m:ok (test, name, level)
    if self.child_name then
        name = name or 'unnamed test'
        self.is_passing = false
        error("Cannot run test (" .. name .. ") with active children")
    end
    name = name or ''
    level = level or 0
    self.curr_test = self.curr_test + 1
    name = tostring(name)
    if match(name, '^[%d%s]+$') then
        self:diag("    You named your test '" .. name .."'.  You shouldn't use numbers for your test names."
        .. "\n    Very confusing.")
    end
    local out = ''
    if not test then
        out = "not "
    end
    out = out .. "ok " .. self.curr_test
    if name ~= '' then
        out = out .. " - " .. name
    end
    if self.todo_reason and in_todo(self) then
        out = out .. " # TODO " .. self.todo_reason
    end
    _print(self, out)
    if not test then
        local msg = in_todo(self) and "Failed (TODO)" or "Failed"
        local info = debug and debug.getinfo(3 + level)
        if info then
            local file = info.short_src
            local line = info.currentline
            self:diag("    " .. msg .. " test (" .. file .. " at line " .. line .. ")")
        else
            self:diag("    " .. msg .. " test")
        end
    end
    if not test and not in_todo(self) then
        self.is_passing = false
    end
    _check_is_passing_plan(self)
end

function m:BAIL_OUT (reason)
    local out = "Bail out!"
    if reason then
        out = out .. "  " .. reason
    end
    _print(self, out)
    os.exit(255)
end

function m:current_test (num)
    if num then
        self.curr_test = num
    end
    return self.curr_test
end

function m:todo (reason, count)
    count = count or 1
    self.todo_upto = self.curr_test + count
    self.todo_reason = reason
end

function m:skip (reason)
    local name = "# skip"
    if reason then
        name = name .. " " .. reason
    end
    self:ok(true, name, 1)
end

function m:todo_skip (reason)
    local name = "# TODO & SKIP"
    if reason then
        name = name .. " " .. reason
    end
    self:ok(false, name, 1)
end

function m:skip_rest (reason)
    for i = self.curr_test, self.expected_tests do
        tb:skip(reason)
    end
end

local function diag_file (self)
    if in_todo(self) then
        return self:todo_output()
    else
        return self:failure_output()
    end
end

function m:diag (...)
    print_comment(self, diag_file(self), ...)
end

function m:note (...)
    print_comment(self, self:output(), ...)
end

function m:output (f)
    if f then
        self.out_file = f
    end
    return self.out_file
end

function m:failure_output (f)
    if f then
        self.fail_file = f
    end
    return self.fail_file
end

function m:todo_output (f)
    if f then
        self.todo_file = f
    end
    return self.todo_file
end

function m:reset_outputs ()
    self:output(testout)
    self:failure_output(testerr)
    self:todo_output(testout)
end

return m
--
-- Copyright (c) 2009-2012 Francois Perrad
--
-- This library is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--
