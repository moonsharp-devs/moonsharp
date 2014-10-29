using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	internal class Instruction
	{
		public OpCode OpCode;
		public SymbolRef Symbol;
		public SymbolRef[] SymbolList;
		public string Name;
		public DynValue Value;
		public int NumVal;
		public int NumVal2;
		public bool Breakpoint;
		public SourceRef SourceCodeRef;

		public Instruction(SourceRef sourceref)
		{
			SourceCodeRef = sourceref;
		}

		public override string ToString()
		{
			string append = "";

			switch (OpCode)
			{
				case OpCode.Closure:
					append = string.Format("{0}{1:X8}({2})", GenSpaces(), NumVal, string.Join(",", SymbolList.Select(s => s.ToString()).ToArray()));
					break;
				case OpCode.Args:
					append = string.Format("{0}({1})", GenSpaces(), string.Join(",", SymbolList.Select(s => s.ToString()).ToArray()));
					break;
				case OpCode.Debug:
					return string.Format("[[ {0} ]]", Name);
				case OpCode.Literal:
				case OpCode.Index:
					append = string.Format("{0}{1}", GenSpaces(), PurifyFromNewLines(Value));
					break;
				case OpCode.IndexSet:
					append = string.Format("{0}{1} <- {2}:{3}", GenSpaces(), PurifyFromNewLines(Value), NumVal, NumVal2);
					break;
				case OpCode.Nop:
					append = string.Format("{0}#{1}", GenSpaces(), Name);
					break;
				case OpCode.Call:
				case OpCode.Ret:
				case OpCode.MkTuple:
				case OpCode.ExpTuple:
				case OpCode.Incr:
				case OpCode.Pop:
				case OpCode.Copy:
					append = string.Format("{0}{1}", GenSpaces(), NumVal);
					break;
				case OpCode.Enter:
				case OpCode.Leave:
				case OpCode.Exit:
				case OpCode.Swap:
					append = string.Format("{0}{1},{2}", GenSpaces(), NumVal, NumVal2);
					break;
				case OpCode.BeginFn:
					append = string.Format("{0}{1}:{2},{3}", GenSpaces(), Name, NumVal, NumVal2);
					break;
				case OpCode.Local:
				case OpCode.Upvalue:
					append = string.Format("{0}{1}", GenSpaces(), Symbol);
					break;
				case OpCode.StoreUpv:
				case OpCode.StoreLcl:
					append = string.Format("{0}{1} <- {2}:{3}", GenSpaces(), Symbol, NumVal, NumVal2);
					break;
				case OpCode.JtOrPop:
				case OpCode.JfOrPop:
				case OpCode.Jf:
				case OpCode.Jump:
				case OpCode.JFor:
				case OpCode.JNil:
					append = string.Format("{0}{1:X8}", GenSpaces(), NumVal);
					break;
				case OpCode.Invalid:
					append = string.Format("{0}{1}", GenSpaces(), Name ?? "(null)");
					break;
				default:
					break;
			}

			return this.OpCode.ToString().ToUpperInvariant() + append;
		}

		private string PurifyFromNewLines(DynValue Value)
		{
			if (Value == null)
				return "";

			return Value.ToString().Replace('\n', ' ').Replace('\r', ' ');
		}

		private string GenSpaces()
		{
			return new string(' ', 10 - this.OpCode.ToString().Length);
		}

		

	}
}
