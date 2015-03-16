using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;

using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ReturnStatement: Statement
	{
		Expression m_Expression = null;
		SourceRef m_Ref;

		public ReturnStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			lcontext.Lexer.Next();

			Token cur = lcontext.Lexer.Current;

			if (cur.IsEndOfBlock() || cur.Type == TokenType.SemiColon)
			{
				m_Expression = null;
				m_Ref = cur.GetSourceRef();
			}
			else
			{
				m_Expression = new ExprListExpression(Expression.ExprList(lcontext), lcontext);
				m_Ref = cur.GetSourceRefUpTo(lcontext.Lexer.Current);
			}
		}



		public override void Compile(Execution.VM.ByteCode bc)
		{
			using (bc.EnterSource(m_Ref))
			{
				if (m_Expression != null)
				{
					m_Expression.Compile(bc);
					bc.Emit_Ret(1);
				}
				else
				{
					bc.Emit_Ret(0);
				}
			}
		}
	}
}
