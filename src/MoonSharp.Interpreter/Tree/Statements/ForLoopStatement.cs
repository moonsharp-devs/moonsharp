using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ForLoopStatement : Statement
	{
		//for' NAME '=' exp ',' exp (',' exp)? 'do' block 'end'
		RuntimeScopeBlock m_StackFrame;
		Statement m_InnerBlock;
		SymbolRef m_VarName;
		Expression m_Start, m_End, m_Step;

		public ForLoopStatement(LuaParser.Stat_forloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			var exps = context.exp();

			m_Start = NodeFactory.CreateExpression(exps[0], lcontext);
			m_End = NodeFactory.CreateExpression(exps[1], lcontext);

			if (exps.Length > 2)
				m_Step = NodeFactory.CreateExpression(exps[2], lcontext);
			else
				m_Step = new LiteralExpression(context, lcontext, DynValue.NewNumber(1));

			lcontext.Scope.PushBlock();
			m_VarName = lcontext.Scope.DefineLocal(context.NAME().GetText());
			m_InnerBlock = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.PopBlock();
		}

		public override void Compile(ByteCode bc)
		{
			Loop L = new Loop()
			{
				Scope = m_StackFrame
			};

			bc.LoopTracker.Loops.Push(L);

			m_End.Compile(bc);
			bc.Emit_ToNum();
			m_Step.Compile(bc);
			bc.Emit_ToNum();
			m_Start.Compile(bc);
			bc.Emit_ToNum();

			int start = bc.GetJumpPointForNextInstruction();
			var jumpend = bc.Emit_Jump(OpCode.JFor, -1);
			bc.Emit_Enter(m_StackFrame);
			//bc.Emit_SymStorN(m_VarName);

			bc.Emit_Store(m_VarName, 0, 0);

			m_InnerBlock.Compile(bc);
			bc.Emit_Debug("..end");
			bc.Emit_Leave(m_StackFrame);
			bc.Emit_Incr(1);
			bc.Emit_Jump(OpCode.Jump, start);

			int exitpoint = bc.GetJumpPointForNextInstruction();

			foreach (Instruction i in L.BreakJumps)
				i.NumVal = exitpoint;

			jumpend.NumVal = exitpoint;
			bc.Emit_Pop(3);
		}

	}
}
