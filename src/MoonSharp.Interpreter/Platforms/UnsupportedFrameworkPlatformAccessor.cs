using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	public class UnsupportedFrameworkPlatformAccessor: LimitedPlatformAccessorBase
	{
		string m_FrameworkName;

		public UnsupportedFrameworkPlatformAccessor(string frameworkname)
		{
			m_FrameworkName = frameworkname;
		}


		public override bool ScriptFileExists(string name)
		{
			return false;
		}

		public override object OpenScriptFile(Script script, string file, Table globalContext)
		{
			var error = string.Format("Loading scripts from files is not automatically supported on {0}. Please implement your own IPlatformAccessor (possibly, inheriting from LimitedPlatformAccessorBase for easier implementation)", m_FrameworkName);
			throw new PlatformNotSupportedException(error);
		}

		public override string GetPlatformNamePrefix()
		{
			return "inv";
		}

		public override void DefaultPrint(string content)
		{
			System.Diagnostics.Debug.WriteLine(content);
		}
	}
}
