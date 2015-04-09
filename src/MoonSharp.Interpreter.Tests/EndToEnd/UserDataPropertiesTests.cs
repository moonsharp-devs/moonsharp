using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;
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

			public int RoIntProp { get { return 5; } }
			public int RoIntProp2 { get; private set; }

			public int WoIntProp { set { IntProp = value; } }
			public int WoIntProp2 { internal get; set; }

			[MoonSharpVisible(false)]
			internal int AccessOverrProp
			{
				get;
				[MoonSharpVisible(true)]
				set;
			}


			public SomeClass()
			{
				RoIntProp2 = 1234;
				WoIntProp2 = 1235;
			}

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

			SomeClass obj = new SomeClass() { IntProp = 321 };

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			SomeClass obj1 = new SomeClass() { ObjProp = "ciao" };
			SomeClass obj2 = new SomeClass() { ObjProp = obj1 };

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
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

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}

		public void Test_RoIntPropertyGetter(InteropAccessMode opt)
		{
			string script = @"    
				x = myobj.RoIntProp;
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(5, res.Number);
		}

		public void Test_RoIntProperty2Getter(InteropAccessMode opt)
		{
			string script = @"    
				x = myobj.RoIntProp2;
				return x;";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1234, res.Number);
		}

		public void Test_RoIntPropertySetter(InteropAccessMode opt)
		{
			try
			{
				string script = @"    
				myobj.RoIntProp = 19;
				return myobj.RoIntProp;
			";

				Script S = new Script();

				SomeClass obj = new SomeClass();

				UserData.UnregisterType<SomeClass>();
				UserData.RegisterType<SomeClass>(opt);

				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);
			}
			catch (ScriptRuntimeException)
			{
				return;
			}

			Assert.Fail();
		}

		public void Test_RoIntProperty2Setter(InteropAccessMode opt)
		{
			try
			{
				string script = @"    
				myobj.RoIntProp2 = 19;
				return myobj.RoIntProp2;
			";

				Script S = new Script();

				SomeClass obj = new SomeClass();

				UserData.UnregisterType<SomeClass>();
				UserData.RegisterType<SomeClass>(opt);

				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);
			}
			catch (ScriptRuntimeException)
			{
				return;
			}

			Assert.Fail();
		}


		public void Test_WoIntPropertySetter(InteropAccessMode opt)
		{
			string script = @"    
				myobj.WoIntProp = 19;
			";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(19, obj.IntProp);
		}

		public void Test_WoIntProperty2Setter(InteropAccessMode opt)
		{
			string script = @"    
				myobj.WoIntProp2 = 19;
			";

			Script S = new Script();

			SomeClass obj = new SomeClass();

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>(opt);

			S.Globals.Set("myobj", UserData.Create(obj));

			DynValue res = S.DoString(script);

			Assert.AreEqual(19, obj.WoIntProp2);
		}


		public void Test_WoIntPropertyGetter(InteropAccessMode opt)
		{
			try
			{
				string script = @"    
				x = myobj.WoIntProp;
				return x;";

				Script S = new Script();

				SomeClass obj = new SomeClass();

				UserData.UnregisterType<SomeClass>();
				UserData.RegisterType<SomeClass>(opt);

				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);

				Assert.AreEqual(DataType.Number, res.Type);
				Assert.AreEqual(5, res.Number);
			}
			catch (ScriptRuntimeException)
			{
				return;
			}

			Assert.Fail();
		}

		public void Test_WoIntProperty2Getter(InteropAccessMode opt)
		{
			try
			{
				string script = @"    
				x = myobj.WoIntProp2;
				return x;";

				Script S = new Script();

				SomeClass obj = new SomeClass();

				UserData.UnregisterType<SomeClass>();
				UserData.RegisterType<SomeClass>(opt);

				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);

				Assert.AreEqual(DataType.Number, res.Type);
				Assert.AreEqual(1234, res.Number);
			}
			catch (ScriptRuntimeException)
			{
				return;
			}

			Assert.Fail();
		}


		public void Test_PropertyAccessOverrides(InteropAccessMode opt)
		{
			SomeClass obj = new SomeClass();

			try
			{
				string script = @"    
				myobj.AccessOverrProp = 19;
				return myobj.AccessOverrProp;
			";

				Script S = new Script();

				obj.AccessOverrProp = 13;

				UserData.UnregisterType<SomeClass>();
				UserData.RegisterType<SomeClass>(opt);

				S.Globals.Set("myobj", UserData.Create(obj));

				DynValue res = S.DoString(script);
			}
			catch (ScriptRuntimeException)
			{
				Assert.AreEqual(19, obj.AccessOverrProp);
				return;
			}

			Assert.Fail();
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
		public void Interop_RoIntPropertyGetter_None()
		{
			Test_RoIntPropertyGetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_RoIntPropertyGetter_Lazy()
		{
			Test_RoIntPropertyGetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_RoIntPropertyGetter_Precomputed()
		{
			Test_RoIntPropertyGetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_RoIntProperty2Getter_None()
		{
			Test_RoIntProperty2Getter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_RoIntProperty2Getter_Lazy()
		{
			Test_RoIntProperty2Getter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_RoIntProperty2Getter_Precomputed()
		{
			Test_RoIntProperty2Getter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_RoIntPropertySetter_None()
		{
			Test_RoIntPropertySetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_RoIntPropertySetter_Lazy()
		{
			Test_RoIntPropertySetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_RoIntPropertySetter_Precomputed()
		{
			Test_RoIntPropertySetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_RoIntProperty2Setter_None()
		{
			Test_RoIntProperty2Setter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_RoIntProperty2Setter_Lazy()
		{
			Test_RoIntProperty2Setter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_RoIntProperty2Setter_Precomputed()
		{
			Test_RoIntProperty2Setter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_WoIntPropertyGetter_None()
		{
			Test_WoIntPropertyGetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_WoIntPropertyGetter_Lazy()
		{
			Test_WoIntPropertyGetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_WoIntPropertyGetter_Precomputed()
		{
			Test_WoIntPropertyGetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_WoIntProperty2Getter_None()
		{
			Test_WoIntProperty2Getter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_WoIntProperty2Getter_Lazy()
		{
			Test_WoIntProperty2Getter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_WoIntProperty2Getter_Precomputed()
		{
			Test_WoIntProperty2Getter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_WoIntPropertySetter_None()
		{
			Test_WoIntPropertySetter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_WoIntPropertySetter_Lazy()
		{
			Test_WoIntPropertySetter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_WoIntPropertySetter_Precomputed()
		{
			Test_WoIntPropertySetter(InteropAccessMode.Preoptimized);
		}

		[Test]
		public void Interop_WoIntProperty2Setter_None()
		{
			Test_WoIntProperty2Setter(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_WoIntProperty2Setter_Lazy()
		{
			Test_WoIntProperty2Setter(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_WoIntProperty2Setter_Precomputed()
		{
			Test_WoIntProperty2Setter(InteropAccessMode.Preoptimized);
		}
		
		[Test]
		public void Interop_PropertyAccessOverrides_None()
		{
			Test_PropertyAccessOverrides(InteropAccessMode.Reflection);
		}

		[Test]
		public void Interop_PropertyAccessOverrides_Lazy()
		{
			Test_PropertyAccessOverrides(InteropAccessMode.LazyOptimized);
		}

		[Test]
		public void Interop_PropertyAccessOverrides_Precomputed()
		{
			Test_PropertyAccessOverrides(InteropAccessMode.Preoptimized);
		}		
		
		
		
		[Test]
		public void Interop_IntPropertySetterWithSimplifiedSyntax()
		{
			string script = @"    
				myobj.IntProp = 19;";

			Script S = new Script();

			SomeClass obj = new SomeClass() { IntProp = 321 };

			UserData.UnregisterType<SomeClass>();
			UserData.RegisterType<SomeClass>();

			S.Globals["myobj"] = obj;

			Assert.AreEqual(321, obj.IntProp);

			DynValue res = S.DoString(script);

			Assert.AreEqual(obj, S.Globals["myobj"]);
			Assert.AreEqual(19, obj.IntProp);
		}

		[Test]
		public void Interop_OutOfRangeNumber()
		{
			Script s = new Script();
			long big = long.MaxValue;
			var v = DynValue.FromObject(s, big);
			Assert.IsNotNull(v);
		}

	}
}
