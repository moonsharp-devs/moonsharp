using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class SymbolRef
	{
		public SymbolRefType Type { get; private set; }

		public int Index { get; private set; }

		public string Name { get; private set; }

		public RValue TableRefObject { get; internal set; }
		public RValue TableRefIndex { get; internal set; }


		public static SymbolRef Global(string name, int index)
		{
			return new SymbolRef() { Index = index, Type = SymbolRefType.Global, Name = name };
		}

		public static SymbolRef Local(string name, int index)
		{
			return new SymbolRef() { Index = index, Type = SymbolRefType.Local, Name = name };
		}

		public static SymbolRef Upvalue(string name, int index)
		{
			return new SymbolRef() { Index = index, Type = SymbolRefType.Upvalue, Name = name };
		}

		public static SymbolRef Invalid()
		{
			return new SymbolRef() { Index = -1, Type = SymbolRefType.Invalid, Name = "!INV!" };
		}

		public static SymbolRef ObjIndex(RValue baseObject, RValue indexObject)
		{
			return new SymbolRef() { TableRefObject = baseObject, TableRefIndex = indexObject, Type = SymbolRefType.Index };
		}

		public bool IsValid()
		{
			return Index >= 0 && Type !=  SymbolRefType.Invalid;
		}

		private static void DebugPrint(SymbolRef s)
		{
			// Debug.WriteLine(string.Format("Defined: {0}", s));
		}

		public override string ToString()
		{
			return string.Format("{0}[{1}] : {2}", Type, Index, Name);
		}


	}
}
