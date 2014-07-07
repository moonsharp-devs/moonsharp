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


		public override void Compile(Execution.VM.ByteCode bc)
		{
			if (m_LValues.Length == 1 && m_RValues.Length == 1)
			{
				m_LValues[0].CompileAssignment(bc);
				m_RValues[0].Compile(bc);
				bc.Emit_Store();
			}
			else
			{
				foreach (var var in m_LValues)
					var.CompileAssignment(bc);

				foreach (var exp in m_RValues)
					exp.Compile(bc);

				bc.Emit_Assign(m_LValues.Length, m_RValues.Length);
			}
		}

	}
}
