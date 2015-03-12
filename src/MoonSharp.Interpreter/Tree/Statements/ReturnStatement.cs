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
	class ReturnStatement: Statement
	{
		Expression m_Expression = null;
		SourceRef m_Ref;

		public ReturnStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			lcontext.Lexer.Next();

			Token cur = lcontext.Lexer.Current();

			if (cur.IsEndOfBlock() || cur.Type == TokenType.SemiColon)
			{
				m_Expression = null;
			}
			else
			{
				m_Expression = new ExprListExpression(Expression.ExprList(lcontext), lcontext);
			}
		}


		public ReturnStatement(LuaParser.RetstatContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			LuaParser.ExplistContext expr = context.children.FirstOrDefault(t => t is LuaParser.ExplistContext) as LuaParser.ExplistContext;

			if (expr != null)
			{
				m_Expression = NodeFactory.CreateExpression(expr, lcontext);
				m_Ref = BuildSourceRef(context.Start, expr.Stop);
			}
			else
			{
				m_Ref = BuildSourceRef(context.Start, context.RETURN());
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
