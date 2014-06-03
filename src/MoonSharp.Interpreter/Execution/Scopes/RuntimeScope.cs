using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Execution
{
	public class RuntimeScope
	{
		Table m_GlobalTable;
		FastStack<RValue> m_ScopeStack = new FastStack<RValue>(131072); // start with a 512KB scope stack
//		FastStack<LRef> m_DebugStack = new FastStack<LRef>(131072); // start with a 512KB scope stack
		FastStack<int> m_LocalBaseIndexes = new FastStack<int>(16384);
		FastStack<List<RValue>> m_ClosureStack = new FastStack<List<RValue>>(4096);
		FastStack<RuntimeScopeFrame> m_ScopeFrames = new FastStack<RuntimeScopeFrame>(8192);

		public RuntimeScope()
		{
		}

		public Table GlobalTable
		{
			get { return m_GlobalTable; }
			set { m_GlobalTable = value; }
		}

		public void EnterClosure(List<RValue> closureValues)
		{
			m_ClosureStack.Push(closureValues);
		}

		public void LeaveClosure()
		{
			m_ClosureStack.RemoveLast();
		}

		public void PushFrame(RuntimeScopeFrame frame)
		{
			int size = frame.Count;

			m_ScopeFrames.Push(frame);

			if (frame.RestartOfBase)
				m_LocalBaseIndexes.Push(m_ScopeStack.Count);

			m_ScopeStack.Expand(size);
		}

		public void PopFrame(RuntimeScopeFrame frame)
		{
			System.Diagnostics.Debug.Assert(frame == m_ScopeFrames.Peek());
			PopFrame();
		}


		public RuntimeScopeFrame PopFrame()
		{
			RuntimeScopeFrame frame = m_ScopeFrames.Pop();

			int size = frame.Count;
			if (size > 0)
			{
				m_ScopeStack.RemoveLast(size);
				//m_DebugStack.RemoveLast(size);
			}

			if (frame.RestartOfBase)
			{
				m_LocalBaseIndexes.RemoveLast();
			}
			return frame;
		}

		public void PopFramesToFunction()
		{
			while (!PopFrame().RestartOfBase) ;
		}

		public void PopFramesToFrame(RuntimeScopeFrame runtimeScopeFrame)
		{
			while (m_ScopeFrames.Peek() != runtimeScopeFrame)
				PopFrame();
		}

		public RValue Get(LRef symref)
		{
			switch (symref.i_Type)
			{
				case LRefType.Global:
					{
						return m_GlobalTable[symref.i_Name];
					}
				case LRefType.Local:
					{
						int lastBaseIdx = m_LocalBaseIndexes[m_LocalBaseIndexes.Count - 1];
						return m_ScopeStack[lastBaseIdx + symref.i_Index] ?? RValue.Nil;
					}
				case LRefType.Upvalue:
					{
						List<RValue> closureValues = m_ClosureStack.Count > 0 ? m_ClosureStack[m_ClosureStack.Count - 1] : null;

						if (closureValues != null)
						{
							return closureValues[symref.i_Index];
						}
						else
						{
							throw new ScriptRuntimeException(null, "Invalid upvalue at resolution: {0}", symref.i_Name);
						}
					}
				case LRefType.Invalid:
				default:
					{
						throw new ScriptRuntimeException(null, "Invalid value at resolution: {0}", symref.i_Name);
					}
			}
		}


		public void Assign(LRef symref, RValue value)
		{
			// Debug.WriteLine(string.Format("Assigning {0} = {1}", symref, value));

			switch (symref.i_Type)
			{
				case LRefType.Global:
					{
						m_GlobalTable[symref.i_Name] = value.CloneAsWritable();
					}
					break;
				case LRefType.Local:
					{
						int lastBaseIdx = m_LocalBaseIndexes[m_LocalBaseIndexes.Count - 1];
						RValue v = m_ScopeStack[lastBaseIdx + symref.i_Index];
						if (v == null)
							m_ScopeStack[lastBaseIdx + symref.i_Index] = v = new RValue();
						v.Assign(value);
						//m_ScopeStack[lastBaseIdx + symref.Index] = value.CloneAsWritable();
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
				case LRefType.Invalid:
				default:
					{
						throw new ScriptRuntimeException(null, "Invalid value at resolution: {0}", symref.i_Name);
					}
			}
		}




	}
}
