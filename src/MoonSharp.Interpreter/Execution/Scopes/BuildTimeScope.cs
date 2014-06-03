using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class BuildTimeScope
	{
		List<BuildTimeScopeFrame> m_Locals = new List<BuildTimeScopeFrame>();
		List<IClosureBuilder> m_ClosureBuilders = new List<IClosureBuilder>();

		public BuildTimeScope()
		{
			PushFunction();
		}

		public void EnterClosure(IClosureBuilder closureBuilder)
		{
			m_ClosureBuilders.Add(closureBuilder);
			closureBuilder.UpvalueCreationTag = (m_Locals.Count - 1);
		}

		public void LeaveClosure()
		{
			m_ClosureBuilders.RemoveAt(m_ClosureBuilders.Count - 1);
		}


		int GetStartIndexForPush()
		{
			return m_Locals[m_Locals.Count - 1].MaxIndex;
		}
		int GetBaseIndexForPush()
		{
			return m_Locals[m_Locals.Count - 1].BaseIndex;
		}

		public void PushBlock()
		{
			// Debug.WriteLine("PushBlock");
			m_Locals.Add(new BuildTimeScopeFrame(GetBaseIndexForPush(), GetStartIndexForPush(), false));
		}

		public void PushFunction()
		{
			// Debug.WriteLine("PushFunction");
			m_Locals.Add(new BuildTimeScopeFrame(0, 0, true));
		}

		RuntimeScopeFrame GetRuntimeFrameFromBuildFrame(BuildTimeScopeFrame frame, bool local)
		{
			List<LRef> symbols = new List<LRef>();
			for (int i = frame.StartIndex; i < frame.MaxIndex; i++)
			{
				LRef s;
				if (local)
					s = LRef.Local(frame.FindRev(i - frame.BaseIndex), i - frame.BaseIndex);
				else
					s = LRef.Global(frame.FindRev(i - frame.BaseIndex));

				symbols.Add(s);
			}

			return new RuntimeScopeFrame(symbols, frame.MaxIndex - frame.StartIndex, frame.Breaking);
		}

		public RuntimeScopeFrame Pop()
		{
			BuildTimeScopeFrame frame = m_Locals[m_Locals.Count - 1];
			m_Locals.RemoveAt(m_Locals.Count - 1);
			// Debug.WriteLine(string.Format("Pop : {0}", frame.MaxIndex - frame.StartIndex));

			return GetRuntimeFrameFromBuildFrame(frame, true);
		}

		public LRef Find(string name)
		{
			for (int i = m_Locals.Count - 1; i >= 0; i--)
			{
				int idx = m_Locals[i].Find(name);
				if (idx >= 0)
					return LRef.Local(name, idx);

				if (m_Locals[i].Breaking)
					break;
			}

			IClosureBuilder closure = m_ClosureBuilders.LastOrDefault();

			if (closure != null)
			{
				int closureLocalBlockIdx = (int)closure.UpvalueCreationTag;

				if (closureLocalBlockIdx >= 0)
				{
					for (int i = closureLocalBlockIdx; i >= 0; i--)
					{
						int idx = m_Locals[i].Find(name);
						if (idx >= 0)
							return closure.CreateUpvalue(this, LRef.Local(name, idx));

						if (m_Locals[i].Breaking)
							break;
					}
				}
			}

			return LRef.Global(name);
		}

		public LRef DefineLocal(string name)
		{
			return LRef.Local(name, m_Locals[m_Locals.Count - 1].Define(name));
		}

		public LRef TryDefineLocal(string name)
		{
			int idx = m_Locals[m_Locals.Count - 1].Find(name);

			if (idx >= 0)
				return LRef.Local(name, idx);

			var s = LRef.Local(name, m_Locals[m_Locals.Count - 1].Define(name));
			// Debug.WriteLine(string.Format("Define local : {0}", s));
			return s;
		}

	}
}
