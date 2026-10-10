using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.Units
{
	[TestFixture]
	public class InteropTests
	{
		[Test]
		public void Converter_FromObject()
		{
			//DynValue v;
			//int? x = 3;
			//int? y = null;

			//v = Converter.FromObject(1);
			//v = Converter.FromObject(x);
			//v = Converter.FromObject(y);




		}

		[Test]
		public void Converter_ClrToScriptGenericTypeDefinition()
		{
			try
			{
				Script.GlobalOptions.CustomConverters.Clear();
				Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(
					typeof(GenericConversionTarget<>),
					(_s, obj) => DynValue.NewString(obj.GetType().GetGenericArguments()[0].Name + ":" + ((IGenericConversionTarget)obj).Value));

				Script script = new Script();
				DynValue value = DynValue.FromObject(script, new GenericConversionTarget<int>("ok"));

				Assert.AreEqual(DataType.String, value.Type);
				Assert.AreEqual("Int32:ok", value.String);
			}
			finally
			{
				Script.GlobalOptions.CustomConverters.Clear();
			}
		}

		private interface IGenericConversionTarget
		{
			string Value { get; }
		}

		private class GenericConversionTarget<T> : IGenericConversionTarget
		{
			public GenericConversionTarget(string value)
			{
				Value = value;
			}

			public string Value { get; private set; }
		}

	}
}
