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
	public class LRef
	{
		// Fields are internal - direct access by the executor was a 10% improvement at profiling here!
		internal LRefType i_Type;
		internal int i_Index;
		internal string i_Name;
		internal RValue i_TableRefObject;
		internal RValue i_TableRefIndex;

		public LRefType Type { get { return i_Type; } }
		public int Index { get { return i_Index; } }
		public string Name { get { return i_Name; } }
		public RValue TableRefObject { get { return i_TableRefObject; } }
		public RValue TableRefIndex { get { return i_TableRefIndex; } } 



		public static LRef Global(string name)
		{
			return new LRef() { i_Index = -1, i_Type = LRefType.Global, i_Name = name };
		}

		public static LRef Local(string name, int index)
		{
			return new LRef() { i_Index = index, i_Type = LRefType.Local, i_Name = name };
		}

		public static LRef Upvalue(string name, int index)
		{
			return new LRef() { i_Index = index, i_Type = LRefType.Upvalue, i_Name = name };
		}
		public static LRef Argument(string name, int index)
		{
			return new LRef() { i_Index = index, i_Type = LRefType.Argument, i_Name = name };
		}

		public static LRef Invalid()
		{
			return new LRef() { i_Index = -1, i_Type = LRefType.Invalid, i_Name = "!INV!" };
		}

		public static LRef ObjIndex(RValue baseObject, RValue indexObject)
		{
			return new LRef() { i_TableRefObject = baseObject, i_TableRefIndex = indexObject, i_Type = LRefType.Index };
		}

		public bool IsValid()
		{
			return i_Type !=  LRefType.Invalid;
		}


		public override string ToString()
		{
			return string.Format("{0}[{1}] : {2}", i_Type, i_Index, i_Name);
		}




	}
}
