using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class FunctionCallChainExpression : Expression
	{
		Expression m_StartingExpression;
		List<FunctionCall> m_CallChain;

		private FunctionCallChainExpression(IParseTree context, ScriptLoadingContext lcontext, 
			LuaParser.VarOrExpContext varOrExp, IEnumerable<LuaParser.NameAndArgsContext> nameAndArgs)
			: base(context, lcontext)
		{
			m_StartingExpression = NodeFactory.CreateExpression(varOrExp, lcontext);
			m_CallChain = nameAndArgs.Select(naa => new FunctionCall(naa, lcontext)).ToList();
		}

		public FunctionCallChainExpression(IParseTree context, ScriptLoadingContext lcontext,
			Expression startingExpression, IEnumerable<LuaParser.NameAndArgsContext> nameAndArgs)
			: base(context, lcontext)
		{
			m_StartingExpression = startingExpression;
			m_CallChain = nameAndArgs.Select(naa => new FunctionCall(naa, lcontext)).ToList();
		}


		public FunctionCallChainExpression(LuaParser.Stat_functioncallContext context, ScriptLoadingContext lcontext)
			: this(context, lcontext, context.varOrExp(), context.nameAndArgs())
		{ }

		public FunctionCallChainExpression(LuaParser.PrefixexpContext context, ScriptLoadingContext lcontext)
			: this(context, lcontext, context.varOrExp(), context.nameAndArgs())
		{ }


		public override void Compile(Execution.VM.Chunk bc)
		{
			m_StartingExpression.Compile(bc);

			foreach (FunctionCall fn in m_CallChain)
			{
				fn.Compile(bc);
			}
		}


	}
}
