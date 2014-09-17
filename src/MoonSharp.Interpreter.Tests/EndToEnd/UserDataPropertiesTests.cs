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

		public void Test_IntPropertyGetter(UserDataAccessMode opt)
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

		public void Test_NIntPropertyGetter(UserDataAccessMode opt)
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

		public void Test_ObjPropertyGetter(UserDataAccessMode opt)
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

		public void Test_IntPropertySetter(UserDataAccessMode opt)
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

		public void Test_NIntPropertySetter(UserDataAccessMode opt)
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

		public void Test_ObjPropertySetter(UserDataAccessMode opt)
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

		public void Test_InvalidPropertySetter(UserDataAccessMode opt)
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

		public void Test_StaticPropertyAccess(UserDataAccessMode opt)
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

		public void Test_IteratorPropertyGetter(UserDataAccessMode opt)
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
			Test_IntPropertyGetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_IntPropertyGetter_Lazy()
		{
			Test_IntPropertyGetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_IntPropertyGetter_Precomputed()
		{
			Test_IntPropertyGetter(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_NIntPropertyGetter_None()
		{
			Test_NIntPropertyGetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_NIntPropertyGetter_Lazy()
		{
			Test_NIntPropertyGetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_NIntPropertyGetter_Precomputed()
		{
			Test_NIntPropertyGetter(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_ObjPropertyGetter_None()
		{
			Test_ObjPropertyGetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_ObjPropertyGetter_Lazy()
		{
			Test_ObjPropertyGetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ObjPropertyGetter_Precomputed()
		{
			Test_ObjPropertyGetter(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_IntPropertySetter_None()
		{
			Test_IntPropertySetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_IntPropertySetter_Lazy()
		{
			Test_IntPropertySetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_IntPropertySetter_Precomputed()
		{
			Test_IntPropertySetter(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_NIntPropertySetter_None()
		{
			Test_NIntPropertySetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_NIntPropertySetter_Lazy()
		{
			Test_NIntPropertySetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_NIntPropertySetter_Precomputed()
		{
			Test_NIntPropertySetter(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_ObjPropertySetter_None()
		{
			Test_ObjPropertySetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_ObjPropertySetter_Lazy()
		{
			Test_ObjPropertySetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_ObjPropertySetter_Precomputed()
		{
			Test_ObjPropertySetter(UserDataAccessMode.Preoptimized);
		}


		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_InvalidPropertySetter_None()
		{
			Test_InvalidPropertySetter(UserDataAccessMode.Reflection);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_InvalidPropertySetter_Lazy()
		{
			Test_InvalidPropertySetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_InvalidPropertySetter_Precomputed()
		{
			Test_InvalidPropertySetter(UserDataAccessMode.Preoptimized);
		}


		[Test]
		public void Interop_StaticPropertyAccess_None()
		{
			Test_StaticPropertyAccess(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_StaticPropertyAccess_Lazy()
		{
			Test_StaticPropertyAccess(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_StaticPropertyAccess_Precomputed()
		{
			Test_StaticPropertyAccess(UserDataAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_IteratorPropertyGetter_None()
		{
			Test_IteratorPropertyGetter(UserDataAccessMode.Reflection);
		}

		[Test]
		public void Interop_IteratorPropertyGetter_Lazy()
		{
			Test_IteratorPropertyGetter(UserDataAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_IteratorPropertyGetter_Precomputed()
		{
			Test_IteratorPropertyGetter(UserDataAccessMode.Preoptimized);
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
	}
}
