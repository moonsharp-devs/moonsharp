using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Execution.Scopes
{
	internal class BuildTimeScopeBlock
	{
		internal BuildTimeScopeBlock Parent { get; private set; }
		internal List<BuildTimeScopeBlock> ChildNodes { get; private set; }

		internal RuntimeScopeBlock ScopeBlock { get; private set; }

		Dictionary<string, SymbolRef> m_DefinedNames = new Dictionary<string, SymbolRef>();

		MultiDictionary<string, GotoStatement> m_PendingGotos;
		Dictionary<string, LabelStatement> m_DefineLabels;

		internal BuildTimeScopeBlock(BuildTimeScopeBlock parent)
		{
			Parent = parent;
			ChildNodes = new List<BuildTimeScopeBlock>();
			ScopeBlock = new RuntimeScopeBlock();
		}


		internal BuildTimeScopeBlock AddChild()
		{
			BuildTimeScopeBlock block = new BuildTimeScopeBlock(this);
			ChildNodes.Add(block);
			return block;
		}

		internal SymbolRef Find(string name)
		{
			return m_DefinedNames.GetOrDefault(name);
		}

		internal SymbolRef Define(string name)
		{
			SymbolRef l = SymbolRef.Local(name, -1);
			m_DefinedNames.Add(name, l);
			return l;
		}

		internal int ResolveLRefs(BuildTimeScopeFrame buildTimeScopeFrame)
		{
			int firstVal = -1;
			int lastVal = -1;

			foreach (SymbolRef lref in m_DefinedNames.Values)
			{
				int pos = buildTimeScopeFrame.AllocVar(lref);

				if (firstVal < 0)
					firstVal = pos;

				lastVal = pos;
			}

			this.ScopeBlock.From = firstVal;
			this.ScopeBlock.ToInclusive = this.ScopeBlock.To = lastVal;

			if (firstVal < 0)
				this.ScopeBlock.From = buildTimeScopeFrame.GetPosForNextVar();

			foreach (var child in ChildNodes)
			{
				this.ScopeBlock.ToInclusive = Math.Max(this.ScopeBlock.ToInclusive, child.ResolveLRefs(buildTimeScopeFrame));
			}

			return lastVal;
		}


		public void DefineLabel(LabelStatement label)
		{
			if (m_DefineLabels.ContainsKey(label.Label))
			{
				throw new SyntaxErrorException(null, "label 'label' already defined on line 3");
			}
			else
			{
				m_DefineLabels.Add(label.Label, label);

				foreach (GotoStatement gotostat in m_PendingGotos.Find(label.Label))
				{
					gotostat.ResolveLabel(label);
				}

				m_PendingGotos.Remove(label.Label);
			}
		}

		public void ResolveGotoOrPending(GotoStatement gotostat)
		{
			if (m_DefineLabels.ContainsKey(gotostat.Label))
			{
				gotostat.ResolveLabel(m_DefineLabels[gotostat.Label]);
			}
			else
			{
				m_PendingGotos.Add(gotostat.Label, gotostat);
			}
		}







	}
}
