using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class AssignmentStatement : Statement
	{
		IVariable[] m_LValues;
		Expression[] m_RValues;
		SourceRef m_Ref;

		public AssignmentStatement(LuaParser.Stat_assignmentContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_LValues = NodeFactory.CreateVariablesArray(context.varlist().var(), lcontext);
			m_RValues = NodeFactory.CreateExpessionArray(context.explist().exp(), lcontext);

			m_Ref = BuildSourceRef(context.Start, context.Stop);
		}

		public AssignmentStatement(LuaParser.Stat_localassignmentContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			var explist = context.explist();

			if (explist != null)
			{
				m_RValues = NodeFactory.CreateExpessionArray(explist.exp(), lcontext);
			}
			else
			{
				m_RValues = new Expression[0];
			}

			m_LValues = NodeFactory.CreateVariablesArray(context, context.namelist().NAME(), lcontext);

			m_Ref = BuildSourceRef(context.Start, context.Stop);
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			using (bc.EnterSource(m_Ref))
			{
				foreach (var exp in m_RValues)
				{
					exp.Compile(bc);
				}

				for (int i = 0; i < m_LValues.Length; i++)
					m_LValues[i].CompileAssignment(bc,
							Math.Max(m_RValues.Length - 1 - i, 0), // index of r-value
							i - Math.Min(i, m_RValues.Length - 1)); // index in last tuple

				bc.Emit_Pop(m_RValues.Length);
			}
		}

	}
}
