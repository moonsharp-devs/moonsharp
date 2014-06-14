using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.DataTypes
{
	public struct TablePair
	{
		private static TablePair s_NilNode = new TablePair(RValue.Nil, RValue.Nil);

		private RValue key, value;

		public RValue Key 
		{
			get { return key; }
			private set { Key = key; }
		}

		public RValue Value
		{
			get { return value; }
			set { if (key.Type != DataType.Nil) Value = value; }
		}


		public TablePair(RValue key, RValue val) 
		{
			this.key = key;
			this.value = val;
		}

		public static TablePair Nil { get { return s_NilNode; } }
	}
}
