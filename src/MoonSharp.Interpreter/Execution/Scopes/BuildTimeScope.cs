using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class BuildTimeScope
	{
		BuildTimeScopeFrame m_GlobalRuntimeScope = new BuildTimeScopeFrame(0, 0, true);

		Dictionary<SymbolRef, RValue> m_PredefinedGlobals = new Dictionary<SymbolRef, RValue>();

		List<BuildTimeScopeFrame> m_Locals = new List<BuildTimeScopeFrame>();

		List<IClosureBuilder> m_ClosureBuilders = new List<IClosureBuilder>();


		public BuildTimeScope(Table t)
		{
			PushFunction();

			foreach (var kvp in t.Pairs().Where(e => e.Key.Type == DataType.String))
			{
				int idx = m_GlobalRuntimeScope.Define(kvp.Key.String);
				m_PredefinedGlobals.Add(SymbolRef.Global(kvp.Key.String, idx), kvp.Value);
			}

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
			List<SymbolRef> symbols = new List<SymbolRef>();
			for (int i = frame.StartIndex; i < frame.MaxIndex; i++)
			{
				SymbolRef s;
				if (local)
					s = SymbolRef.Local(frame.FindRev(i - frame.BaseIndex), i - frame.BaseIndex);
				else
					s = SymbolRef.Global(frame.FindRev(i - frame.BaseIndex), i - frame.BaseIndex);

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

		public SymbolRef Find(string name)
		{
			for (int i = m_Locals.Count - 1; i >= 0; i--)
			{
				int idx = m_Locals[i].Find(name);
				if (idx >= 0)
					return SymbolRef.Local(name, idx);

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
							return closure.CreateUpvalue(this, SymbolRef.Local(name, idx));

						if (m_Locals[i].Breaking)
							break;
					}
				}
			}

			int idxglob = m_GlobalRuntimeScope.Find(name);
			if (idxglob >= 0)
				return SymbolRef.Global(name, idxglob);

			// Debug.WriteLine(string.Format("Attempted to find '{0}' failed", name));
			return SymbolRef.Invalid();
		}

		public SymbolRef DefineLocal(string name)
		{
			var s = SymbolRef.Local(name, m_Locals[m_Locals.Count - 1].Define(name));
			// Debug.WriteLine(string.Format("Define local  : {0}", s));
			return s;
		}

		public SymbolRef TryDefineLocal(string name)
		{
			int idx = m_Locals[m_Locals.Count - 1].Find(name);

			if (idx >= 0)
				return SymbolRef.Local(name, idx);

			var s = SymbolRef.Local(name, m_Locals[m_Locals.Count - 1].Define(name));
			// Debug.WriteLine(string.Format("Define local : {0}", s));
			return s;
		}


		public SymbolRef DefineGlobal(string name)
		{
			int idxglob = m_GlobalRuntimeScope.Find(name);
			if (idxglob >= 0)
				return SymbolRef.Global(name, idxglob);

			var s = SymbolRef.Global(name, m_GlobalRuntimeScope.Define(name));
			// Debug.WriteLine(string.Format("Define global : {0}", s));
			return s;
		}

		internal RuntimeScope SpawnRuntimeScope()
		{
			RuntimeScope scope = new RuntimeScope();

			scope.ExpandGlobal(m_GlobalRuntimeScope.MaxIndex);

			foreach (var kvp in m_PredefinedGlobals)
				scope.Assign(kvp.Key, kvp.Value);

			scope.PushFrame(GetRuntimeFrameFromBuildFrame(m_Locals[0], true));

			return scope;
		}

	}
}
