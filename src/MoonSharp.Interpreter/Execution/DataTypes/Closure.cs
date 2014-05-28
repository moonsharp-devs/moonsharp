using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class Closure
	{
		public int ByteCodeLocation { get; private set; }

		private List<RValue> closureValues = null;

		private static List<RValue> emptyClosure = new List<RValue>();


		public Closure(int idx, LRef[] symbols, RuntimeScope scope)
		{
			ByteCodeLocation = idx;

			if (symbols.Length > 0)
				closureValues = symbols.Select(s => scope.Get(s)).ToList();
			else
				closureValues = emptyClosure;
		}

		public void EnterClosureBeforeCall(RuntimeScope scope)
		{
			scope.EnterClosure(closureValues);
		}

	}
}
