using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	public enum MyEnum : short
	{
		Uno = 1,
		MenoUno = -1,
		Quattro = 4,
		Cinque = 5,
		TantaRoba = short.MaxValue,
		PocaRoba = short.MinValue,
	}

	[Flags]
	public enum MyFlags : ushort
	{
		Uno = 1,
		Due = 2,
		Quattro = 4,
		Cinque = 5,
		Otto = 8
	}


	[TestFixture]
	public class UserDataEnumsTests
	{
		public class EnumOverloadsTestClass
		{
			public string MyMethod(MyEnum enm)
			{
				return "[" + enm.ToString() + "]";
			}

			public string MyMethod(MyFlags enm)
			{
				return ((long)enm).ToString();
			}

			public string MyMethod2(MyEnum enm)
			{
				return "(" + enm.ToString() + ")";
			}

			public string MyMethodB(bool b)
			{
				return b ? "T" : "F";
			}

			public MyEnum Get()
			{
				return MyEnum.Quattro;
			}

			public MyFlags GetF()
			{
				return MyFlags.Quattro;
			}
		}


		private void RunTestOverload(string code, string expected)
		{
			Script S = new Script();

			EnumOverloadsTestClass obj = new EnumOverloadsTestClass();

			UserData.RegisterType<EnumOverloadsTestClass>(InteropAccessMode.Reflection);

			UserData.RegisterType<MyEnum>();
			UserData.RegisterType<MyFlags>();

			S.Globals.Set("MyEnum", UserData.CreateStatic<MyEnum>());
//			S.Globals.Set("MyFlags", UserData.CreateStatic<MyFlags>());
			S.Globals["MyFlags"] = typeof(MyFlags);

			S.Globals.Set("o", UserData.Create(obj));

			DynValue v = S.DoString("return " + code);

			Assert.AreEqual(DataType.String, v.Type);
			Assert.AreEqual(expected, v.String);
		}


		[Test]
		public void Interop_Enum_Simple()
		{
			RunTestOverload("o:MyMethod2(MyEnum.Cinque)", "(Cinque)");
		}

		[Test]
		public void Interop_Enum_Simple2()
		{
			RunTestOverload("o:MyMethod2(MyEnum.cinque)", "(Cinque)");
		}

		[Test]
		public void Interop_Enum_Overload1()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsOr(MyFlags.Uno, MyFlags.Due))", "3");
			RunTestOverload("o:MyMethod(MyEnum.Cinque)", "[Cinque]");
		}

		[Test]
		public void Interop_Enum_NumberConversion()
		{
			RunTestOverload("o:MyMethod2(5)", "(Cinque)");
		}


		[Test]
		public void Interop_Enum_Flags_Or()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsOr(MyFlags.Uno, MyFlags.Due))", "3");
		}

		[Test]
		public void Interop_Enum_Flags_And()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsAnd(MyFlags.Uno, MyFlags.Cinque))", "1");
		}

		[Test]
		public void Interop_Enum_Flags_Xor()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsXor(MyFlags.Uno, MyFlags.Cinque))", "4");
		}

		[Test]
		public void Interop_Enum_Flags_Not()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsAnd(MyFlags.Cinque, MyFlags.flagsNot(MyFlags.Uno)))", "4");
		}

		[Test]
		public void Interop_Enum_Flags_Or2()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsOr(MyFlags.Uno, 2))", "3");
		}

		[Test]
		public void Interop_Enum_Flags_Or3()
		{
			RunTestOverload("o:MyMethod(MyFlags.flagsOr(1, MyFlags.Due))", "3");
		}

		[Test]
		public void Interop_Enum_Flags_Or_Meta()
		{
			RunTestOverload("o:MyMethod(MyFlags.Uno .. MyFlags.Due)", "3");
		}


		[Test]
		public void Interop_Enum_Flags_HasAll()
		{
			RunTestOverload("o:MyMethodB(MyFlags.hasAll(MyFlags.Uno, MyFlags.Cinque))", "F");
			RunTestOverload("o:MyMethodB(MyFlags.hasAll(MyFlags.Cinque, MyFlags.Uno))", "T");
		}

		[Test]
		public void Interop_Enum_Flags_HasAny()
		{
			RunTestOverload("o:MyMethodB(MyFlags.hasAny(MyFlags.Uno, MyFlags.Cinque))", "T");
			RunTestOverload("o:MyMethodB(MyFlags.hasAny(MyFlags.Cinque, MyFlags.Uno))", "T");
			RunTestOverload("o:MyMethodB(MyFlags.hasAny(MyFlags.Quattro, MyFlags.Uno))", "F");
		}

		[Test]
		public void Interop_Enum_Read()
		{
			RunTestOverload("o:MyMethod(o:get())", "[Quattro]");
		}

		[Test]
		public void Interop_Enum_Flags_Or_Meta_Read()
		{
			RunTestOverload("o:MyMethod(o:getF() .. MyFlags.Due)", "6");
		}


	}
}
