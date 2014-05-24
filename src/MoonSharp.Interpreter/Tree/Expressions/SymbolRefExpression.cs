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

		public SymbolRefExpression(ITerminalNode terminalNode, ScriptLoadingContext lcontext)
			: base(terminalNode, lcontext)
		{
			string varName = terminalNode.GetText();
			m_Ref = lcontext.Scope.Find(varName);

			if (!m_Ref.IsValid())
			{
				m_Ref = lcontext.Scope.DefineGlobal(varName);
			}
		}

		public override RValue Eval(RuntimeScope scope)
		{
			RValue v = scope.Get(m_Ref);

			if (v == null)
				throw new ScriptRuntimeException(this.TreeNode, "Undefined symbol: {0}", m_Ref.Name);

			return v;
		}

		public void SetValue(RuntimeScope scope, RValue rValue)
		{
			scope.Assign(m_Ref, rValue);
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
