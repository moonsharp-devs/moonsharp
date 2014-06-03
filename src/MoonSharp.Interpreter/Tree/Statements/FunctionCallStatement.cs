using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class FunctionCallStatement : Statement
	{
		FunctionCallChainExpression m_FunctionCallChain;

		public FunctionCallStatement(LuaParser.Stat_functioncallContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_FunctionCallChain = new FunctionCallChainExpression(context, lcontext);
		}


		public override void Compile(Chunk bc)
		{
			m_FunctionCallChain.Compile(bc);
			bc.Pop();
		}
	}
}
