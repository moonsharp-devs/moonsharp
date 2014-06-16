using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class TailCallTests
	{
		[Test]
		public void TailCallFromCLR()
		{
			string script = @"
				function getResult(x)
					return 156*x;  
				end

				return clrtail(9)";


			var globalCtx = new Table();
			globalCtx[new RValue("clrtail")] = new RValue(new CallbackFunction((xc, a) =>
			{
				LRef lref = LRef.Global("getResult");
				RValue fn = xc.GetVar(lref);
				RValue k3 = new RValue(a[0].Number / 3);

				return new RValue(fn, k3);
			}));

			var res = MoonSharpInterpreter.LoadFromString(script).Execute(globalCtx);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(468, res.Number);
		}

	}
}
