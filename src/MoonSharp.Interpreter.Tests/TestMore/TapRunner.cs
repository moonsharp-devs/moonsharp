using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Loaders;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	public class TapRunner
	{
		string m_File;

		public DynValue Print(ScriptExecutionContext exctx, CallbackArguments values)
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
			Script S = new Script();

			S.Globals["print"] = DynValue.NewCallback(Print);
			S.Globals["arg"] = DynValue.NewTable(S);

			var L = new ClassicLuaScriptLoader();
			L.ModulePaths = L.UnpackStringPaths("TestMore/Modules/?;TestMore/Modules/?.lua");
			S.ScriptLoader = L;
			S.DoFile(m_File);
		}

		public static void Run(string filename)
		{
			TapRunner t = new TapRunner(filename);
			t.Run();
		}



	}
}
