using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	public class PortableFrameworkAccessor: LimitedPlatformAccessorBase
	{
		public override bool ScriptFileExists(string name)
		{
			return true;
		}

		public override object OpenScriptFile(Script script, string file, Table globalContext)
		{
			throw new NotImplementedException();
		}

		public override string GetPlatformNamePrefix()
		{
			return "pstd";
		}

		public override void DefaultPrint(string content)
		{
			System.Diagnostics.Debug.WriteLine(content);
		}
	}
}
