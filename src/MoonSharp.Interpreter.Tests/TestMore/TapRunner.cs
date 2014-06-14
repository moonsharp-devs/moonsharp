using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	public class TapRunner
	{
		string m_File;

		public RValue Print(IList<RValue> values)
		{
			string str = string.Join(" ", values.Select(s => s.AsString()).ToArray());
			Assert.IsFalse(str.Trim().StartsWith("not ok"), string.Format("TAP fail ({0}) : {1}", m_File, str));
			return RValue.Nil;
		}

		public TapRunner(string filename)
		{
			m_File = filename;
		}

		public void Run()
		{
			string script = File.ReadAllText(m_File);
			var globalCtx = new Table();
			globalCtx[new RValue("print")] = new RValue(new CallbackFunction(Print));
			globalCtx[new RValue("arg")] = new RValue(new Table());
			MoonSharpInterpreter.LoadFromString(script).Execute(globalCtx);
		}

		public static void Run(string filename)
		{
			TapRunner t = new TapRunner(filename);
			t.Run();
		}



	}
}
