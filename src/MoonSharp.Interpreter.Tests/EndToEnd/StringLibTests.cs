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
			Utils.DynAssert(res, 10, 11, null);
		}

		[Test]
		public void String_Find_7()
		{
			string script = @"return string.find('Hello Lua user', '%su', 1);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, 10, 11, null);
		}

		[Test]
		public void String_Find_8()
		{
			string script = @"return string.find('Hello Lua user', '%su', 1, true);";
			DynValue res = Script.RunString(script);
			Utils.DynAssert(res, null);
		}

	}
}
