using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class AssignmentStatement : Statement
	{
		IVariable[] m_LValues;
		Expression[] m_RValues;

		public AssignmentStatement(LuaParser.Stat_assignmentContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_LValues = context.varlist().var()
				.Select(v => NodeFactory.CreateVariableExpression(v, lcontext))
				.Cast<IVariable>()
				.ToArray();

			m_RValues = context.explist()
				.exp()
				.Select(e => NodeFactory.CreateExpression(e, lcontext))
				.ToArray();
		}

		public AssignmentStatement(LuaParser.Stat_localassignmentContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			var explist = context.explist();

			if (explist != null)
			{
				m_RValues = explist
				.exp()
				.Select(e => NodeFactory.CreateExpression(e, lcontext))
				.ToArray();
			}
			else
				m_RValues = new Expression[0];

			m_LValues = context.namelist().NAME()
				.Select(n => n.GetText())
				.Select(n => lcontext.Scope.TryDefineLocal(n))
				.Select(s => new SymbolRefExpression(context, lcontext, s))
				.Cast<IVariable>()
				.ToArray();
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			foreach (var exp in m_RValues)
			{
				exp.Compile(bc);

				if (exp is SymbolRefExpression)
				{
					bc.Emit_Clone();
				}
			}

			for(int i = 0; i < m_LValues.Length; i++)
				m_LValues[i].CompileAssignment(bc,
						Math.Max(m_RValues.Length - 1 - i, 0), // index of r-value
						i - Math.Min(i, m_RValues.Length - 1)); // index in last tuple

			bc.Emit_Pop(m_RValues.Length);
		}

	}
}
