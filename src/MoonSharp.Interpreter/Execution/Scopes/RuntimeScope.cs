using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class RuntimeScope
	{
		List<RValue> m_GlobalScope = new List<RValue>(131072); // start with a 512KB scope stack
		List<RValue> m_ScopeStack = new List<RValue>(131072); // start with a 512KB scope stack
		List<LRef> m_DebugStack = new List<LRef>(131072); // start with a 512KB scope stack
		List<int> m_LocalBaseIndexes = new List<int>(16384);
		List<List<RValue>> m_ClosureStack = new List<List<RValue>>();
		List<RuntimeScopeFrame> m_ScopeFrames = new List<RuntimeScopeFrame>(2048);

		public void EnterClosure(List<RValue> closureValues)
		{
			m_ClosureStack.Add(closureValues);
		}

		public void LeaveClosure()
		{
			m_ClosureStack.RemoveAt(m_ClosureStack.Count - 1);
		}

		public void PushFrame(RuntimeScopeFrame frame)
		{
			int size = frame.Count;

			m_ScopeFrames.Add(frame);

			if (frame.RestartOfBase)
				m_LocalBaseIndexes.Add(m_ScopeStack.Count);

			for (int i = 0; i < size; i++)
			{
				m_ScopeStack.Add(new RValue());
				m_DebugStack.Add(frame.m_DebugSymbols[i]);
			}

			// Debug.WriteLine(string.Format("RPush : {0} - Stack is {1}", frame, m_ScopeStack.Count));
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
				m_ScopeStack.RemoveRange(m_ScopeStack.Count - size, size);
				m_DebugStack.RemoveRange(m_DebugStack.Count - size, size);
			}

			if (frame.RestartOfBase)
			{
				m_LocalBaseIndexes.RemoveAt(m_LocalBaseIndexes.Count - 1);
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
						return m_GlobalScope[symref.i_Index] ?? RValue.Nil;
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
						m_GlobalScope[symref.i_Index] = value.CloneAsWritable();
					}
					break;
				case LRefType.Local:
					{
						int lastBaseIdx = m_LocalBaseIndexes[m_LocalBaseIndexes.Count - 1];
						m_ScopeStack[lastBaseIdx + symref.i_Index].Assign(value);
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

		public void ExpandGlobal(int maxidx)
		{
			if (m_GlobalScope.Count > 0)
				throw new ScriptRuntimeException(null, "INTERNAL ERROR");

			for (int i = 0; i <= maxidx; i++)
				m_GlobalScope.Add(RValue.Nil);
		}




	}
}
