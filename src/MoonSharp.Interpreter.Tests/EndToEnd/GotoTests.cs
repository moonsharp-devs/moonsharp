using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class GotoTests
	{
		[Test]
		[Ignore]
		public void Goto1()
		{
			string script = @"
				function test()
					x = 3
					goto skip	
					x = x + 2;
					::skip::
					return x;
				end				

				return test();
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}


	}
}
