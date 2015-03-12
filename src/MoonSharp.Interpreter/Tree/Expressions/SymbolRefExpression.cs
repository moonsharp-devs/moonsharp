using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class SymbolRefExpression : Expression, IVariable
	{
		SymbolRef m_Ref;
		string m_VarName;

		public SymbolRefExpression(Token T, ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			m_VarName = T.Text;

			if (T.Type == TokenType.VarArgs)
			{
				m_Ref = lcontext.Scope.TryDefineLocal(WellKnownSymbols.VARARGS);

				if (!lcontext.Scope.CurrentFunctionHasVarArgs())
					throw new SyntaxErrorException("error:0: cannot use '...' outside a vararg function");

				if (lcontext.IsDynamicExpression)
					throw new DynamicExpressionException("Cannot use '...' in a dynamic expression.");
			}
			else
			{
				if (!lcontext.IsDynamicExpression)
					m_Ref = lcontext.Scope.Find(m_VarName);
			}
		}


		public SymbolRefExpression(IParseTree context, ScriptLoadingContext lcontext, SymbolRef refr)
			: base(context, lcontext)
		{
			m_Ref = refr;

			if (lcontext.IsDynamicExpression)
			{
				throw new DynamicExpressionException("Unsupported symbol reference expression detected.");
			}
		}


		public SymbolRefExpression(LuaParser.VarargContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Ref = lcontext.Scope.TryDefineLocal(WellKnownSymbols.VARARGS);

			if (!lcontext.Scope.CurrentFunctionHasVarArgs())
			{
				throw new SyntaxErrorException("error:0: cannot use '...' outside a vararg function");
			}

			if (lcontext.IsDynamicExpression)
			{
				throw new DynamicExpressionException("Cannot use '...' in a dynamic expression.");
			}
		}


		public SymbolRefExpression(ITerminalNode terminalNode, ScriptLoadingContext lcontext)
			: base(terminalNode, lcontext)
		{
			m_VarName = terminalNode.GetText();

			if (!lcontext.IsDynamicExpression)
			{
				m_Ref = lcontext.Scope.Find(m_VarName);
			}
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_Load(m_Ref);
		}


		public void CompileAssignment(Execution.VM.ByteCode bc, int stackofs, int tupleidx)
		{
			bc.Emit_Store(m_Ref, stackofs, tupleidx);
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			return context.EvaluateSymbolByName(m_VarName);
		}

		public override SymbolRef FindDynamic(ScriptExecutionContext context)
		{
			return context.FindSymbolByName(m_VarName);
		}
	}
}
