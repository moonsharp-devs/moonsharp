using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class Clr2Platform : Platform
	{
		public override string Name
		{
			get { return "Clr2"; }
		}

		public override string GetEnvironmentVariable(string variable)
		{
			return Environment.GetEnvironmentVariable(variable);
		}

		public override CoreModules FilterSupportedCoreModules(CoreModules module)
		{
			return module;
		}

	}
}
