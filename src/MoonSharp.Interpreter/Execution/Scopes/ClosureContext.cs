using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class ClosureContext : List<DynValue>
	{
		public enum UpvaluesType
		{
			None,
			Environment,
			Closure
		}


		public string[] Symbols { get; private set; }

		internal ClosureContext(SymbolRef[] symbols, IEnumerable<DynValue> values)
		{
			Symbols = symbols.Select(s => s.i_Name).ToArray();
			this.AddRange(values);
		}

		internal ClosureContext()
		{
			Symbols = new string[0];
		}

		public UpvaluesType GetUpvaluesType()
		{
			if (Symbols.Length == 0)
				return UpvaluesType.None;
			else if (Symbols.Length == 1 && Symbols[0] == WellKnownSymbols.ENV)
				return UpvaluesType.Environment;
			else
				return UpvaluesType.Closure;
		}
	}
}
