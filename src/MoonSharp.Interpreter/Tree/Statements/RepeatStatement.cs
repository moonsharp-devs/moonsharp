using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class RepeatStatement : Statement
	{
		Expression m_Condition;
		Statement m_Block;
		RuntimeScopeBlock m_StackFrame;
		SourceRef m_Repeat, m_Until;

		public RepeatStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			CheckTokenType(lcontext, TokenType.Repeat);

			lcontext.Scope.PushBlock();
			m_Block = new CompositeStatement(lcontext);

			CheckTokenType(lcontext, TokenType.Until);

			m_Condition = Expression.Expr(lcontext);

			m_StackFrame = lcontext.Scope.PopBlock();

			//m_Repeat = BuildSourceRef(context.Start, context.REPEAT());
			//m_Until = BuildSourceRef(exp.Start, exp.Stop);
		}

		public RepeatStatement(LuaParser.Stat_repeatuntilloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			lcontext.Scope.PushBlock();
			var exp = context.exp();

			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_Condition = NodeFactory.CreateExpression(exp, lcontext);
			m_StackFrame = lcontext.Scope.PopBlock();

			m_Repeat = BuildSourceRef(context.Start, context.REPEAT());
			m_Until = BuildSourceRef(exp.Start, exp.Stop);
		}

		public override void Compile(ByteCode bc)
		{
			Loop L = new Loop()
			{
				Scope = m_StackFrame
			};

			bc.PushSourceRef(m_Repeat);

			bc.LoopTracker.Loops.Push(L);

			int start = bc.GetJumpPointForNextInstruction();

			bc.Emit_Enter(m_StackFrame);
			m_Block.Compile(bc);

			bc.PopSourceRef();
			bc.PushSourceRef(m_Until);
			bc.Emit_Debug("..end");

			m_Condition.Compile(bc);
			bc.Emit_Leave(m_StackFrame);
			bc.Emit_Jump(OpCode.Jf, start);

			bc.LoopTracker.Loops.Pop();

			int exitpoint = bc.GetJumpPointForNextInstruction();

			foreach (Instruction i in L.BreakJumps)
				i.NumVal = exitpoint;

			bc.PopSourceRef();
		}


	}
}
