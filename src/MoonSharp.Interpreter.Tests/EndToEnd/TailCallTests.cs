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
			globalCtx["clrtail"] = DynValue.NewCallback((xc, a) =>
			{
				SymbolRef lref = SymbolRef.Global("getResult");
				DynValue fn = xc.GetVar(lref);
				DynValue k3 = DynValue.NewNumber(a[0].Number / 3);

				return DynValue.NewTailCallReq(fn, k3);
			});

			var res = (new Script(globalCtx)).DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(468, res.Number);
		}

	}
}
