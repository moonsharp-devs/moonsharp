using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class ANTLR_FunctionCallChainExpression : Expression
	{
		Expression m_StartingExpression;
		List<ANTLR_FunctionCall> m_CallChain;

		private ANTLR_FunctionCallChainExpression(IParseTree context, ScriptLoadingContext lcontext, 
			LuaParser.VarOrExpContext varOrExp, IEnumerable<LuaParser.NameAndArgsContext> nameAndArgs)
			: base(context, lcontext)
		{
			m_StartingExpression = NodeFactory.CreateExpression(varOrExp, lcontext);
			m_CallChain = nameAndArgs.Select(naa => new ANTLR_FunctionCall(naa, lcontext)).ToList();
		}

		public ANTLR_FunctionCallChainExpression(IParseTree context, ScriptLoadingContext lcontext,
			Expression startingExpression, IEnumerable<LuaParser.NameAndArgsContext> nameAndArgs)
			: base(context, lcontext)
		{
			m_StartingExpression = startingExpression;
			m_CallChain = nameAndArgs.Select(naa => new ANTLR_FunctionCall(naa, lcontext)).ToList();
		}


		public ANTLR_FunctionCallChainExpression(LuaParser.Stat_functioncallContext context, ScriptLoadingContext lcontext)
			: this(context, lcontext, context.varOrExp(), context.nameAndArgs())
		{ }

		public ANTLR_FunctionCallChainExpression(LuaParser.PrefixexpContext context, ScriptLoadingContext lcontext)
			: this(context, lcontext, context.varOrExp(), context.nameAndArgs())
		{ }


		public override void Compile(Execution.VM.ByteCode bc)
		{
			m_StartingExpression.Compile(bc);

			foreach (ANTLR_FunctionCall fn in m_CallChain)
			{
				fn.Compile(bc);
			}
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			throw new DynamicExpressionException("Dynamic Expressions cannot call functions.");
		}
	}
}
