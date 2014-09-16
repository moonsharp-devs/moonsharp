using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class UserDataMethodsTests
	{
		public class SomeClass : IComparable
		{
			public StringBuilder Concat(int p1, string p2, IComparable p3, bool p4, List<object> p5, IEnumerable<object> p6, 
				StringBuilder p7, Dictionary<object, object> p8, SomeClass p9, int p10 = 1994)
			{
				p7.Append(p1);
				p7.Append(p2);
				p7.Append(p3);
				p7.Append(p4);

				p7.Append("|");
				foreach (var o in p5) p7.Append(o);
				p7.Append("|");
				foreach (var o in p6) p7.Append(o);
				p7.Append("|");
				foreach (var o in p8.Keys.OrderBy(x => x.ToString())) p7.Append(o);
				p7.Append("|");
				foreach (var o in p8.Values.OrderBy(x => x.ToString())) p7.Append(o);
				p7.Append("|");

				p7.Append(p9);
				p7.Append(p10);

				return p7;
			}

			public override string ToString()
			{
				return "!SOMECLASS!";
			}


			public int CompareTo(object obj)
			{
				throw new NotImplementedException();
			}
		}


		public void Test_ConcatMethod(UserDataOptimizationMode opt)
		{
			string script = @"    
				t = { 'asd', 'qwe', 'zxc', ['x'] = 'X', ['y'] = 'Y' };
				x = myobj.Concat(1, 'ciao', myobj, true, t, t, 'eheh', t, myobj);
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			S.UserDataRepository.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", S.UserDataRepository.CreateUserData(obj));

			DynValue res = S.DoString(script);

			// expected:
			// "eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|XYasdqwezxc|!SOMECLASS!1994";


			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|asdqweXYzxc|!SOMECLASS!1994", res.String);
		}

		[Test]
		public void Interpo_ConcatMethod_None()
		{
			Test_ConcatMethod(UserDataOptimizationMode.None);
		}

	}
}
