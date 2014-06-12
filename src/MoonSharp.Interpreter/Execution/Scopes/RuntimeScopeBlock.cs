using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class RuntimeScopeBlock
	{
		public int From { get; internal set; }
		public int To { get; internal set; }
		public int ToInclusive { get; internal set; }

		public override string ToString()
		{
			return String.Format("ScopeBlock : {0} -> {1} --> {2}", From, To, ToInclusive);
		}
	}
}
