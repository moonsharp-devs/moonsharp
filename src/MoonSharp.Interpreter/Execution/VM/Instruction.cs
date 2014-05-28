using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class Instruction
	{
		public OpCode OpCode;
		public SymbolRef Symbol;
		public SymbolRef[] SymbolList;
		public string Name;
		public RValue Value;
		public int NumVal;
		public int NumVal2;
		public RuntimeScopeFrame Frame;

		public override string ToString()
		{
			string append = "";

			switch (OpCode)
			{
				case OpCode.Debug:
					return string.Format("[[ {0} ]]", Name);
				case OpCode.Load:
				case OpCode.Symbol:
				case OpCode.NSymStor:
					append = string.Format("{0}{1}", GenSpaces(), Symbol);
					break;
				case OpCode.Literal:
					append = string.Format("{0}{1}", GenSpaces(), PurifyFromNewLines(Value));
					break;
				case OpCode.Nop:
					append = string.Format("{0}// {1}", GenSpaces(), Name);
					break;
				case OpCode.Call:
				case OpCode.Ret:
				case OpCode.MkTuple:
				case OpCode.Incr:
				case OpCode.Pop:
				case OpCode.TmpClear:
				case OpCode.TmpPush:
				case OpCode.TmpPop:
				case OpCode.TmpPeek:
					append = string.Format("{0}{1}", GenSpaces(), NumVal);
					break;
				case OpCode.JtOrPop:
				case OpCode.JfOrPop:
				case OpCode.Jf:
				case OpCode.Jump:
				case OpCode.JFor:
					append = string.Format("{0}{1:X8}", GenSpaces(), NumVal);
					break;
				case OpCode.Invalid:
					append = string.Format("{0}{1}", GenSpaces(), Name ?? "(null)");
					break;
				case OpCode.Assign:
					append = string.Format("{0}{1},{2}", GenSpaces(), NumVal, NumVal2);
					break;
				default:
					break;
			}

			return this.OpCode.ToString().ToLowerInvariant() + append;
		}

		private string PurifyFromNewLines(RValue Value)
		{
			return Value.ToString().Replace('\n', ' ').Replace('\r', ' ');
		}

		private string GenSpaces()
		{
			return new string(' ', 9 - this.OpCode.ToString().Length);
		}


	}
}
