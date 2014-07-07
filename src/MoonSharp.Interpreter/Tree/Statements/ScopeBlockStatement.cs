using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ScopeBlockStatement : Statement
	{
		Statement m_Block;
		RuntimeScopeBlock m_StackFrame;

		public ScopeBlockStatement(LuaParser.Stat_doblockContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			lcontext.Scope.PushBlock();
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.PopBlock();
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_Enter(m_StackFrame);
			m_Block.Compile(bc);
			bc.Emit_Leave(m_StackFrame);
		}

	}
}
