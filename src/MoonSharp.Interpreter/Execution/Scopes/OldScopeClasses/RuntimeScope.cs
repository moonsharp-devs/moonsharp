using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter.Execution
{
	public class RuntimeScope_UNUSED
	{
		Table m_GlobalTable;
		FastStack<RValue> m_ScopeStack = new FastStack<RValue>(131072); // start with a 512KB scope stack
		FastStack<int> m_LocalBaseIndexes = new FastStack<int>(16384);
		FastStack<ClosureContext> m_ClosureStack = new FastStack<ClosureContext>(4096);
		FastStack<RuntimeScopeFrame_UNUSED> m_ScopeFrames = new FastStack<RuntimeScopeFrame_UNUSED>(8192);

		public RuntimeScope_UNUSED()
		{
		}

		public Table GlobalTable
		{
			get { return m_GlobalTable; }
			set { m_GlobalTable = value; }
		}

		public void EnterClosure(ClosureContext closureValues)
		{
			m_ClosureStack.Push(closureValues);
		}

		public void LeaveClosure()
		{
			m_ClosureStack.RemoveLast();
		}

		public void PushFrame(RuntimeScopeFrame_UNUSED frame)
		{
			int size = frame.Count;

			m_ScopeFrames.Push(frame);

			if (frame.RestartOfBase)
				m_LocalBaseIndexes.Push(m_ScopeStack.Count);

			m_ScopeStack.Expand(size);

		}

		public void PopFrame(RuntimeScopeFrame_UNUSED frame)
		{
			System.Diagnostics.Debug.Assert(frame == m_ScopeFrames.Peek());
			PopFrame();
		}


		public RuntimeScopeFrame_UNUSED PopFrame()
		{
			RuntimeScopeFrame_UNUSED frame = m_ScopeFrames.Pop();

			int size = frame.Count;
			if (size > 0)
			{
				m_ScopeStack.RemoveLast(size);
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

		public void PopFramesToFrame(RuntimeScopeFrame_UNUSED runtimeScopeFrame)
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
						var v = m_ScopeStack[lastBaseIdx + symref.i_Index];
						if (v == null)
							m_ScopeStack[lastBaseIdx + symref.i_Index] = v = new RValue();
						return v;
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

		public LRef FindRefByName(string name)
		{
			for(int i = m_ScopeFrames.Count - 1; i >= 0; i--)
			{
				var frame = m_ScopeFrames[i];
				
				foreach(LRef l in frame.m_DebugSymbols)
					if (l.i_Name == name)
						return l;
			
				if (frame.RestartOfBase)
					break;
			}

			if (m_ClosureStack.Count > 0)
			{
				var closure = m_ClosureStack.Peek(0);

				for(int i = 0; i < closure.Symbols.Length; i++)
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
