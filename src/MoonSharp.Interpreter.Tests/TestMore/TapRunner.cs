using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	public class TapRunner
	{
		string m_File;

		public DynValue Print(IExecutionContext exctx, CallbackArguments values)
		{
			string str = string.Join(" ", values.List.Select(s => s.ToPrintString()).ToArray());
			Assert.IsFalse(str.Trim().StartsWith("not ok"), string.Format("TAP fail ({0}) : {1}", m_File, str));
			return DynValue.Nil;
		}

		public TapRunner(string filename)
		{
			m_File = filename;
		}

		public void Run()
		{
			string script = File.ReadAllText(m_File);
			var globalCtx = new Table();
			globalCtx["print"] = DynValue.NewCallback(Print);
			globalCtx["arg"] = DynValue.NewTable();
			globalCtx.RegisterModuleType<TableIterators>();
			globalCtx.RegisterModuleType<MetaTableMethods>();
			(new Script(globalCtx)).DoString(script);
		}

		public static void Run(string filename)
		{
			TapRunner t = new TapRunner(filename);
			t.Run();
		}



	}
}
