using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	class CoroutineTests
	{
		[Test]
		public void Coroutine_Basic()
		{
			string script = @"
				s = ''

				function foo()
					for i = 1, 4 do
						s = s .. i;
						coroutine.yield();
					end
				end

				function bar()
					for i = 5, 9 do
						s = s .. i;
						coroutine.yield();
					end
				end

				cf = coroutine.create(foo);
				cb = coroutine.create(bar);

				for i = 1, 4 do
					coroutine.resume(cf);
					s = s .. '-';
					coroutine.resume(cb);
					s = s .. ';';
				end

				return s;
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1-5;2-6;3-7;4-8;", res.String);
		}

		[Test]
		public void Coroutine_Wrap()
		{
			string script = @"
				s = ''

				function foo()
					for i = 1, 4 do
						s = s .. i;
						coroutine.yield();
					end
				end

				function bar()
					for i = 5, 9 do
						s = s .. i;
						coroutine.yield();
					end
				end

				cf = coroutine.wrap(foo);
				cb = coroutine.wrap(bar);

				for i = 1, 4 do
					cf();
					s = s .. '-';
					cb();
					s = s .. ';';
				end

				return s;
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1-5;2-6;3-7;4-8;", res.String);
		}

		[Test]
		public void Coroutine_ClrBoundaryHandling()
		{
			string code = @"
				function a()
					callback(b)
				end

				function b()
					coroutine.yield();
				end						

				c = coroutine.create(a);

				return coroutine.resume(c);		
				";

			// Load the code and get the returned function
			Script script = new Script();

			script.Globals["callback"] = DynValue.NewCallback(
				(ctx, args) => args[0].Function.Call()
				);

			DynValue ret = script.DoString(code);

			Assert.AreEqual(DataType.Tuple, ret.Type);
			Assert.AreEqual(2, ret.Tuple.Length);
			Assert.AreEqual(DataType.Boolean, ret.Tuple[0].Type);
			Assert.AreEqual(false, ret.Tuple[0].Boolean);
			Assert.AreEqual(DataType.String, ret.Tuple[1].Type);
			Assert.IsTrue(ret.Tuple[1].String.EndsWith("attempt to yield across a CLR-call boundary"));
		}

		[Test]
		public void Coroutine_VariousErrorHandling()
		{
			string last = "";
			string code = @"

function checkresume(step, ex, ey)
	local x, y = coroutine.resume(c)
	
	assert(x == ex, 'Step ' .. step .. ': ' .. tostring(ex) .. ' was expected, got ' .. tostring(x));
	assert(y:endsWith(ey), 'Step ' .. step .. ': ' .. tostring(ey) .. ' was expected, got ' .. tostring(y));
end


t = { }
m = { __tostring = function() print('2'); coroutine.yield(); print('3'); end }

setmetatable(t, m);


function a()
	checkresume(1, false, 'cannot resume non-suspended coroutine');
	coroutine.yield('ok');
	print(t);
	coroutine.yield('ok'); 
end

c = coroutine.create(a);

checkresume(2, true, 'ok');
checkresume(3, false, 'attempt to yield across a CLR-call boundary');
checkresume(4, false, 'cannot resume dead coroutine');
checkresume(5, false, 'cannot resume dead coroutine');
checkresume(6, false, 'cannot resume dead coroutine');

				";

			// Load the code and get the returned function
			Script script = new Script();

			script.Options.DebugPrint = (s) => last = s;

			script.DoString(code);

			Assert.AreEqual(last, "2");
		}

		[Test]
		public void Coroutine_Direct_Resume()
		{
			string code = @"
				return function()
					local x = 0
					while true do
						x = x + 1
						coroutine.yield(x)
						if (x > 5) then
							return 7
						end
					end
				end
				";

			// Load the code and get the returned function
			Script script = new Script();
			DynValue function = script.DoString(code);

			// Create the coroutine in C#
			DynValue coroutine = script.CreateCoroutine(function);

			// Loop the coroutine 
			string ret = "";
			while (coroutine.Coroutine.State != CoroutineState.Dead)
			{
				DynValue x = coroutine.Coroutine.Resume();
				ret = ret + x.ToString();
			}

			Assert.AreEqual("1234567", ret);
		}


		[Test]
		public void Coroutine_Direct_AsEnumerable()
		{
			string code = @"
				return function()
					local x = 0
					while true do
						x = x + 1
						coroutine.yield(x)
						if (x > 5) then
							return 7
						end
					end
				end
				";

			// Load the code and get the returned function
			Script script = new Script();
			DynValue function = script.DoString(code);

			// Create the coroutine in C#
			DynValue coroutine = script.CreateCoroutine(function);

			// Loop the coroutine 
			string ret = "";

			foreach (DynValue x in coroutine.Coroutine.AsTypedEnumerable())
			{
				ret = ret + x.ToString();
			}

			Assert.AreEqual("1234567", ret);
		}


		[Test]
		public void Coroutine_AutoYield()
		{
			string code = @"
				function fib(n)
					if (n == 0 or n == 1) then
						return 1;
					else
						return fib(n - 1) + fib(n - 2);
					end
				end
				";

			// Load the code and get the returned function
			Script script = new Script(CoreModules.None);
			script.DoString(code);

			// get the function
			DynValue function = script.Globals.Get("fib");

			// Create the coroutine in C#
			DynValue coroutine = script.CreateCoroutine(function);

			// Set the automatic yield counter every 10 instructions. 
			// 10 is a too small! Use a much bigger value in your code to avoid interrupting too often!
			coroutine.Coroutine.AutoYieldCounter = 10;

			int cycles = 0;
			DynValue result = null;

			// Cycle until we get that the coroutine has returned something useful and not an automatic yield..
			for (result = coroutine.Coroutine.Resume(8); 
				result.Type == DataType.YieldRequest;
				result = coroutine.Coroutine.Resume()) 
			{
				cycles += 1;
			}

			// Check the values of the operation
			Assert.AreEqual(DataType.Number, result.Type);
			Assert.AreEqual(34, result.Number);

			// Check the autoyield actually triggered
			Assert.IsTrue(cycles > 10);
		}






	}
}
