using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor
	{

		public void EnterClosure(ClosureContext closureValues)
		{
			m_ClosureStack.Push(closureValues);
		}

		public void LeaveClosure()
		{
			m_ClosureStack.RemoveLast();
		}

		private void NilifyBlockData(RuntimeScopeBlock runtimeScopeBlock)
		{
			int from = runtimeScopeBlock.From;
			int to = runtimeScopeBlock.To;

			var array = this.m_ExecutionStack.Peek().LocalScope;

			if (to >= 0 && from >= 0)
			{
				for (int i = from; i <= to; i++)
					array[i] = new RValue();
			}
		}

		private void ClearBlockData(RuntimeScopeBlock runtimeScopeBlock, bool clearToInclusive)
		{
			int from = runtimeScopeBlock.From;
			int to = clearToInclusive ? runtimeScopeBlock.ToInclusive : runtimeScopeBlock.To;

			var array = this.m_ExecutionStack.Peek().LocalScope;

			if (to >= 0 && from >= 0)
			{
				Array.Clear(array, from, to - from + 1);
			}
		}


		public RValue GetGenericSymbol(LRef symref)
		{
			switch (symref.i_Type)
			{
				case LRefType.Global:
					return m_GlobalTable[symref.i_Name];
				case LRefType.Local:
					return m_ExecutionStack.Peek().LocalScope[symref.i_Index];
				case LRefType.Upvalue:
					List<RValue> closureValues = m_ClosureStack.Count > 0 ? m_ClosureStack[m_ClosureStack.Count - 1] : null;

					if (closureValues != null)
					{
						return closureValues[symref.i_Index];
					}
					else
					{
						throw new ScriptRuntimeException(null, "Invalid upvalue at resolution: {0}", symref.i_Name);
					}
				case LRefType.Index:
				case LRefType.Invalid:
				default:
					throw new InternalErrorException("Unexpected {0} LRef at resolution: {1}", symref.i_Type, symref.i_Name);
			}
		}

		public void AssignGenericSymbol(LRef symref, RValue value)
		{
			switch (symref.i_Type)
			{
				case LRefType.Global:
					m_GlobalTable[symref.i_Name] = value.CloneAsWritable();
					break;
				case LRefType.Local:
					{
						var stackframe = m_ExecutionStack.Peek();

						RValue v = stackframe.LocalScope[symref.i_Index];
						if (v == null)
							stackframe.LocalScope[symref.i_Index] = v = new RValue();

						v.Assign(value);
					}
					break;
				case LRefType.Upvalue:
					{
						List<RValue> closureValues = m_ClosureStack.Count > 0 ? m_ClosureStack[m_ClosureStack.Count - 1] : null;

						if (closureValues != null)
						{
							closureValues[symref.i_Index].Assign(value);
						}
						else
						{
							throw new ScriptRuntimeException(null, "Invalid upvalue at resolution: {0}", symref.i_Name);
						}
					}
					break;
				case LRefType.Index:
				case LRefType.Invalid:
				default:
					throw new InternalErrorException("Unexpected {0} LRef at resolution: {1}", symref.i_Type, symref.i_Name);
			}
		}

		public LRef FindRefByName(string name)
		{
			var stackframe = m_ExecutionStack.Peek();

			if (stackframe.Debug_Symbols != null)
			{
				for (int i = stackframe.Debug_Symbols.Length - 1; i >= 0; i--)
				{
					var l = stackframe.Debug_Symbols[i];

					if (l.i_Name == name && stackframe.LocalScope[i] != null)
						return l;
				}
			}

			if (m_ClosureStack.Count > 0)
			{
				var closure = m_ClosureStack.Peek(0);

				for (int i = 0; i < closure.Symbols.Length; i++)
					if (closure.Symbols[i] == name)
					{
						return LRef.Upvalue(name, i);
					}
			}

			if (m_GlobalTable.HasStringSymbol(name))
				return LRef.Global(name);

			return null;
		}

	}
}
