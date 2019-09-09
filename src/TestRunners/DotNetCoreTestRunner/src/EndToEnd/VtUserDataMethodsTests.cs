using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class VtUserDataMethodsTests
	{
		public struct SomeClass_NoRegister : IComparable
		{
			public string ManipulateString(string input, ref string tobeconcat, out string lowercase)
			{
				tobeconcat = input + tobeconcat;
				lowercase = input.ToLower();
				return input.ToUpper();
			}

			public string ConcatNums(int p1, int p2)
			{
				return string.Format("{0}%{1}", p1, p2);
			}

			public int SomeMethodWithLongName(int i)
			{
				return i * 2;
			}

			public static StringBuilder SetComplexRecursive(List<int[]> intList)
			{
				StringBuilder sb = new StringBuilder();

				foreach (int[] arr in intList)
				{
					sb.Append(string.Join(",", arr.Select(s => s.ToString()).ToArray()));
					sb.Append("|");
				}

				return sb;
			}

			public static StringBuilder SetComplexTypes(List<string> strlist, IList<int> intlist, Dictionary<string, int> map,
				string[] strarray, int[] intarray)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(string.Join(",", strlist.ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", intlist.Select(i => i.ToString()).ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", map.Keys.OrderBy(x => x.ToUpperInvariant()).ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", map.Values.OrderBy(x => x).Select(i => i.ToString()).ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", strarray));

				sb.Append("|");

				sb.Append(string.Join(",", intarray.Select(i => i.ToString()).ToArray()));

				return sb;
			}


			public static StringBuilder ConcatS(int p1, string p2, IComparable p3, bool p4, List<object> p5, IEnumerable<object> p6,
				StringBuilder p7, Dictionary<object, object> p8, SomeClass_NoRegister p9, int p10 = 1994)
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
				foreach (var o in p8.Keys.OrderBy(x => x.ToString().ToUpperInvariant())) p7.Append(o);
				p7.Append("|");
				foreach (var o in p8.Values.OrderBy(x => x.ToString().ToUpperInvariant())) p7.Append(o);
				p7.Append("|");

				p7.Append(p9);
				p7.Append(p10);

				return p7;
			}

			public string Format(string s, params object[] args)
			{
				return string.Format(s, args);
			}

			public StringBuilder ConcatI(Script s, int p1, string p2, IComparable p3, bool p4, List<object> p5, IEnumerable<object> p6,
				StringBuilder p7, Dictionary<object, object> p8, SomeClass_NoRegister p9, int p10 = 1912)
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

		public struct SomeClass : IComparable
		{
			public string ManipulateString(string input, ref string tobeconcat, out string lowercase)
			{
				tobeconcat = input + tobeconcat;
				lowercase = input.ToLower();
				return input.ToUpper();
			}

			public string ConcatNums(int p1, int p2)
			{
				return string.Format("{0}%{1}", p1, p2);
			}

			public int SomeMethodWithLongName(int i)
			{
				return i * 2;
			}

			public static StringBuilder SetComplexRecursive(List<int[]> intList)
			{
				StringBuilder sb = new StringBuilder();

				foreach (int[] arr in intList)
				{
					sb.Append(string.Join(",", arr.Select(s => s.ToString()).ToArray()));
					sb.Append("|");
				}

				return sb;
			}

			public static StringBuilder SetComplexTypes(List<string> strlist, IList<int> intlist, Dictionary<string, int> map,
				string[] strarray, int[] intarray)
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(string.Join(",", strlist.ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", intlist.Select(i => i.ToString()).ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", map.Keys.OrderBy(x => x.ToUpperInvariant()).ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", map.Values.OrderBy(x => x).Select(i => i.ToString()).ToArray()));

				sb.Append("|");

				sb.Append(string.Join(",", strarray));

				sb.Append("|");

				sb.Append(string.Join(",", intarray.Select(i => i.ToString()).ToArray()));

				return sb;
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
				foreach (var o in p8.Keys.OrderBy(x => x.ToString().ToUpperInvariant())) p7.Append(o);
				p7.Append("|");
				foreach (var o in p8.Values.OrderBy(x => x.ToString().ToUpperInvariant())) p7.Append(o);
				p7.Append("|");

				p7.Append(p9);
				p7.Append(p10);

				return p7;
			}

			public string Format(string s, params object[] args)
			{
				return string.Format(s, args);
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


		public struct SomeOtherClassCustomDescriptor
		{
		}

		public struct CustomDescriptor : IUserDataDescriptor
		{
			public string Name
			{
				get { return "ciao"; }
			}

			public Type Type
			{
				get { return typeof(SomeOtherClassCustomDescriptor); }
			}

			public DynValue Index(Script script, object obj, DynValue index, bool dummy)
			{
				return DynValue.NewNumber(index.Number * 4);
			}

			public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool dummy)
			{
				throw new NotImplementedException();
			}

			public string AsString(object obj)
			{
				return null;
			}

			public DynValue MetaIndex(Script script, object obj, string metaname)
			{
				return null;
			}

			public bool IsTypeCompatible(Type type, object obj)
			{
				return Framework.Do.IsInstanceOfType(type, obj);
			}
		}




		public struct SelfDescribingClass : IUserDataType
		{
			public DynValue Index(Script script, DynValue index, bool isNameIndex)
			{
				return DynValue.NewNumber(index.Number * 3);
			}

			public bool SetIndex(Script script, DynValue index, DynValue value, bool isNameIndex)
			{
				throw new NotImplementedException();
			}

			public DynValue MetaIndex(Script script, string metaname)
			{
				throw new NotImplementedException();
			}
		}

		public struct SomeOtherClassWithDualInterfaces : Interface1, Interface2
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

		public void Test_VarArgs(InteropAccessMode opt)
		{
			UserData.UnregisterType<SomeClass>();

			string script = @"    
			return myobj.format('{0}.{1}@{2}:{3}', 1, 2, 'ciao', true);";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1.2@ciao:True", res.String);
		}


		public void Test_ConcatMethodStaticComplexCustomConv(InteropAccessMode opt)
		{
			try
			{
				UserData.UnregisterType<SomeClass>();

				string script = @"    
				strlist = { 'ciao', 'hello', 'aloha' };
				intlist = {  };
				dictry = { ciao = 39, hello = 78, aloha = 128 };
				
				x = static.SetComplexTypes(strlist, intlist, dictry, strlist, intlist);

				return x;";

				Script S = new Script();

				SomeClass obj = new SomeClass();

				UserData.UnregisterType<SomeClass>();
				UserData.RegisterType<SomeClass>(opt);

				Script.GlobalOptions.CustomConverters.Clear();

				Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(List<string>),
					v => null);

				Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(IList<int>),
					v => new List<int>() { 42, 77, 125, 13 });

				Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, typeof(int[]),
					v => new int[] { 43, 78, 126, 14 });

				Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<StringBuilder>(
					(_s, v) => DynValue.NewString(v.ToString().ToUpper()));


				S.Globals.Set("static", UserData.CreateStatic<SomeClass>());
				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);

				Assert.AreEqual(DataType.String, res.Type);
				Assert.AreEqual("CIAO,HELLO,ALOHA|42,77,125,13|ALOHA,CIAO,HELLO|39,78,128|CIAO,HELLO,ALOHA|43,78,126,14", res.String);
			}
			finally
			{
				Script.GlobalOptions.CustomConverters.Clear();
			}
		}


		public void Test_ConcatMethodStaticComplex(InteropAccessMode opt)
		{
			UserData.UnregisterType<SomeClass>();

			string script = @"    
				strlist = { 'ciao', 'hello', 'aloha' };
				intlist = { 42, 77, 125, 13 };
				dictry = { ciao = 39, hello = 78, aloha = 128 };
				
				x = static.SetComplexTypes(strlist, intlist, dictry, strlist, intlist);

				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("static", UserData.CreateStatic<SomeClass>());
			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("ciao,hello,aloha|42,77,125,13|aloha,ciao,hello|39,78,128|ciao,hello,aloha|42,77,125,13", res.String);
		}

		public void Test_ConcatMethodStaticComplexRec(InteropAccessMode opt)
		{
			UserData.UnregisterType<SomeClass>();

			string script = @"    
				array = { { 1, 2, 3 }, { 11, 35, 77 }, { 16, 42, 64 }, {99, 76, 17 } };				
			
				x = static.SetComplexRecursive(array);

				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("static", UserData.CreateStatic<SomeClass>());
			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1,2,3|11,35,77|16,42,64|99,76,17|", res.String);
		}


		public void Test_RefOutParams(InteropAccessMode opt)
		{
			UserData.UnregisterType<SomeClass>();

			string script = @"    
				x, y, z = myobj:manipulateString('CiAo', 'hello');
				return x, y, z;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("static", UserData.CreateStatic<SomeClass>());
			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(3, res.Tuple.Length);
			Assert.AreEqual(DataType.String, res.Tuple[0].Type);
			Assert.AreEqual(DataType.String, res.Tuple[1].Type);
			Assert.AreEqual(DataType.String, res.Tuple[2].Type);
			Assert.AreEqual("CIAO", res.Tuple[0].String);
			Assert.AreEqual("CiAohello", res.Tuple[1].String);
			Assert.AreEqual("ciao", res.Tuple[2].String);
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("eheh1ciao!SOMECLASS!True|asdqwezxc|asdqwezxc|123xy|asdqweXYzxc|!SOMECLASS!1912", res.String);
		}

		public void Test_ConstructorAndConcatMethodSemicolon(InteropAccessMode opt)
		{
			UserData.UnregisterType<SomeClass>();

			string script = @"    
				myobj = mytype.__new();
				t = { 'asd', 'qwe', 'zxc', ['x'] = 'X', ['y'] = 'Y' };
				x = myobj:ConcatI(1, 'ciao', myobj, true, t, t, 'eheh', t, myobj);
				return x;";

			Script S = new Script();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals["mytype"] = typeof(SomeClass);

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

			UserData.UnregisterType<SomeClass>();
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
			string script = @"    
				x = mklist(1, 4);
				sum = 0;				

				for _, v in ipairs(x) do
					sum = sum + v;
				end

				return sum;";

			Script S = new Script();

			SomeClass_NoRegister obj = new SomeClass_NoRegister();

			S.Globals["mklist"] = CallbackFunction.FromDelegate(S, (Func<int, int, List<int>>)obj.MkList, opt);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}


		[Test]
		public void VInterop_ConcatMethod_None()
		{
			Test_ConcatMethod(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethod_Lazy()
		{
			Test_ConcatMethod(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethod_Precomputed()
		{
			Test_ConcatMethod(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ConcatMethodSemicolon_None()
		{
			Test_ConcatMethodSemicolon(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethodSemicolon_Lazy()
		{
			Test_ConcatMethodSemicolon(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethodSemicolon_Precomputed()
		{
			Test_ConcatMethodSemicolon(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ConstructorAndConcatMethodSemicolon_None()
		{
			Test_ConstructorAndConcatMethodSemicolon(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConstructorAndConcatMethodSemicolon_Lazy()
		{
			Test_ConstructorAndConcatMethodSemicolon(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConstructorAndConcatMethodSemicolon_Precomputed()
		{
			Test_ConstructorAndConcatMethodSemicolon(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplxCustomConv_None()
		{
			Test_ConcatMethodStaticComplexCustomConv(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplxCustomConv_Lazy()
		{
			Test_ConcatMethodStaticComplexCustomConv(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplxCustomConv_Precomputed()
		{
			Test_ConcatMethodStaticComplexCustomConv(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplx_None()
		{
			Test_ConcatMethodStaticComplex(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplx_Lazy()
		{
			Test_ConcatMethodStaticComplex(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplx_Precomputed()
		{
			Test_ConcatMethodStaticComplex(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplxRec_None()
		{
			Test_ConcatMethodStaticComplexRec(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplxRec_Lazy()
		{
			Test_ConcatMethodStaticComplexRec(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticCplxRec_Precomputed()
		{
			Test_ConcatMethodStaticComplexRec(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStatic_None()
		{
			Test_ConcatMethodStatic(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethodStatic_Lazy()
		{
			Test_ConcatMethodStatic(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStatic_Precomputed()
		{
			Test_ConcatMethodStatic(InteropAccessMode.Preoptimized);
		}


		[Test]
		public void VInterop_ConcatMethodStaticSimplifiedSyntax_None()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ConcatMethodStaticSimplifiedSyntax_Lazy()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ConcatMethodStaticSimplifiedSyntax_Precomputed()
		{
			Test_ConcatMethodStaticSimplifiedSyntax(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_VarArgs_None()
		{
			Test_VarArgs(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_VarArgs_Lazy()
		{
			Test_VarArgs(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_VarArgs_Precomputed()
		{
			Test_VarArgs(InteropAccessMode.Preoptimized);
		}


		[Test]
		public void VInterop_DelegateMethod_None()
		{
			Test_DelegateMethod(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_DelegateMethod_Lazy()
		{
			Test_DelegateMethod(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_DelegateMethod_Precomputed()
		{
			Test_DelegateMethod(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_OutRefParams_None()
		{
			Test_RefOutParams(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_OutRefParams_Lazy()
		{
			Test_RefOutParams(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_OutRefParams_Precomputed()
		{
			Test_RefOutParams(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_ListMethod_None()
		{
			Test_ListMethod(InteropAccessMode.Reflection);
		}

		[Test]
		public void VInterop_ListMethod_Lazy()
		{
			Test_ListMethod(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void VInterop_ListMethod_Precomputed()
		{
			Test_ListMethod(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void VInterop_TestAutoregisterPolicy()
		{
			var oldPolicy = UserData.RegistrationPolicy;

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
				UserData.RegistrationPolicy = oldPolicy;
			}
		}

		[Test]
		public void VInterop_DualInterfaces()
		{
			string script = @"return myobj:Test1() .. myobj:Test2()";

			Script S = new Script();

			UserData.UnregisterType<Interface1>();
			UserData.UnregisterType<Interface2>();
			UserData.RegisterType<Interface1>();
			UserData.RegisterType<Interface2>();

			SomeOtherClassWithDualInterfaces obj = new SomeOtherClassWithDualInterfaces();

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Test1Test2", res.String);
		}

		[Test]
		public void VInterop_TestNamesCamelized()
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

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>();

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(20, res.Number);

		}

		[Test]
		public void VInterop_TestSelfDescribingType()
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

			UserData.UnregisterType<SelfDescribingClass>();
			UserData.RegisterType<SelfDescribingClass>();

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(18, res.Number);
		}

		[Test]
		public void VInterop_TestCustomDescribedType()
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

