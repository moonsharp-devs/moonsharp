using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution
{
	class ScriptLoadingContext
	{
		public BuildTimeScope Scope { get; set; }
		public SourceCode Source { get; set; }


	}
}
