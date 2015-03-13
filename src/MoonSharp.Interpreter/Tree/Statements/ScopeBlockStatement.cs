using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ScopeBlockStatement : Statement
	{
		Statement m_Block;
		RuntimeScopeBlock m_StackFrame;
		SourceRef m_Do, m_End;

		public ScopeBlockStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			lcontext.Scope.PushBlock();

			CheckTokenType(lcontext, TokenType.Do);

			m_Block = new CompositeStatement(lcontext);

			CheckTokenType(lcontext, TokenType.End);

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
