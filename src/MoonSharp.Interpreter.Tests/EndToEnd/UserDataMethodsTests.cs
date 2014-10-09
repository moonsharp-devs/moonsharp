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
			public string ConcatNums(int p1, int p2)
			{
				return string.Format("{0}%{1}", p1, p2);
			}


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

			public StringBuilder ConcatI(Script s, int p1, string p2, IComparable p3, bool p4, List<object> p5, IEnumerable<object> p6,
				StringBuilder p7, Dictionary<object, object> p8, SomeClass p9, int p10 = 1912)
			{
				Assert.IsNotNull(s);
				return ConcatS(p1, p2, p3, p4, p5, p6, p7, p8, this, p10);
			}

			public override string ToString()
			{
				return "!SOMECLASS!";
			}

			public List<int> MkList(int from, int to)
			{
				List<int> l = new List<int>();
				for (int i = from; i <= to; i++)
					l.Add(i);
				return l;
			}


			public int CompareTo(object obj)
			{
				throw new NotImplementedException();
			}
		}


		public void Test_ConcatMethodStatic(InteropAccessMode opt)
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
		public void Test_ConcatMethod(InteropAccessMode opt)
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
		public void Test_ConcatMethodSemicolon(InteropAccessMode opt)
		{
			string script = @"    
				t = { 'asd', 'qwe', 'zxc', ['x'] = 'X', ['y'] = 'Y' };
				x = myobj:ConcatI(1, 'ciao', myobj, true, t, t, 'eheh', t, myobj);
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|asdqweXYzxc|!SOMECLASS!1912", res.String);
		}

		public void Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode opt)
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

		public void Test_DelegateMethod(InteropAccessMode opt)
		{
			string script = @"    
				x = concat(1, 2);
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			S.Globals["concat"] = CallbackFunction.FromDelegate(S, (Func<int, int, string>)obj.ConcatNums, opt);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1%2", res.String);
		}

		public void Test_ListMethod(InteropAccessMode opt)
		{
			string script = @"    
				x = mklist(1, 4);
				sum = 0;				

				for _, v in ipairs(x) do
					sum = sum + v;
				end

				return sum;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			S.Globals["mklist"] = CallbackFunction.FromDelegate(S, (Func<int, int, List<int>>)obj.MkList, opt);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}


		[Test]
		public void Interop_ConcatMethod_None()
		{
			Test_ConcatMethod(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ConcatMethod_Lazy()
		{
			Test_ConcatMethod(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ConcatMethod_Precomputed()
		{
			Test_ConcatMethod(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_ConcatMethodSemicolon_None()
		{
			Test_ConcatMethodSemicolon(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ConcatMethodSemicolon_Lazy()
		{
			Test_ConcatMethodSemicolon(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ConcatMethodSemicolon_Precomputed()
		{
			Test_ConcatMethodSemicolon(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_ConcatMethodStatic_None()
		{
			Test_ConcatMethodStatic(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ConcatMethodStatic_Lazy()
		{
			Test_ConcatMethodStatic(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ConcatMethodStatic_Precomputed()
		{
			Test_ConcatMethodStatic(InteropAccessMode.Preoptimized);
		}


		[Test]
		public void Interop_ConcatMethodStaticSimplifiedSyntax_None()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ConcatMethodStaticSimplifiedSyntax_Lazy()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ConcatMethodStaticSimplifiedSyntax_Precomputed()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_DelegateMethod_None()
		{
			Test_DelegateMethod(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_DelegateMethod_Lazy()
		{
			Test_DelegateMethod(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_DelegateMethod_Precomputed()
		{
			Test_DelegateMethod(InteropAccessMode.Preoptimized);
		}


		[Test]
		public void Interop_ListMethod_None()
		{
			Test_ListMethod(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ListMethod_Lazy()
		{
			Test_ListMethod(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ListMethod_Precomputed()
		{
			Test_ListMethod(InteropAccessMode.Preoptimized);
		}
	}
}
