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
			public string ConcatNums(int p1, int p2)
			{
				return string.Format("{0}%{1}", p1, p2);
			}

			public int SomeMethodWithLongName(int i)
			{
				return i * 2;
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

		public interface Interface1
		{
			string Test1();
		}

		public interface Interface2
		{
			string Test2();
		}


		public class SomeOtherClass
		{
			public string Test1()
			{
				return "Test1";
			}

			public string Test2()
			{
				return "Test2";
			}
		}


		public class SomeOtherClassCustomDescriptor
		{
		}

		public class CustomDescriptor : IUserDataDescriptor
		{
			public string Name
			{
				get { return "ciao"; }
			}

			public Type Type
			{
				get { return typeof(SomeOtherClassCustomDescriptor); }
			}

			public DynValue Index(Script script, object obj, DynValue index)
			{
				return DynValue.NewNumber(index.Number * 4);
			}

			public bool SetIndex(Script script, object obj, DynValue index, DynValue value)
			{
				throw new NotImplementedException();
			}

			public string AsString(object obj)
			{
				return null;
			}

			public DynValue MetaIndex(Script script, object obj, string metaname)
			{
				throw new NotImplementedException();
			}
		}




		public class SelfDescribingClass : IUserDataType
		{
			public DynValue Index(Script script, DynValue index)
			{
				return DynValue.NewNumber(index.Number * 3);
			}

			public bool SetIndex(Script script, DynValue index, DynValue value)
			{
				throw new NotImplementedException();
			}

			public DynValue MetaIndex(Script script, string metaname)
			{
				throw new NotImplementedException();
			}
		}

		public class SomeOtherClassWithDualInterfaces : Interface1, Interface2
		{
			public string Test1()
			{
				return "Test1";
			}

			public string Test2()
			{
				return "Test2";
			}
		}



		public void Test_ConcatMethodStatic(InteropAccessMode opt)
		{
			UserData.UnregisterType<SomeClass>();

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
			UserData.UnregisterType<SomeClass>();

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
			UserData.UnregisterType<SomeClass>();

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
			UserData.UnregisterType<SomeClass>();

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
			UserData.UnregisterType<SomeClass>();

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
			UserData.UnregisterType<SomeClass>();

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

		[Test]
		public void Interop_TestAutoregisterPolicy()
		{
			try
			{
				string script = @"return myobj:Test1()";

				UserData.RegistrationPolicy = InteropRegistrationPolicy.Automatic;

				Script S = new Script();

				SomeOtherClass obj = new SomeOtherClass();

				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);

				Assert.AreEqual(DataType.String, res.Type);
				Assert.AreEqual("Test1", res.String);
			}
			finally
			{
				UserData.RegistrationPolicy = InteropRegistrationPolicy.Explicit;
			}
		}

		[Test]
		public void Interop_TestAutoregisterPolicyWithDualInterfaces()
		{
			string script = @"return myobj:Test1() .. myobj:Test2()";

			Script S = new Script();

			UserData.RegisterType<Interface1>();
			UserData.RegisterType<Interface2>();

			SomeOtherClassWithDualInterfaces obj = new SomeOtherClassWithDualInterfaces();

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Test1Test2", res.String);
		}

		[Test]
		public void Interop_TestNamesCamelized()
		{
			UserData.UnregisterType<SomeClass>();

			string script = @"    
				a = myobj:SomeMethodWithLongName(1);
				b = myobj:someMethodWithLongName(2);
				c = myobj:some_method_with_long_name(3);
				d = myobj:Some_method_withLong_name(4);
				
				return a + b + c + d;
			";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.RegisterType<SomeClass>();

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(20, res.Number);

		}

		[Test]
		public void Interop_TestSelfDescribingType()
		{
			UserData.UnregisterType<SelfDescribingClass>();

			string script = @"    
				a = myobj[1];
				b = myobj[2];
				c = myobj[3];
				
				return a + b + c;
			";

			Script S = new Script();

			SelfDescribingClass obj = new SelfDescribingClass();

			UserData.RegisterType<SelfDescribingClass>();

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(18, res.Number);
		}

		[Test]
		public void Interop_TestCustomDescribedType()
		{
			UserData.UnregisterType<SomeOtherClassCustomDescriptor>();

			string script = @"    
				a = myobj[1];
				b = myobj[2];
				c = myobj[3];
				
				return a + b + c;
			";

			Script S = new Script();

			SomeOtherClassCustomDescriptor obj = new SomeOtherClassCustomDescriptor();

			UserData.RegisterType<SomeOtherClassCustomDescriptor>(new CustomDescriptor());

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(24, res.Number);
		}

	}
}
