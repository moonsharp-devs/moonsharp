using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class UserDataOverloadsTests
	{
		public class OverloadsTestClass
		{
			public string Method1()
			{
				return "1";
			}

			public static string Method1(bool b)
			{
				return "s";
			}

			public string Method1(int a)
			{
				return "2";
			}

			public string Method1(double d)
			{
				return "3";
			}

			public string Method1(double d, string x = null)
			{
				return "4";
			}

			public string Method1(double d, string x, int y = 5)
			{
				return "5";
			}
		}

		private void RunTestOverload(string code, string expected)
		{
			Script S = new Script();

			OverloadsTestClass obj = new OverloadsTestClass();

			UserData.RegisterType<OverloadsTestClass>();

			S.Globals.Set("s", UserData.CreateStatic<OverloadsTestClass>());
			S.Globals.Set("o", UserData.Create(obj));

			DynValue v = S.DoString("return " + code);
			Assert.AreEqual(DataType.String, v.Type);
			Assert.AreEqual(expected, v.String);
		}


		[Test]
		public void Interop_Overloads_NoParams()
		{
			RunTestOverload("o:method1()", "1");
		}

		[Test]
		public void Interop_Overloads_NumDowncast()
		{
			RunTestOverload("o:method1(5)", "3");
		}

		[Test]
		public void Interop_Overloads_NilSelectsNonOptional()
		{
			RunTestOverload("o:method1(5, nil)", "4");
		}

		[Test]
		public void Interop_Overloads_FullDecl()
		{
			RunTestOverload("o:method1(5, nil, 0)", "5");
		}

		[Test]
		public void Interop_Overloads_Static1()
		{
			RunTestOverload("s:method1(true)", "s");
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_Overloads_Static2()
		{
			// pollute cache
			RunTestOverload("o:method1(5)", "3");
			// exec non static on static
			RunTestOverload("s:method1(5)", "s");
		}

		[Test]
		public void Interop_Overloads_Cache1()
		{
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5)", "3");
		}

		[Test]
		public void Interop_Overloads_Cache2()
		{
			RunTestOverload("o:method1()", "1");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5, nil)", "4");
			RunTestOverload("o:method1(5, nil, 0)", "5");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("s:method1(true)", "s");
			RunTestOverload("o:method1(5, nil, 0)", "5");
			RunTestOverload("o:method1(5, 'x')", "4");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5, 'x', 0)", "5");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5, nil, 0)", "5");
			RunTestOverload("s:method1(true)", "s");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5, 5)", "4");
			RunTestOverload("o:method1(5, nil, 0)", "5");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("s:method1(true)", "s");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5, 5, 0)", "5");
			RunTestOverload("s:method1(true)", "s");
		}

	



	}
}
