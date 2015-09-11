using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class VarargsTupleTests
	{

		private void DoTest(string code, string expectedResult)
		{
			Script S = new Script();
			
			S.DoString(@"
function f(a,b)
	local debug = 'a: ' .. tostring(a) .. ' b: ' .. tostring(b)
	return debug
end


function g(a, b, ...)
	local debug = 'a: ' .. tostring(a) .. ' b: ' .. tostring(b)
	local arg = {...}
	debug = debug .. ' arg: {'
	for k, v in pairs(arg) do
		debug = debug .. tostring(v) .. ', '
	end
	debug = debug .. '}'
	return debug
end

function r()
	return 1, 2, 3
end

function h(...)
	return g(...)
end

function i(...)
	return g('extra', ...)
end
");
			DynValue res = S.DoString("return " + code);

			Assert.AreEqual(res.Type, DataType.String);
			Assert.AreEqual(expectedResult, res.String);
		}

		[Test]
		public void VarArgsTuple_Basic()
		{
			DoTest("f(3)", "a: 3 b: nil");
			DoTest("f(3,4)", "a: 3 b: 4");
			DoTest("f(3,4,5)", "a: 3 b: 4");
			DoTest("f(r(),10)", "a: 1 b: 10");
			DoTest("f(r())", "a: 1 b: 2");
		}

		[Test]
		public void VarArgsTuple_Intermediate()
		{
			DoTest("g(3)      	", "a: 3 b: nil arg: {}");
			DoTest("g(3,4)    	", "a: 3 b: 4 arg: {}");
			DoTest("g(3,4,5,8)	", "a: 3 b: 4 arg: {5, 8, }");
			DoTest("g(5,r())  	", "a: 5 b: 1 arg: {2, 3, }");
		}

		[Test]
		public void VarArgsTuple_Advanced()
		{
			//DoTest("h(3)      	", "a: 3 b: nil arg: {}");
			//DoTest("h(3,4)    	", "a: 3 b: 4 arg: {}");
			//DoTest("h(3,4,5,8)	", "a: 3 b: 4 arg: {5, }");
			DoTest("h(5,r())  	", "a: 5 b: 1 arg: {2, 3, }");
		}

		[Test]
		public void VarArgsTuple_Advanced2()
		{
			DoTest("i(3)      	", "a: extra b: 3 arg: {}");
			DoTest("i(3,4)    	", "a: extra b: 3 arg: {4, }");
			DoTest("i(3,4,5,8)	", "a: extra b: 3 arg: {4, 5, 8, }");
			DoTest("i(5,r())  	", "a: extra b: 5 arg: {1, 2, 3, }");
		}

		[Test]
		public void VarArgsTuple_DontCrash()
		{
			string script = @"
				function Obj(...)
					do
						local args = { ... }
					end
				end
				Obj(1)
			";

			Script S = new Script(CoreModules.None);
				
			S.DoString(script);

		}

	}
}
