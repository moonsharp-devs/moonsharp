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
	class IfStatement : Statement
	{
		private class IfBlock
		{
			public Expression Exp;
			public Statement Block;
			public RuntimeScopeBlock StackFrame;
		}

		List<IfBlock> m_Ifs = new List<IfBlock>();
		Statement m_Else = null;
		RuntimeScopeBlock m_ElseStackFrame;


		public IfStatement(LuaParser.Stat_ifblockContext context, ScriptLoadingContext lcontext)
			: base(context,lcontext)
		{
			int ecount = context.exp().Length;
			int bcount = context.block().Length;

			for(int i = 0; i < ecount; i++)
			{
				var exp = context.exp()[i];
				var blk = context.block()[i];

				lcontext.Scope.PushBlock();
				var ifblock = new IfBlock() { Exp = NodeFactory.CreateExpression(exp, lcontext), Block = NodeFactory.CreateStatement(blk, lcontext) };
				ifblock.StackFrame = lcontext.Scope.PopBlock();

				m_Ifs.Add(ifblock);
			}

			if (bcount > ecount)
			{
				lcontext.Scope.PushBlock();
				m_Else = NodeFactory.CreateStatement(context.block()[bcount - 1], lcontext);
				m_ElseStackFrame = lcontext.Scope.PopBlock();
			}
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			List<Instruction> endJumps = new List<Instruction>();

			Instruction lastIfJmp = null;

			foreach (var ifblock in m_Ifs)
			{
				if (lastIfJmp != null)
					lastIfJmp.NumVal = bc.GetJumpPointForNextInstruction();

				ifblock.Exp.Compile(bc);
				lastIfJmp = bc.Emit_Jump(OpCode.Jf, -1);
				bc.Emit_Enter(ifblock.StackFrame);
				ifblock.Block.Compile(bc);
				bc.Emit_Leave(ifblock.StackFrame);
				endJumps.Add(bc.Emit_Jump(OpCode.Jump, -1));
			}

			lastIfJmp.NumVal = bc.GetJumpPointForNextInstruction();

			if (m_Else != null)
			{
				bc.Emit_Enter(m_ElseStackFrame);
				m_Else.Compile(bc);
				bc.Emit_Leave(m_ElseStackFrame);
			}

			foreach(var endjmp in endJumps)
				endjmp.NumVal = bc.GetJumpPointForNextInstruction();
		}



	}
}
