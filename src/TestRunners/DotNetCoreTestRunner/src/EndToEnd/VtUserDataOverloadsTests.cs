using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	public static class VtOverloadsExtMethods
	{
		public static string Method1(this VtUserDataOverloadsTests.OverloadsTestClass obj, string x, bool b)
		{
			return "X" + obj.Method1();
		}

		public static string Method3(this VtUserDataOverloadsTests.OverloadsTestClass obj)
		{
			obj.Method1();
			return "X3";
		}
	}


	[TestFixture]
	public class VtUserDataOverloadsTests
	{
		public struct OverloadsTestClass
		{
			public static void UnCalled()
			{
				var otc = new OverloadsTestClass();
				otc.Method1();
				OverloadsTestClass.Method1(false);
			}

			public string MethodV(string fmt, params object[] args)
			{
				return "varargs:" + string.Format(fmt, args);
			}

			public string MethodV(string fmt, int a, bool b)
			{
				return "exact:" + string.Format(fmt, a, b);
			}

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

			public string Method2(string x, string y)
			{
				return "v";
			}

			public string Method2(string x, ref string y)
			{
				return "r";
			}

			public string Method2(string x, ref string y, int z)
			{
				return "R";
			}
		}

		private void RunTestOverload(string code, string expected, bool tupleExpected = false)
		{
			Script S = new Script();

			OverloadsTestClass obj = new OverloadsTestClass();

			UserData.RegisterType<OverloadsTestClass>();

			S.Globals.Set("s", UserData.CreateStatic<OverloadsTestClass>());
			S.Globals.Set("o", UserData.Create(obj));

			DynValue v = S.DoString("return " + code);

			if (tupleExpected)
			{
				Assert.AreEqual(DataType.Tuple, v.Type);
				v = v.Tuple[0];
			}

			Assert.AreEqual(DataType.String, v.Type);
			Assert.AreEqual(expected, v.String);
		}


		[Test]
		public void VInterop_Overloads_Varargs1()
		{
			RunTestOverload("o:methodV('{0}-{1}', 15, true)", "exact:15-True");
		}

		[Test]
		public void VInterop_Overloads_Varargs2()
		{
			RunTestOverload("o:methodV('{0}-{1}-{2}', 15, true, false)", "varargs:15-True-False");
		}


		[Test]
		public void VInterop_Overloads_ByRef()
		{
			RunTestOverload("o:method2('x', 'y')", "v");
		}

		[Test]
		public void VInterop_Overloads_ByRef2()
		{
			RunTestOverload("o:method2('x', 'y', 5)", "R", true);
		}

		[Test]
		public void VInterop_Overloads_NoParams()
		{
			RunTestOverload("o:method1()", "1");
		}

		[Test]
		public void VInterop_Overloads_NumDowncast()
		{
			RunTestOverload("o:method1(5)", "3");
		}

		[Test]
		public void VInterop_Overloads_NilSelectsNonOptional()
		{
			RunTestOverload("o:method1(5, nil)", "4");
		}

		[Test]
		public void VInterop_Overloads_FullDecl()
		{
			RunTestOverload("o:method1(5, nil, 0)", "5");
		}

		[Test]
		public void VInterop_Overloads_Static1()
		{
			RunTestOverload("s:method1(true)", "s");
		}

		[Test]
		public void VInterop_Overloads_ExtMethods()
		{
			UserData.RegisterExtensionType(typeof(VtOverloadsExtMethods));

			RunTestOverload("o:method1('xx', true)", "X1");
			RunTestOverload("o:method3()", "X3");
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void VInterop_Overloads_ExtMethods2()
		{
			UserData.RegisterExtensionType(typeof(VtOverloadsExtMethods));
			RunTestOverload("s:method3()", "X3");
		}



		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void VInterop_Overloads_Static2()
		{
			// pollute cache
			RunTestOverload("o:method1(5)", "3");
			// exec non static on static
			RunTestOverload("s:method1(5)", "s");
		}

		[Test]
		public void VInterop_Overloads_Cache1()
		{
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5)", "3");
			RunTestOverload("o:method1(5)", "3");
		}

		[Test]
		public void VInterop_Overloads_Cache2()
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
