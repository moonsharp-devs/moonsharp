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


	}
}
