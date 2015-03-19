using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Platforms;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
#if !EMBEDTEST
	class TestsPlatformAccessor : LimitedPlatformAccessorBase
	{
		public override bool ScriptFileExists(string name)
		{
			return File.Exists(name);
		}

		public override object OpenScriptFile(Script script, string file, Table globalContext)
		{
			return new FileStream(file, FileMode.Open, FileAccess.Read);
		}

		public override string GetPlatformNamePrefix()
		{
			return "tests";
		}

		public override void DefaultPrint(string content)
		{
			Debug.WriteLine("PRINTED : " + content);
		}
	}
#endif

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
#if PCL
	#if EMBEDTEST
			Script.Platform = new EmbeddedResourcePlatformAccessor(Assembly.GetExecutingAssembly());
	#else
			Script.Platform = new TestsPlatformAccessor();
	#endif
#endif

			Script S = new Script();

			S.Options.DebugPrint = Print;

			S.Options.UseLuaErrorLocations = true;

			S.Globals.Set("arg", DynValue.NewTable(S));

			S.Options.ModulesPaths = new string[] { "TestMore/Modules/?", "TestMore/Modules/?.lua" };

			S.DoFile(m_File);
		}

		public static void Run(string filename)
		{
			TapRunner t = new TapRunner(filename);
			t.Run();
		}



	}
}
