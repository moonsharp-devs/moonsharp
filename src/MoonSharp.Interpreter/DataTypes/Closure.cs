using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	public class Closure
	{
		public int ByteCodeLocation { get; private set; }

		public ClosureContext ClosureContext { get; private set; }

		public Table GlobalEnv { get; set; }

		private static ClosureContext emptyClosure = new ClosureContext();


		internal Closure(int idx, SymbolRef[] symbols, DynValue[] localscope)
		{
			ByteCodeLocation = idx;

			if (symbols.Length > 0)
				ClosureContext = new ClosureContext(symbols, symbols.Select(s => localscope[s.i_Index]));
			else
				ClosureContext = emptyClosure;
		}
	}
}
