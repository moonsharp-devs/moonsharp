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

		public void Print(string str)
		{
			Assert.IsFalse(str.Trim().StartsWith("not ok"), string.Format("TAP fail ({0}) : {1}", m_File, str));
		}

		public TapRunner(string filename)
		{
			m_File = filename;
		}

		public void Run()
		{
			Script S = new Script();

			//S.Globals["print"] = DynValue.NewCallback(Print);
			S.Options.DebugPrint = Print;

			S.Options.UseLuaErrorLocations = true;

			S.Globals.Set("arg", DynValue.NewTable(S));

			ClassicLuaScriptLoader L = S.Options.ScriptLoader as ClassicLuaScriptLoader;

			if (L == null)
			{
				L = new ClassicLuaScriptLoader();
				S.Options.ScriptLoader = L;
			}

			L.ModulePaths = L.UnpackStringPaths("TestMore/Modules/?;TestMore/Modules/?.lua");
			S.DoFile(m_File);
		}

		public static void Run(string filename)
		{
			TapRunner t = new TapRunner(filename);
			t.Run();
		}



	}
}
