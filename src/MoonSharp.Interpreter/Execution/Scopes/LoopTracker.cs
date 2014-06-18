using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Execution
{
	interface ILoop
	{
		void CompileBreak(ByteCode bc);
	}


	class LoopTracker
	{
		public FastStack<ILoop> Loops = new FastStack<ILoop>(16384);
	}
}
