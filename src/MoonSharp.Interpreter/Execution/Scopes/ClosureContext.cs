using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class ClosureContext : List<DynValue>
	{
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
	}
}
