using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class ConfigPropertyAssignerTests
	{
		private class MySubclass
		{
			[MoonSharpProperty]
			public string MyString { get; set; }

			[MoonSharpProperty("number")]
			public int MyNumber { get; private set; }
		}

		private class MyClass
		{
			[MoonSharpProperty]
			public string MyString { get; set; }

			[MoonSharpProperty("number")]
			public int MyNumber { get; private set; }

			[MoonSharpProperty]
			internal Table SomeTable { get; private set; }

			[MoonSharpProperty]
			public DynValue NativeValue { get; private set; }

			[MoonSharpProperty]
			public MySubclass SubObj { get; private set; }
		}

		private MyClass Test(string tableDef)
		{
			Script s = new Script(CoreModules.None);

			DynValue table = s.DoString("return " + tableDef);

			Assert.AreEqual(DataType.Table, table.Type);

			PropertyTableAssigner<MyClass> pta = new PropertyTableAssigner<MyClass>("class");
			PropertyTableAssigner<MySubclass> pta2 = new PropertyTableAssigner<MySubclass>();

			pta.SetSubassigner(pta2);

			MyClass o = new MyClass();

			pta.AssignObject(o, table.Table);

			return o;
		}

		[Test]
		public void ConfigProp_SimpleAssign()
		{
			MyClass x = Test(@"
				{
				class = 'oohoh',
				myString = 'ciao',
				number = 3,
				some_table = {},
				nativeValue = function() end,
				subObj = { number = 15, myString = 'hi' },
				}");

			Assert.AreEqual(x.MyNumber, 3);
			Assert.AreEqual(x.MyString, "ciao");
			Assert.AreEqual(x.NativeValue.Type, DataType.Function);
			Assert.AreEqual(x.SubObj.MyNumber, 15);
			Assert.AreEqual(x.SubObj.MyString, "hi");
			Assert.IsNotNull(x.SomeTable);

		}


		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void ConfigProp_ThrowsOnInvalid()
		{
			MyClass x = Test(@"
				{
				class = 'oohoh',
				myString = 'ciao',
				number = 3,
				some_table = {},
				invalid = 3,
				nativeValue = function() end,
				}");

			Assert.AreEqual(x.MyNumber, 3);
			Assert.AreEqual(x.MyString, "ciao");
			Assert.AreEqual(x.NativeValue.Type, DataType.Function);
			Assert.IsNotNull(x.SomeTable);

		}

	}
}
