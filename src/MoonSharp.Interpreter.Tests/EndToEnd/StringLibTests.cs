using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class StringLibTests
	{
		[Test]
		public void String_GMatch_1()
		{
			string script = @"    
				t = '';

				for word in string.gmatch('Hello Lua user', '%a+') do 
					t = t .. word;
				end

				return (t);
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("HelloLuauser", res.String);
		}

		[Test]
		public void String_Find_1()
		{
			string script = @"return string.find('Hello Lua user', 'Lua');";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, 7, 9);
		}

		[Test]
		public void String_Find_2()
		{
			string script = @"return string.find('Hello Lua user', 'banana');";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, null);
		}

		[Test]
		public void String_Find_3()
		{
			string script = @"return string.find('Hello Lua user', 'Lua', 1);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, 7, 9);
		}

		[Test]
		public void String_Find_4()
		{
			string script = @"return string.find('Hello Lua user', 'Lua', 8);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, null);
		}

		[Test]
		public void String_Find_5()
		{
			string script = @"return string.find('Hello Lua user', 'e', -5);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, 13, 13);
		}

		[Test]
		public void String_Find_6()
		{
			string script = @"return string.find('Hello Lua user', '%su');";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, 10, 11);
		}

		[Test]
		public void String_Find_7()
		{
			string script = @"return string.find('Hello Lua user', '%su', 1);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, 10, 11);
		}

		[Test]
		public void String_Find_8()
		{
			string script = @"return string.find('Hello Lua user', '%su', 1, true);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, null);
		}
		[Test]
		public void String_Find_9()
		{
			string script = @"
				s = 'Deadline is 30/05/1999, firm'
				date = '%d%d/%d%d/%d%d%d%d';
				return s:sub(s:find(date));
			";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, "30/05/1999");
		}

		[Test]
		public void String_Find_10()
		{
			string script = @"
				s = 'Deadline is 30/05/1999, firm'
				date = '%f[%S]%d%d/%d%d/%d%d%d%d';
				return s:sub(s:find(date));
			";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, "30/05/1999");
		}

		[Test]
		public void String_Find_11()
		{
			string script = @"
				s = 'Deadline is 30/05/1999, firm'
				date = '%f[%s]%d%d/%d%d/%d%d%d%d';
				return s:find(date);
			";
			DynValue res = Script.RunString(script);
			Assert.IsTrue(res.IsNil());
		}

		[Test]
		public void String_Format_1()
		{
			string script = @"
				d = 5; m = 11; y = 1990
				return string.format('%02d/%02d/%04d', d, m, y)
			";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, "05/11/1990");
		}

		[Test]
		public void String_GSub_1()
		{
			string script = @"
				s = string.gsub('hello world', '(%w+)', '%1 %1')
				return s, s == 'hello hello world world'
			";
			DynValue res = Script.RunString(script);
			Assert.AreEqual(res.Tuple[0].String, "hello hello world world");
			Assert.AreEqual(res.Tuple[1].Boolean, true);
		}

		[Test]
		public void PrintTest1()
		{
			string script = @"
				print('ciao', 1);
			";
			string printed = null;

			Script S = new Script();
			DynValue main = S.LoadString(script);

			S.Options.DebugPrint = s =>
			{
				printed = s;
			};

			S.Call(main);

			Assert.AreEqual("ciao\t1", printed);
		}

		[Test]
		public void PrintTest2()
		{
			string script = @"
				t = {};
				m = {};

				function m.__tostring()
					return 'ciao';
				end

				setmetatable(t, m);

				print(t, 1);
			";
			string printed = null;

			Script S = new Script();
			DynValue main = S.LoadString(script);

			S.Options.DebugPrint = s =>
			{
				printed = s;
			};

			S.Call(main);

			Assert.AreEqual("ciao\t1", printed);
		}

		[Test]
		public void ToStringTest()
		{
			string script = @"
				t = {}
				mt = {}
				a = nil
				function mt.__tostring () a = 'yup' end
				setmetatable(t, mt)
				return tostring(t), a;
			";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, DataType.Void, "yup");
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void String_GSub_2()
		{
			string script = @"
				string.gsub('hello world', '%w+', '%e')
			";
			DynValue res = Script.RunString(script);
		}

		[Test]
		public void String_GSub_3()
		{
			Script S = new Script();
			S.Globals["a"] = @"                  'C:\temp\test.lua:68: bad argument #1 to 'date' (invalid conversion specifier '%Ja')'
    doesn't match '^[^:]+:%d+: bad argument #1 to 'date' %(invalid conversion specifier '%%Ja'%)'";

			string script = @"
				string.gsub(a, '\n', '\n #')
			";
			DynValue res = S.DoString(script);
		}

		[Test]
		public void String_Match_1()
		{
			string s = @"test.lua:185: field 'day' missing in date table";
			string p = @"^[^:]+:%d+: field 'day' missing in date table";

			TestMatch(s, p, true);
		}

		private void TestMatch(string s, string p, bool expected)
		{
			Script S = new Script(CoreModules.String);
			S.Globals["s"] = s;
			S.Globals["p"] = p;
			DynValue res = S.DoString("return string.match(s, p)");

			Assert.AreEqual(expected, !res.IsNil());
		}

	}
}
