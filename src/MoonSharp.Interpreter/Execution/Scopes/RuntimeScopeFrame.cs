using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class RuntimeScopeFrame 
	{
		public List<LRef> DebugSymbols { get; private set; }
		public int Count { get { return DebugSymbols.Count; } }
		public int ToFirstBlock { get; internal set; }

		public RuntimeScopeFrame()
		{
			DebugSymbols = new List<LRef>();
		}

		public override string ToString()
		{
			return string.Format("ScopeFrame : #{0}", Count);
		}
	}
}
