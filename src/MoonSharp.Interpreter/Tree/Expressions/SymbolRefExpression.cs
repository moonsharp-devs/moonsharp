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
		LRef m_Ref;

		public SymbolRefExpression(ITerminalNode terminalNode, ScriptLoadingContext lcontext)
			: base(terminalNode, lcontext)
		{
			string varName = terminalNode.GetText();
			m_Ref = lcontext.Scope.Find(varName);
		}


		public override void Compile(Execution.VM.Chunk bc)
		{
			bc.Load(m_Ref);
		}

		public void CompileAssignment(Execution.VM.Chunk bc)
		{
			bc.Symbol(m_Ref);
		}
	}
}
