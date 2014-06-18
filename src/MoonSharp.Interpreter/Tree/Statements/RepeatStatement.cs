using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public RepeatStatement(LuaParser.Stat_repeatuntilloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			lcontext.Scope.PushBlock();
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_Condition = NodeFactory.CreateExpression(context.exp(), lcontext);
			m_StackFrame = lcontext.Scope.PopBlock();
		}


		public override void Compile(ByteCode bc)
		{
			Loop L = new Loop()
			{
				Scope = m_StackFrame
			};

			bc.LoopTracker.Loops.Push(L);

			int start = bc.GetJumpPointForNextInstruction();

			bc.Enter(m_StackFrame);
			m_Block.Compile(bc);
			bc.Debug("..end");

			m_Condition.Compile(bc);
			bc.Leave(m_StackFrame);
			bc.Jump(OpCode.Jf, start);

			int exitpoint = bc.GetJumpPointForNextInstruction();

			foreach (Instruction i in L.BreakJumps)
				i.NumVal = exitpoint;
		}


	}
}
