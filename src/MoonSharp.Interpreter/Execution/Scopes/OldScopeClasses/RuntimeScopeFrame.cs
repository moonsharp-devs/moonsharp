using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class RuntimeScopeFrame_UNUSED
	{
		public List<LRef> m_DebugSymbols { get; private set;}
		public int Count { get; private set;}
		public bool RestartOfBase { get; private set; }


		public RuntimeScopeFrame_UNUSED(IEnumerable<LRef> symbols, int count, bool restartOfBase)
		{
			m_DebugSymbols = symbols.ToList();
			Count = count;
			RestartOfBase = restartOfBase;

			if (Count != m_DebugSymbols.Count)
				throw new Exception("WTF");
		}

		public override string ToString()
		{
			return string.Format("{0} frame : #{1}", RestartOfBase ? "Function" : "Block", Count);
		}
	}
}
