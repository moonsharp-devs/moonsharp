using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class Closure
	{
		public int ByteCodeLocation { get; private set; }

		private ClosureContext closureCtx = null;

		private static ClosureContext emptyClosure = new ClosureContext();


		internal Closure(int idx, LRef[] symbols, RuntimeScope scope)
		{
			ByteCodeLocation = idx;

			if (symbols.Length > 0)
				closureCtx = new ClosureContext(symbols, symbols.Select(s => scope.Get(s)));
			else
				closureCtx = emptyClosure;
		}

		internal void EnterClosureBeforeCall(RuntimeScope scope)
		{
			scope.EnterClosure(closureCtx);
		}

	}
}
