using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	/// <summary>
	/// This class stores a possible l-value (that is a potential target of an assignment)
	/// </summary>
	public class SymbolRef
	{
		// Fields are internal - direct access by the executor was a 10% improvement at profiling here!
		internal SymbolRefType i_Type;
		internal int i_Index;
		internal string i_Name;
		internal DynValue i_TableRefObject;
		internal DynValue i_TableRefIndex;

		public SymbolRefType Type { get { return i_Type; } }
		public int Index { get { return i_Index; } }
		public string Name { get { return i_Name; } }
		public DynValue TableRefObject { get { return i_TableRefObject; } }
		public DynValue TableRefIndex { get { return i_TableRefIndex; } } 



		public static SymbolRef Global(string name)
		{
			return new SymbolRef() { i_Index = -1, i_Type = SymbolRefType.Global, i_Name = name };
		}

		public static SymbolRef Local(string name, int index)
		{
			return new SymbolRef() { i_Index = index, i_Type = SymbolRefType.Local, i_Name = name };
		}

		public static SymbolRef Upvalue(string name, int index)
		{
			return new SymbolRef() { i_Index = index, i_Type = SymbolRefType.Upvalue, i_Name = name };
		}
		public static SymbolRef Argument(string name, int index)
		{
			return new SymbolRef() { i_Index = index, i_Type = SymbolRefType.Argument, i_Name = name };
		}

		public static SymbolRef Invalid()
		{
			return new SymbolRef() { i_Index = -1, i_Type = SymbolRefType.Invalid, i_Name = "!INV!" };
		}

		public static SymbolRef ObjIndex(DynValue baseObject, DynValue indexObject)
		{
			return new SymbolRef() { i_TableRefObject = baseObject, i_TableRefIndex = indexObject, i_Type = SymbolRefType.Index };
		}

		public bool IsValid()
		{
			return i_Type !=  SymbolRefType.Invalid;
		}


		public override string ToString()
		{
			return string.Format("{0}[{1}] : {2}", i_Type, i_Index, i_Name);
		}




	}
}
