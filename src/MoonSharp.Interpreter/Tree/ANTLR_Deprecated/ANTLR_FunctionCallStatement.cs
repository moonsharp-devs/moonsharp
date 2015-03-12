using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ANTLR_FunctionCallStatement : Statement
	{
		ANTLR_FunctionCallChainExpression m_FunctionCallChain;
		SourceRef m_SourceRef;

		public ANTLR_FunctionCallStatement(LuaParser.Stat_functioncallContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_FunctionCallChain = new ANTLR_FunctionCallChainExpression(context, lcontext);
			m_SourceRef = BuildSourceRef(context.Start, context.Stop);
		}


		public override void Compile(ByteCode bc)
		{
			using (bc.EnterSource(m_SourceRef))
			{
				m_FunctionCallChain.Compile(bc);
				bc.Emit_Pop();
			}
		}
	}
}
