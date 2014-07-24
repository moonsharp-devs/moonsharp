using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// This class stores a possible l-value (that is a potential target of an assignment)
	/// </summary>
	public class SymbolRef
	{
		internal SymbolRef i_Env;

		// Fields are internal - direct access by the executor was a 10% improvement at profiling here!
		internal SymbolRefType i_Type;
		internal int i_Index;
		internal string i_Name;

		public SymbolRefType Type { get { return i_Type; } }
		public int Index { get { return i_Index; } }
		public string Name { get { return i_Name; } }
		public SymbolRef Environment { get { return i_Env; } }


		public static SymbolRef Global(string name, SymbolRef envSymbol)
		{
			return new SymbolRef() { i_Index = -1, i_Type = SymbolRefType.Global, i_Env = envSymbol, i_Name = name };
		}

		public static SymbolRef Local(string name, int index)
		{
			//Debug.Assert(index >= 0, "Symbol Index < 0");
			return new SymbolRef() { i_Index = index, i_Type = SymbolRefType.Local, i_Name = name };
		}

		public static SymbolRef Upvalue(string name, int index)
		{
			//Debug.Assert(index >= 0, "Symbol Index < 0");
			return new SymbolRef() { i_Index = index, i_Type = SymbolRefType.Upvalue, i_Name = name };
		}

		public override string ToString()
		{
			if (i_Type == SymbolRefType.Global)
				return string.Format("{2} : {0} / {1}", i_Type, i_Env, i_Name);
			else
				return string.Format("{2} : {0}[{1}]", i_Type, i_Index, i_Name);
		}
	}
}
