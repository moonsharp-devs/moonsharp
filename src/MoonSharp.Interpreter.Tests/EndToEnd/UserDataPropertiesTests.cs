using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class UserDataPropertiesTests
	{
		public class SomeClass
		{
			public int IntProp { get; set; }
			public int? NIntProp { get; set; }
			public object ObjProp { get; set; }
			public static string StaticProp { get; set; }

			public static IEnumerable<int> Numbers
			{
				get
				{
					for (int i = 1; i <= 4; i++)
						yield return i;
				}
			}
		}

		public void Test_IntPropertyGetter(InteropAccessMode opt)
		{
			string script = @"    
				x = myobj.IntProp;
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass() {  IntProp = 321 };
			
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(321, res.Number);
		}

		public void Test_NIntPropertyGetter(InteropAccessMode opt)
		{
			string script = @"    
				x = myobj1.NIntProp;
				y = myobj2.NIntProp;
				return x,y;";

			Script S = new Script();

			SomeClass obj1 = new SomeClass() { NIntProp = 321 };
			SomeClass obj2 = new SomeClass() { NIntProp = null };

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj1", UserData.Create(obj1));
			S.Globals.Set("myobj2", UserData.Create(obj2));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(321.0, res.Tuple[0].Number);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Nil, res.Tuple[1].Type);
		}

		public void Test_ObjPropertyGetter(InteropAccessMode opt)
		{
			string script = @"    
				x = myobj1.ObjProp;
				y = myobj2.ObjProp;
				z = myobj2.ObjProp.ObjProp;
				return x,y,z;";

			Script S = new Script();

			SomeClass obj1 = new SomeClass() { ObjProp="ciao" };
			SomeClass obj2 = new SomeClass() { ObjProp = obj1 };

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj1", UserData.Create(obj1));
			S.Globals.Set("myobj2", UserData.Create(obj2));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(DataType.String, res.Tuple[0].Type);
			Assert.AreEqual("ciao", res.Tuple[0].String);
			Assert.AreEqual(DataType.String, res.Tuple[2].Type);
			Assert.AreEqual("ciao", res.Tuple[2].String);
			Assert.AreEqual(DataType.UserData, res.Tuple[1].Type);
			Assert.AreEqual(obj1, res.Tuple[1].UserData.Object);
		}

		public void Test_IntPropertySetter(InteropAccessMode opt)
		{
			string script = @"    
				myobj.IntProp = 19;";

			Script S = new Script();

			SomeClass obj = new SomeClass() { IntProp = 321 };

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			Assert.AreEqual(321, obj.IntProp);

			DynValue res = S.DoString(script);

			Assert.AreEqual(19, obj.IntProp);
		}

		public void Test_NIntPropertySetter(InteropAccessMode opt)
		{
			string script = @"    
				myobj1.NIntProp = nil;
				myobj2.NIntProp = 19;";

			Script S = new Script();

			SomeClass obj1 = new SomeClass() { NIntProp = 321 };
			SomeClass obj2 = new SomeClass() { NIntProp = null };

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj1", UserData.Create(obj1));
			S.Globals.Set("myobj2", UserData.Create(obj2));

			Assert.AreEqual(321, obj1.NIntProp);
			Assert.AreEqual(null, obj2.NIntProp);

			DynValue res = S.DoString(script);
	
			Assert.AreEqual(null, obj1.NIntProp);
			Assert.AreEqual(19, obj2.NIntProp);
		}

		public void Test_ObjPropertySetter(InteropAccessMode opt)
		{
			string script = @"    
				myobj1.ObjProp = myobj2;
				myobj2.ObjProp = 'hello';";

			Script S = new Script();

			SomeClass obj1 = new SomeClass() { ObjProp = "ciao" };
			SomeClass obj2 = new SomeClass() { ObjProp = obj1 };

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj1", UserData.Create(obj1));
			S.Globals.Set("myobj2", UserData.Create(obj2));

			Assert.AreEqual("ciao", obj1.ObjProp);
			Assert.AreEqual(obj1, obj2.ObjProp);

			DynValue res = S.DoString(script);

			Assert.AreEqual(obj2, obj1.ObjProp);
			Assert.AreEqual("hello", obj2.ObjProp);
		}

		public void Test_InvalidPropertySetter(InteropAccessMode opt)
		{
			string script = @"    
				myobj.IntProp = '19';";

			Script S = new Script();

			SomeClass obj = new SomeClass() { IntProp = 321 };

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			Assert.AreEqual(321, obj.IntProp);

			DynValue res = S.DoString(script);

			Assert.AreEqual(19, obj.IntProp);
		}

		public void Test_StaticPropertyAccess(InteropAccessMode opt)
		{
			string script = @"    
				static.StaticProp = 'asdasd' .. static.StaticProp;";

			Script S = new Script();

			SomeClass.StaticProp = "qweqwe";

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("static", UserData.CreateStatic<SomeClass>());

			Assert.AreEqual("qweqwe", SomeClass.StaticProp);

			DynValue res = S.DoString(script);

			Assert.AreEqual("asdasdqweqwe", SomeClass.StaticProp);
		}

		public void Test_IteratorPropertyGetter(InteropAccessMode opt)
		{
			string script = @"    
				x = 0;
				for i in myobj.Numbers do
					x = x + i;
				end

				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}

		[Test]
		public void Interop_IntPropertyGetter_None()
		{
			Test_IntPropertyGetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_IntPropertyGetter_Lazy()
		{
			Test_IntPropertyGetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_IntPropertyGetter_Precomputed()
		{
			Test_IntPropertyGetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_NIntPropertyGetter_None()
		{
			Test_NIntPropertyGetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_NIntPropertyGetter_Lazy()
		{
			Test_NIntPropertyGetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_NIntPropertyGetter_Precomputed()
		{
			Test_NIntPropertyGetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_ObjPropertyGetter_None()
		{
			Test_ObjPropertyGetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ObjPropertyGetter_Lazy()
		{
			Test_ObjPropertyGetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ObjPropertyGetter_Precomputed()
		{
			Test_ObjPropertyGetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_IntPropertySetter_None()
		{
			Test_IntPropertySetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_IntPropertySetter_Lazy()
		{
			Test_IntPropertySetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_IntPropertySetter_Precomputed()
		{
			Test_IntPropertySetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_NIntPropertySetter_None()
		{
			Test_NIntPropertySetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_NIntPropertySetter_Lazy()
		{
			Test_NIntPropertySetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_NIntPropertySetter_Precomputed()
		{
			Test_NIntPropertySetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_ObjPropertySetter_None()
		{
			Test_ObjPropertySetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_ObjPropertySetter_Lazy()
		{
			Test_ObjPropertySetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ObjPropertySetter_Precomputed()
		{
			Test_ObjPropertySetter(InteropAccessMode.Preoptimized);
		}


		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_InvalidPropertySetter_None()
		{
			Test_InvalidPropertySetter(InteropAccessMode.Reflection);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_InvalidPropertySetter_Lazy()
		{
			Test_InvalidPropertySetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_InvalidPropertySetter_Precomputed()
		{
			Test_InvalidPropertySetter(InteropAccessMode.Preoptimized);
		}


		[Test]
		public void Interop_StaticPropertyAccess_None()
		{
			Test_StaticPropertyAccess(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_StaticPropertyAccess_Lazy()
		{
			Test_StaticPropertyAccess(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_StaticPropertyAccess_Precomputed()
		{
			Test_StaticPropertyAccess(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_IteratorPropertyGetter_None()
		{
			Test_IteratorPropertyGetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_IteratorPropertyGetter_Lazy()
		{
			Test_IteratorPropertyGetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_IteratorPropertyGetter_Precomputed()
		{
			Test_IteratorPropertyGetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_IntPropertySetterWithSimplifiedSyntax()
		{
			string script = @"    
				myobj.IntProp = 19;";

			Script S = new Script();

			SomeClass obj = new SomeClass() { IntProp = 321 };

			UserData.RegisterType<SomeClass>();

			S.Globals["myobj"] = obj;

			Assert.AreEqual(321, obj.IntProp);

			DynValue res = S.DoString(script);

			Assert.AreEqual(obj, S.Globals["myobj"]);
			Assert.AreEqual(19, obj.IntProp);
		}

		[Test]
		public void Interop_Boh()
		{
			Script s = new Script();
			long big = long.MaxValue;
			var v = DynValue.FromObject(s, big);
			Assert.IsNotNull(v);
		}

	}
}
