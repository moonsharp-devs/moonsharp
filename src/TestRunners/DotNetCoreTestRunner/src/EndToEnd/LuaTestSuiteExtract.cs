using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	/// <summary>
	/// Selected tests extracted from Lua test suite
	/// </summary>
	[TestFixture]
	class LuaTestSuiteExtract
	{
		void RunTest(string script)
		{
			HashSet<string> failedTests = new HashSet<string>();
			int i = 0;

			Script S = new Script();

			var globalCtx = S.Globals;
			globalCtx.Set(DynValue.NewString("xassert"), DynValue.NewCallback(new CallbackFunction(
				(x, a) =>
				{
					if (!a[1].CastToBool())
						failedTests.Add(a[0].String);

					return DynValue.Nil;
				})));
			globalCtx.Set(DynValue.NewString("assert"), DynValue.NewCallback(new CallbackFunction(
				(x, a) =>
				{
					++i;

					if (!a[0].CastToBool())
						failedTests.Add(string.Format("assert #{0}", i));

					return DynValue.Nil;
				})));

			globalCtx.Set(DynValue.NewString("print"), DynValue.NewCallback(new CallbackFunction((x, a) =>
			{
				// Debug.WriteLine(string.Join(" ", a.Select(v => v.AsString()).ToArray()));
				return DynValue.Nil;
			})));


			DynValue res = S.DoString(script);

			Assert.IsFalse(failedTests.Any(), string.Format("Failed asserts {0}",
				string.Join(", ", failedTests.Select(xi => xi.ToString()).ToArray())));
		}

		[Test]
		public void LuaSuite_Calls_LocalFunctionRecursion()
		{
			RunTest(@"
				-- testing local-function recursion
				fact = false
				do
				  local res = 1
				  local function fact (n)
					if n==0 then return res
					else return n*fact(n-1)
					end
				  end
				  xassert('fact(5) == 120', fact(5) == 120)
				end
				xassert('fact == false', fact == false)
				");
		}

		[Test]
		public void LuaSuite_Calls_Declarations()
		{
			RunTest(@"
				-- testing local-function recursion
				-- testing declarations
				a = {i = 10}
				self = 20
				function a:x (x) return x+self.i end
				function a.y (x) return x+self end

				xassert('a:x(1)+10 == a.y(1)', a:x(1)+10 == a.y(1))

				a.t = {i=-100}
				a['t'].x = function (self, a,b) return self.i+a+b end

				xassert('a.t:x(2,3) == -95', a.t:x(2,3) == -95)

				do
				  local a = {x=0}
				  function a:add (x) self.x, a.y = self.x+x, 20; return self end
				  xassert('a:add(10):add(20):add(30).x == 60 and a.y == 20', a:add(10):add(20):add(30).x == 60 and a.y == 20)
				end

				local a = {b={c={}}}

				function a.b.c.f1 (x) return x+1 end
				function a.b.c:f2 (x,y) self[x] = y end
				xassert('a.b.c.f1(4) == 5', a.b.c.f1(4) == 5)
				a.b.c:f2('k', 12); xassert('a.b.c.k == 12', a.b.c.k == 12)

				print('+')

				t = nil   -- 'declare' t
				function f(a,b,c) local d = 'a'; t={a,b,c,d} end

				f(      -- this line change must be valid
				  1,2)
				xassert('missingparam', t[1] == 1 and t[2] == 2 and t[3] == nil and t[4] == 'a')
				f(1,2,   -- this one too
					  3,4)
				xassert('extraparam', t[1] == 1 and t[2] == 2 and t[3] == 3 and t[4] == 'a')

				");
		}



		[Test]
		public void LuaSuite_Calls_Closures()
		{
			RunTest(@"
				-- fixed-point operator
				Z = function (le)
					  local function a (f)
						return le(function (x) return f(f)(x) end)
					  end
					  return a(a)
					end


				-- non-recursive factorial

				F = function (f)
					  return function (n)
							   if n == 0 then return 1
							   else return n*f(n-1) end
							 end
					end

				fat = Z(F)

				xassert('fat(0) == 1 and fat(4) == 24 and Z(F)(5)==5*Z(F)(4)', fat(0) == 1 and fat(4) == 24 and Z(F)(5)==5*Z(F)(4))

				local function g (z)
				  local function f (a,b,c,d)
					return function (x,y) return a+b+c+d+a+x+y+z end
				  end
				  return f(z,z+1,z+2,z+3)
				end

				f = g(10)

				xassert('f(9, 16) == 10+11+12+13+10+9+16+10', f(9, 16) == 10+11+12+13+10+9+16+10)

				Z, F, f = nil
				--print('+')
				");
		}





	}
}
