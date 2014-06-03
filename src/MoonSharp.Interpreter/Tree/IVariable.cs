using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Tree
{
	interface IVariable
	{
		void CompileAssignment(Execution.VM.Chunk bc);
	}
}
