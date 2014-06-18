using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.DataTypes
{
	public struct TablePair
	{
		private static TablePair s_NilNode = new TablePair(DynValue.Nil, DynValue.Nil);

		private DynValue key, value;

		public DynValue Key 
		{
			get { return key; }
			private set { Key = key; }
		}

		public DynValue Value
		{
			get { return value; }
			set { if (key.Type != DataType.Nil) Value = value; }
		}


		public TablePair(DynValue key, DynValue val) 
		{
			this.key = key;
			this.value = val;
		}

		public static TablePair Nil { get { return s_NilNode; } }
	}
}
