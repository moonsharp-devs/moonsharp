using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ReturnStatement: Statement
	{
		Expression m_Expression = null;

		public ReturnStatement(LuaParser.RetstatContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			LuaParser.ExplistContext expr = context.children.FirstOrDefault(t => t is LuaParser.ExplistContext) as LuaParser.ExplistContext;

			if (expr != null)
				m_Expression = NodeFactory.CreateExpression(expr, lcontext);
		}



		public override void Compile(Execution.VM.ByteCode bc)
		{
			if (m_Expression != null)
			{
				m_Expression.Compile(bc);
				bc.Ret(1);
			}
			else
			{
				bc.Ret(0);
			}
		}
	}
}
