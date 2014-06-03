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
		Expression m_Expression;

		public ReturnStatement(LuaParser.RetstatContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Expression = NodeFactory.CreateExpression(context.children.Single(t => t is LuaParser.ExplistContext), lcontext);
		}



		public override void Compile(Execution.VM.Chunk bc)
		{
			m_Expression.Compile(bc);
			bc.Exit();
			bc.Ret(1);
		}
	}
}
