using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataTypes;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	public class Closure : IScriptPrivateResource
	{
		public int ByteCodeLocation { get; private set; }

		public ClosureContext ClosureContext { get; private set; }

		public Script OwnerScript { get; private set; }


		private static ClosureContext emptyClosure = new ClosureContext();


		internal Closure(Script script, int idx, SymbolRef[] symbols, IEnumerable<DynValue> resolvedLocals)
		{
			OwnerScript = script;

			ByteCodeLocation = idx;

			if (symbols.Length > 0)
				ClosureContext = new ClosureContext(symbols, resolvedLocals);
			else
				ClosureContext = emptyClosure;
		}

	}
}
