using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{

	class VmStackFrame
	{
		public int ReturnIndex;
		public Chunk Chunk;
	}

}
