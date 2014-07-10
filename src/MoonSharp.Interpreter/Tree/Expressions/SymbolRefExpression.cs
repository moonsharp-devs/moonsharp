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

		public SymbolRefExpression(IParseTree context, ScriptLoadingContext lcontext, SymbolRef refr)
			: base(context, lcontext)
		{
			m_Ref = refr;
		}


		public SymbolRefExpression(LuaParser.VarargContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Ref = lcontext.Scope.TryDefineLocal("...");
		}


		public SymbolRefExpression(ITerminalNode terminalNode, ScriptLoadingContext lcontext)
			: base(terminalNode, lcontext)
		{
			string varName = terminalNode.GetText();
			m_Ref = lcontext.Scope.Find(varName);
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_Load(m_Ref);
		}


		public void CompileAssignment(Execution.VM.ByteCode bc, int stackofs, int tupleidx)
		{
			bc.Emit_Store(m_Ref, stackofs, tupleidx);
		}
	}
}
