using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	interface IVariable
	{
		void SetValue(RuntimeScope scope, RValue rValue);

		void CompileAssignment(Execution.VM.Chunk bc);
	}
}
