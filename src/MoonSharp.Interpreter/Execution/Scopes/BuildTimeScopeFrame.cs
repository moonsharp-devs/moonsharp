using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Execution
{
	class BuildTimeScopeFrame
	{
		Dictionary<string, int> m_IndexList = new Dictionary<string, int>();
		Dictionary<int, string> m_RevIndexList = new Dictionary<int, string>();

		public int BaseIndex { get; private set; }
		public int StartIndex { get; private set; }

		public int MaxIndex { get; private set; }

		public bool Breaking { get; private set; }

		public BuildTimeScopeFrame(int baseIndex, int startIndex, bool breaking)
		{
			BaseIndex = baseIndex;
			StartIndex = MaxIndex = startIndex;
			Breaking = breaking;
		}

		public int Find(string name)
		{
			if (m_IndexList.ContainsKey(name))
				return m_IndexList[name];

			return -1;
		}

		public string FindRev(int idx)
		{
			return m_RevIndexList[idx];
		}

		public int Define(string name)
		{
			if (!m_IndexList.ContainsKey(name))
			{
				m_IndexList.Add(name, MaxIndex - BaseIndex);
				m_RevIndexList.Add(MaxIndex - BaseIndex, name);
				return MaxIndex++;
			}
			else
			{
				return m_IndexList[name];
			}
		}

	}
}
