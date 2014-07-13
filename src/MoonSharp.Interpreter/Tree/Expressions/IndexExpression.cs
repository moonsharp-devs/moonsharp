using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class IndexExpression : Expression, IVariable
	{
		Expression m_BaseExp;
		Expression m_IndexExp;

		public IndexExpression(IParseTree node, ScriptLoadingContext lcontext, Expression baseExp, Expression indexExp)
			:base(node, lcontext)
		{
			m_BaseExp = baseExp;
			m_IndexExp = indexExp;
		}



		public override void Compile(ByteCode bc)
		{
			m_BaseExp.Compile(bc);

			if (m_IndexExp is LiteralExpression)
			{
				LiteralExpression lit = (LiteralExpression)m_IndexExp;
				bc.Emit_Index(lit.Value);
			}
			else
			{
				m_IndexExp.Compile(bc);
				bc.Emit_Index();
			}
		}

		public void CompileAssignment(ByteCode bc, int stackofs, int tupleidx)
		{
			m_BaseExp.Compile(bc);

			if (m_IndexExp is LiteralExpression)
			{
				LiteralExpression lit = (LiteralExpression)m_IndexExp;
				bc.Emit_IndexSet(stackofs, tupleidx, lit.Value);
			}
			else
			{
				m_IndexExp.Compile(bc);
				bc.Emit_IndexSet(stackofs, tupleidx);
			}


		}
	}
}
