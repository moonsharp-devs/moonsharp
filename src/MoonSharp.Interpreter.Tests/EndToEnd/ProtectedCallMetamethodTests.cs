using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class ProtectedCallMetamethodTests
	{
		[Test]
		public void ProtectedCall_TableArgumentsAndMultipleReturns()
		{
			Script.RunString(@"
local t = {}
setmetatable(t, { __call = function(self, ...)
    assert(self == t)
    assert(select('#', ...) == 3)
    local a, b, c = ...
    assert(a == 4 and b == nil and c == 6)
    return a, b, c, nil
end })
local function check(r)
    assert(r.n == 5 and r[1] == true)
    assert(r[2] == 4 and r[3] == nil and r[4] == 6 and r[5] == nil)
end
check(table.pack(pcall(t, 4, nil, 6)))
check(table.pack(xpcall(t, function() error('unexpected handler') end, 4, nil, 6)))
");
		}

		[Test]
		public void ProtectedCall_TableErrorAndHandler()
		{
			Script.RunString(@"
local calls, handled = 0, 0
local t = setmetatable({}, { __call = function()
    calls = calls + 1
    error('hook-error', 0)
end })
local ok, message = pcall(t)
assert(ok == false and string.find(message, 'hook-error', 1, true))
ok, message = xpcall(t, function(e)
    assert(string.find(e, 'hook-error', 1, true))
    handled = handled + 1
    return 'handled'
end)
assert(ok == false and message == 'handled' and handled == 1 and calls == 2)
assert(pcall(function() return 42 end) == true)
");
		}

		[Test]
		public void ProtectedCall_TableYieldsAndResumes()
		{
			Script.RunString(@"
for _, protected in ipairs({
    function(f) return pcall(f) end,
    function(f) return xpcall(f, function() return 'handled' end) end
}) do
    local t = setmetatable({}, { __call = function()
        local v = coroutine.yield('paused')
        return v, 42
    end })
    local co = coroutine.create(function() return protected(t) end)
    local ok, message = coroutine.resume(co)
    assert(ok == true and message == 'paused')
    local resumed, called, a, b = coroutine.resume(co, 'resumed')
    assert(resumed == true and called == true and a == 'resumed' and b == 42)
    assert(coroutine.status(co) == 'dead')
end
");
		}

		[Test]
		public void ProtectedCall_TableErrorsAfterYield()
		{
			Script.RunString(@"
local handled = 0
local t = setmetatable({}, { __call = function()
    coroutine.yield('paused')
    error('after-yield', 0)
end })
local co = coroutine.create(function()
    return xpcall(t, function(e)
        assert(string.find(e, 'after-yield', 1, true))
        handled = handled + 1
        return 'handled'
    end)
end)
local ok, message = coroutine.resume(co)
assert(ok == true and message == 'paused')
local resumed, called, result = coroutine.resume(co)
assert(resumed == true and called == false and result == 'handled' and handled == 1)
assert(coroutine.status(co) == 'dead')
");
		}

		[Test]
		public void ProtectedCall_NestedCallableTables()
		{
			Script.RunString(@"
local inner = setmetatable({}, { __call = function() return 42 end })
local outer = setmetatable({}, { __call = function()
    return xpcall(inner, function() return 'unexpected' end)
end })
local a, b, c = pcall(outer)
assert(a == true and b == true and c == 42)
");
		}

		[Test]
		public void ProtectedCall_OrdinaryFunctionsAndNonCallableValues()
		{
			Script.RunString(@"
local a, b = pcall(function(x) return x end, 42)
assert(a == true and b == 42)
a, b = xpcall(function() error('ordinary', 0) end, function() return 'handled' end)
assert(a == false and b == 'handled')
for _, value in ipairs({ 5, {}, false, 'text', setmetatable({}, { __call = 5 }) }) do
    local ok, message = pcall(value)
    assert(ok == false and type(message) == 'string')
end
");
		}

		[Test]
		public void ProtectedCall_MetamethodLookupIsRaw()
		{
			Script.RunString(@"
local calls = 0
local mt = setmetatable({}, { __index = function()
    calls = calls + 1
    return function() return 42 end
end })
local t = setmetatable({}, mt)
local ok = pcall(t)
assert(ok == false and calls == 0)
");
		}

		[Test]
		public void ProtectedCall_ClrMetamethodReturns()
		{
			Script script = new Script();
			Table table = new Table(script);
			Table meta = new Table(script);
			table.MetaTable = meta;
			meta.Set("__call", DynValue.NewCallback((context, args) =>
			{
				Assert.AreSame(table, args[0].Table);
				Assert.AreEqual(2, args.Count);
				Assert.AreEqual(42, args[1].Number);
				return DynValue.NewTuple(DynValue.NewNumber(42), DynValue.Nil);
			}));
			script.Globals["t"] = table;
			script.DoString(@"
local function check(r)
    assert(r.n == 3 and r[1] == true and r[2] == 42 and r[3] == nil)
end
check(table.pack(pcall(t, 42)))
check(table.pack(xpcall(t, function() return 'unexpected' end, 42)))
");
		}

		[Test]
		public void ProtectedCall_ClrMetamethodErrors()
		{
			Script script = new Script();
			Table table = new Table(script);
			Table meta = new Table(script);
			table.MetaTable = meta;
			meta.Set("__call", DynValue.NewCallback((context, args) =>
			{
				throw new ScriptRuntimeException("clr-hook-error");
			}));
			script.Globals["t"] = table;
			script.DoString(@"
local ok, message = pcall(t)
assert(ok == false and string.find(message, 'clr-hook-error', 1, true))
local handled = 0
ok, message = xpcall(t, function(e)
    assert(string.find(e, 'clr-hook-error', 1, true))
    handled = handled + 1
    return 'handled'
end)
assert(ok == false and message == 'handled' and handled == 1)
");
		}

		[Test]
		public void ProtectedCall_ClrMetamethodTailCall()
		{
			Script script = new Script();
			DynValue target = script.DoString("return function(x) return x, nil end");
			Table table = new Table(script);
			Table meta = new Table(script);
			table.MetaTable = meta;
			meta.Set("__call", DynValue.NewCallback((context, args) =>
			{
				return DynValue.NewTailCallReq(target, new DynValue[] { args[1] });
			}));
			script.Globals["t"] = table;
			script.DoString(@"
local function check(r)
    assert(r.n == 3 and r[1] == true and r[2] == 42 and r[3] == nil)
end
check(table.pack(pcall(t, 42)))
check(table.pack(xpcall(t, function() return 'unexpected' end, 42)))
");
		}
	}
}
