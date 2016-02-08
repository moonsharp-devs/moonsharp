using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class BinaryDumpTests
	{
		private DynValue Script_RunString(string script)
		{
			Script s1 = new Script();
			DynValue v1 = s1.LoadString(script);

			using (MemoryStream ms = new MemoryStream())
			{
				s1.Dump(v1, ms);
				ms.Seek(0, SeekOrigin.Begin);

				Script s2 = new Script();
				DynValue func = s2.LoadStream(ms);
				return func.Function.Call();
			}
		}

		private DynValue Script_LoadFunc(string script, string funcname)
		{
			Script s1 = new Script();
			DynValue v1 = s1.DoString(script);
			DynValue func = s1.Globals.Get(funcname);

			using (MemoryStream ms = new MemoryStream())
			{
				s1.Dump(func, ms);
				ms.Seek(0, SeekOrigin.Begin);

				Script s2 = new Script();
				return s2.LoadStream(ms);
			}
		}


		[Test]
		public void BinDump_ChunkDump()
		{
			string script = @"
				local chunk = load('return 81;');
				local str = string.dump(chunk);
				local fn = load(str);
				return fn(9);
			";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(81, res.Number);
		}

		[Test]
		public void BinDump_StringDump()
		{
			string script = @"
				local str = string.dump(function(n) return n * n; end);
				local fn = load(str);
				return fn(9);
			";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(81, res.Number);
		}

		[Test]
		public void BinDump_StandardDumpFunc()
		{
			string script = @"
				function fact(n)
					return n * 24;
				end

				local str = string.dump(fact);
				
			";

			DynValue fact = Script_LoadFunc(script, "fact");
			DynValue res = fact.Function.Call(5);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120, res.Number);
		}



		[Test]
		public void BinDump_FactorialDumpFunc()
		{
			string script = @"
				function fact(n)
					if (n == 0) then return 1; end
					return fact(n - 1) * n;
				end
			";

			DynValue fact = Script_LoadFunc(script, "fact");
			fact.Function.OwnerScript.Globals.Set("fact", fact);
			DynValue res = fact.Function.Call(5);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120, res.Number);
		}

		[Test]
		public void BinDump_FactorialDumpFuncGlobal()
		{
			string script = @"
				x = 0

				function fact(n)
					if (n == x) then return 1; end
					return fact(n - 1) * n;
				end
			";

			DynValue fact = Script_LoadFunc(script, "fact");
			fact.Function.OwnerScript.Globals.Set("fact", fact);
			fact.Function.OwnerScript.Globals.Set("x", DynValue.NewNumber(0));
			DynValue res = fact.Function.Call(5);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120, res.Number);
		}


		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BinDump_FactorialDumpFuncUpvalue()
		{
			string script = @"
				local x = 0

				function fact(n)
					if (n == x) then return 1; end
					return fact(n - 1) * n;
				end
			";

			DynValue fact = Script_LoadFunc(script, "fact");
			fact.Function.OwnerScript.Globals.Set("fact", fact);
			fact.Function.OwnerScript.Globals.Set("x", DynValue.NewNumber(0));
			DynValue res = fact.Function.Call(5);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120, res.Number);
		}























		[Test]
		public void BinDump_FactorialClosure()
		{
			string script = @"
local x = 5;

function fact(n)
	if (n == x) then return 1; end
	return fact(n - 1) * n;
end

x = 0;

y = fact(5);

x = 3;

y = y + fact(5);

return y;
";

			DynValue res = Script_RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(140, res.Number);
		}

		[Test]
		public void BinDump_ClosureOnParam()
		{
			string script = @"
				local function g (z)
				  local function f(a)
					return a + z;
				  end
				  return f;
				end

				return (g(3)(2));";

			DynValue res = Script_RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(5, res.Number);
		}

		[Test]
		public void BinDump_NestedUpvalues()
		{
			string script = @"
	local y = y;

	local x = 0;
	local m = { };

	function m:a()
		self.t = {
			dojob = function() 
				if (x == 0) then return 1; else return 0; end
			end,
		};
	end

	m:a();

	return 10 * m.t.dojob();
								";

			DynValue res = Script_RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}


		[Test]
		public void BinDump_NestedOutOfScopeUpvalues()
		{
			string script = @"

	function X()
		local y = y;

		local x = 0;
		local m = { };

		function m:a()
			self.t = {
				dojob = function() 
					if (x == 0) then return 1; else return 0; end
				end,
			};
		end

		return m;
	end

	Q = X();

	Q:a();

	return 10 * Q.t.dojob();
								";

			DynValue res = Script_RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}


		[Test]
		public void Load_ChangeEnvWithDebugSetUpvalue()
		{
			List<Table> list = new List<Table>();

			string script = @"
				function print_env()
				  print(_ENV)
				end

				function sandbox()
				  print(_ENV) -- prints: 'table: 0x100100610'
				  -- need to keep access to a few globals:
				  _ENV = { print = print, print_env = print_env, debug = debug, load = load }
				  print(_ENV) -- prints: 'table: 0x100105140'
				  print_env() -- prints: 'table: 0x100105140'
				  local code1 = load('print(_ENV)')
				  code1()     -- prints: 'table: 0x100100610'
				  debug.setupvalue(code1, 0, _ENV) -- set our modified env
				  debug.setupvalue(code1, 1, _ENV) -- set our modified env
				  code1()     -- prints: 'table: 0x100105140'
				  local code2 = load('print(_ENV)', nil, nil, _ENV) -- pass 'env' arg
				  code2()     -- prints: 'table: 0x100105140'
				end

				sandbox()";

			Script S = new Script(CoreModules.Preset_Complete);

			S.Globals["print"] = (Action<Table>)(t => list.Add(t));

			S.DoString(script);

			Assert.AreEqual(6, list.Count);

			int[] eqs = new int[] { 0, 1, 1, 0, 1, 1 };

			for (int i = 0; i < 6; i++)
				Assert.AreEqual(list[eqs[i]], list[i]);
		}
	}
}
