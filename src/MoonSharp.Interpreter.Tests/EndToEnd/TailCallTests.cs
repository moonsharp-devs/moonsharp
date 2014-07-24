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
		[Ignore]
		public void TailCallFromCLR()
		{
//			string script = @"
//				function getResult(x)
//					return 156*x;  
//				end
//
//				return clrtail(9)";


//			Script S = new Script();

//			S.Globals["clrtail"] = DynValue.NewCallback((xc, a) =>
//			{
//				SymbolRef lref = SymbolRef.Global("getResult");
//				DynValue fn = xc.GetVar(lref);
//				DynValue k3 = DynValue.NewNumber(a[0].Number / 3);

//				return DynValue.NewTailCallReq(fn, k3);
//			});

//			var res = S.DoString(script);

//			Assert.AreEqual(DataType.Number, res.Type);
//			Assert.AreEqual(468, res.Number);
		}

	}
}
