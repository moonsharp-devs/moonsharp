using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ScopeBlockStatement : Statement
	{
		Statement m_Block;
		RuntimeScopeBlock m_StackFrame;
		SourceRef m_Do, m_End;

		public ScopeBlockStatement(LuaParser.Stat_doblockContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			lcontext.Scope.PushBlock();
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);

			m_Do = BuildSourceRef(lcontext, context.Start, context.DO());
			m_End = BuildSourceRef(lcontext, context.Stop, context.END());

			m_StackFrame = lcontext.Scope.PopBlock();
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			using(bc.EnterSource(m_Do))
				bc.Emit_Enter(m_StackFrame);

			m_Block.Compile(bc);

			using (bc.EnterSource(m_End))
				bc.Emit_Leave(m_StackFrame);
		}

	}
}
