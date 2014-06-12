using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class Closure
	{
		public int ByteCodeLocation { get; private set; }

		public ClosureContext ClosureContext { get; private set; }

		private static ClosureContext emptyClosure = new ClosureContext();


		internal Closure(int idx, LRef[] symbols, RValue[] localscope)
		{
			ByteCodeLocation = idx;

			if (symbols.Length > 0)
				ClosureContext = new ClosureContext(symbols, symbols.Select(s => localscope[s.i_Index]));
			else
				ClosureContext = emptyClosure;
		}
	}
}
