using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// This class stores a possible l-value (that is a potential target of an assignment)
	/// </summary>
	public class SymbolRef
	{
		private static SymbolRef s_DefaultEnv = new SymbolRef() { i_Type = SymbolRefType.DefaultEnv };

		// Fields are internal - direct access by the executor was a 10% improvement at profiling here!
		internal SymbolRefType i_Type;
		internal SymbolRef i_Env;
		internal int i_Index;
		internal string i_Name;

		public SymbolRefType Type { get { return i_Type; } }
		public int Index { get { return i_Index; } }
		public string Name { get { return i_Name; } }
		public SymbolRef Environment { get { return i_Env; } }


		public static SymbolRef DefaultEnv { get { return s_DefaultEnv; } }

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
			if (i_Type == SymbolRefType.DefaultEnv)
				return "(default _ENV)";
			else
			if (i_Type == SymbolRefType.Global)
				return string.Format("{2} : {0} / {1}", i_Type, i_Env, i_Name);
			else
				return string.Format("{2} : {0}[{1}]", i_Type, i_Index, i_Name);
		}

		internal void WriteBinary(BinaryWriter bw)
		{
			bw.Write((byte)this.i_Type);
			bw.Write(i_Index);
			bw.Write(i_Name);
		}

		internal static SymbolRef ReadBinary(BinaryReader br)
		{
			SymbolRef that = new SymbolRef();
			that.i_Type = (SymbolRefType)br.ReadByte();
			that.i_Index = br.ReadInt32();
			that.i_Name = br.ReadString();
			return that;
		}

		internal void WriteBinaryEnv(BinaryWriter bw, Dictionary<SymbolRef, int> symbolMap)
		{
			if (this.i_Env != null)
				bw.Write(symbolMap[i_Env]);
			else
				bw.Write(-1);
		}

		internal void ReadBinaryEnv(BinaryReader br, SymbolRef[] symbolRefs)
		{
			int idx = br.ReadInt32();

			if (idx >= 0)
				i_Env = symbolRefs[idx];
		}
	}
}
