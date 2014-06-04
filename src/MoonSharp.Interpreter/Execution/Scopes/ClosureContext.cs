using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class ClosureContext : List<RValue>
	{
		public string[] Symbols { get; private set; }

		public ClosureContext(LRef[] symbols, IEnumerable<RValue> values)
		{
			Symbols = symbols.Select(s => s.i_Name).ToArray();
			this.AddRange(values);
		}

		public ClosureContext()
		{
			Symbols = new string[0];
		}
	}
}
