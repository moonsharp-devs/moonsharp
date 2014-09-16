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


			Script S = new Script();

			S.Globals.Set("clrtail", DynValue.NewCallback((xc, a) =>
			{
				DynValue fn = S.Globals.Get("getResult");
				DynValue k3 = DynValue.NewNumber(a[0].Number / 3);

				return DynValue.NewTailCallReq(fn, k3);
			}));

			var res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(468, res.Number);
		}


		[Test]
		public void CheckToString()
		{
			string script = @"
				return tostring(9)";


			Script S = new Script();
			var res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("9", res.String);
		}

		[Test]
		public void CheckToStringMeta()
		{
			string script = @"
				t = {}
				m = {
					__tostring = function(v)
						return 'ciao';
					end
				}

				setmetatable(t, m);
				s = tostring(t);

				return (s);";


			Script S = new Script();
			var res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("ciao", res.String);
		}
	}
}
