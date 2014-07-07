using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class Chunk
	{
		public SourceCode SourceCode { get; private set; }
		public ByteCode ByteCode { get; private set; }
	}
}
