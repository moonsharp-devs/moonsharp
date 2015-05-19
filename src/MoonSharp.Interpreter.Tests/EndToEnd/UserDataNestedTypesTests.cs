using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class UserDataNestedTypesTests
	{
		public class SomeType
		{
			public enum SomeNestedEnum
			{
				Asdasdasd,
			}

			public static SomeNestedEnum Get()
			{
				return SomeNestedEnum.Asdasdasd;
			}

			public class SomeNestedType
			{
				public static string Get()
				{
					return "Ciao from SomeNestedType";
				}
			}

			[MoonSharpUserData]
			private class SomeNestedTypePrivate
			{
				public static string Get()
				{
					return "Ciao from SomeNestedTypePrivate";
				}
			}

			private class SomeNestedTypePrivate2
			{
				public static string Get()
				{
					return "Ciao from SomeNestedTypePrivate2";
				}
			}

		}

		public struct VSomeType
		{
			public struct SomeNestedType
			{
				public static string Get()
				{
					return "Ciao from SomeNestedType";
				}
			}

			[MoonSharpUserData]
			private struct SomeNestedTypePrivate
			{
				public static string Get()
				{
					return "Ciao from SomeNestedTypePrivate";
				}
			}

			private struct SomeNestedTypePrivate2
			{
				public static string Get()
				{
					return "Ciao from SomeNestedTypePrivate2";
				}
			}

		}

		[Test]
		public void Interop_NestedTypes_Public_Enum()
		{
			Script S = new Script();

			UserData.RegisterType<SomeType>();

			S.Globals.Set("o", UserData.CreateStatic<SomeType>());

			DynValue res = S.DoString("return o:Get()");

			Assert.AreEqual(DataType.UserData, res.Type);
		}


		[Test]
		public void Interop_NestedTypes_Public_Ref()
		{
			Script S = new Script();

			UserData.RegisterType<SomeType>();

			S.Globals.Set("o", UserData.CreateStatic<SomeType>());

			DynValue res = S.DoString("return o.SomeNestedType:Get()");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Ciao from SomeNestedType", res.String);
		}


		[Test]
		public void Interop_NestedTypes_Private_Ref()
		{
			Script S = new Script();

			UserData.RegisterType<SomeType>();

			S.Globals.Set("o", UserData.CreateStatic<SomeType>());

			DynValue res = S.DoString("return o.SomeNestedTypePrivate:Get()");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Ciao from SomeNestedTypePrivate", res.String);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_NestedTypes_Private_Ref_2()
		{
			Script S = new Script();

			UserData.RegisterType<SomeType>();

			S.Globals.Set("o", UserData.CreateStatic<SomeType>());

			DynValue res = S.DoString("return o.SomeNestedTypePrivate2:Get()");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Ciao from SomeNestedTypePrivate2", res.String);
		}

		[Test]
		public void Interop_NestedTypes_Public_Val()
		{
			Script S = new Script();

			UserData.RegisterType<VSomeType>();

			S.Globals.Set("o", UserData.CreateStatic<VSomeType>());

			DynValue res = S.DoString("return o.SomeNestedType:Get()");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Ciao from SomeNestedType", res.String);
		}


		[Test]
		public void Interop_NestedTypes_Private_Val()
		{
			Script S = new Script();

			UserData.RegisterType<VSomeType>();

			S.Globals.Set("o", UserData.CreateStatic<VSomeType>());

			DynValue res = S.DoString("return o.SomeNestedTypePrivate:Get()");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Ciao from SomeNestedTypePrivate", res.String);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_NestedTypes_Private_Val_2()
		{
			Script S = new Script();

			UserData.RegisterType<VSomeType>();

			S.Globals.Set("o", UserData.CreateStatic<VSomeType>());

			DynValue res = S.DoString("return o.SomeNestedTypePrivate2:Get()");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("Ciao from SomeNestedTypePrivate2", res.String);
		}



	}
}
