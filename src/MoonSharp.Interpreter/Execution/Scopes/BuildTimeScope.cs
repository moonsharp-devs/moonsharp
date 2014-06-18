using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.Scopes;

namespace MoonSharp.Interpreter.Execution
{
	public class BuildTimeScope
	{
		List<BuildTimeScopeFrame> m_Frames = new List<BuildTimeScopeFrame>();
		List<IClosureBuilder> m_ClosureBuilders = new List<IClosureBuilder>();

		public BuildTimeScope()
		{
			//PushFunction();
		}

		public void PushFunction()
		{
			m_Frames.Add(new BuildTimeScopeFrame());
		}

		public void PushBlock()
		{
			m_Frames.Last().PushBlock();
		}

		public RuntimeScopeBlock PopBlock()
		{
			return m_Frames.Last().PopBlock();
		}

		public RuntimeScopeFrame PopFunction()
		{
			var last = m_Frames.Last();
			last.ResolveLRefs();
			m_Frames.RemoveAt(m_Frames.Count - 1);
			
			return last.GetRuntimeFrameData();
		}


		public SymbolRef Find(string name)
		{
			SymbolRef local = m_Frames.Last().Find(name);

			if (local != null)
				return local;

			IClosureBuilder closure = m_ClosureBuilders.LastOrDefault();

			if (closure != null)
			{
				int closureLocalBlockIdx = (int)closure.UpvalueCreationTag;

				if (closureLocalBlockIdx >= 0)
				{
					for (int i = closureLocalBlockIdx; i >= 0; i--)
					{
						SymbolRef symb = m_Frames[i].Find(name);
						if (symb != null)
							return closure.CreateUpvalue(this, symb);
					}
				}
			}

			return SymbolRef.Global(name);
		}

		public SymbolRef DefineLocal(string name)
		{
			return m_Frames.Last().DefineLocal(name);
		}

		public SymbolRef TryDefineLocal(string name)
		{
			return m_Frames.Last().TryDefineLocal(name);
		}


		public void EnterClosure(IClosureBuilder closureBuilder)
		{
			m_ClosureBuilders.Add(closureBuilder);
			closureBuilder.UpvalueCreationTag = (m_Frames.Count - 1);
		}

		public void LeaveClosure()
		{
			m_ClosureBuilders.RemoveAt(m_ClosureBuilders.Count - 1);
		}


	}
}
