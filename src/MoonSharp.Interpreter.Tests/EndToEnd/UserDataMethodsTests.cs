using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class UserDataMethodsTests
	{
		public class SomeClass : IComparable
		{
			public static StringBuilder ConcatS(int p1, string p2, IComparable p3, bool p4, List<object> p5, IEnumerable<object> p6, 
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

			public StringBuilder ConcatI(int p1, string p2, IComparable p3, bool p4, List<object> p5, IEnumerable<object> p6,
				StringBuilder p7, Dictionary<object, object> p8, SomeClass p9, int p10 = 1912)
			{
				return ConcatS(p1, p2, p3, p4, p5, p6, p7, p8, this, p10);
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


		public void Test_ConcatMethodStatic(UserDataAccessMode opt)
		{
			string script = @"    
				t = { 'asd', 'qwe', 'zxc', ['x'] = 'X', ['y'] = 'Y' };
				x = static.ConcatS(1, 'ciao', myobj, true, t, t, 'eheh', t, myobj);
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("static", UserData.CreateStatic<SomeClass>());
			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|asdqweXYzxc|!SOMECLASS!1994", res.String);
		}
		public void Test_ConcatMethod(UserDataAccessMode opt)
		{
			string script = @"    
				t = { 'asd', 'qwe', 'zxc', ['x'] = 'X', ['y'] = 'Y' };
				x = myobj.ConcatI(1, 'ciao', myobj, true, t, t, 'eheh', t, myobj);
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|asdqweXYzxc|!SOMECLASS!1912", res.String);
		}

		public void Test_ConcatMethodStaticSimplifiedSyntax(UserDataAccessMode opt)
		{
			string script = @"    
				t = { 'asd', 'qwe', 'zxc', ['x'] = 'X', ['y'] = 'Y' };
				x = static.ConcatS(1, 'ciao', myobj, true, t, t, 'eheh', t, myobj);
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.RegisterType<SomeClass>(opt);

			S.Globals["static"] = typeof(SomeClass);
			S.Globals["myobj"] = obj;

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|asdqweXYzxc|!SOMECLASS!1994", res.String);
		}


		[Test]
		public void Interpo_ConcatMethod_None()
		{
			Test_ConcatMethod(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interpo_ConcatMethod_Lazy()
		{
			Test_ConcatMethod(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interpo_ConcatMethod_Precomputed()
		{
			Test_ConcatMethod(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interpo_ConcatMethodStatic_None()
		{
			Test_ConcatMethodStatic(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interpo_ConcatMethodStatic_Lazy()
		{
			Test_ConcatMethodStatic(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interpo_ConcatMethodStatic_Precomputed()
		{
			Test_ConcatMethodStatic(UserDataAccessMode.Preoptimized);
		}


		[Test]
		public void Interpo_ConcatMethodStaticSimplifiedSyntax_None()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interpo_ConcatMethodStaticSimplifiedSyntax_Lazy()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interpo_ConcatMethodStaticSimplifiedSyntax_Precomputed()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(UserDataAccessMode.Preoptimized);
		}
	}
}
