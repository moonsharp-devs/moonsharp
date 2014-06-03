using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ForEachLoopStatement : Statement
	{
		RuntimeScopeFrame m_StackFrame;
		LRef[] m_Names;
		Expression m_RValues;
		Statement m_Block;


		public ForEachLoopStatement(LuaParser.Stat_foreachloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			context.explist();

			var explist = context.explist();

			m_RValues = NodeFactory.CreateExpression(explist, lcontext);

			lcontext.Scope.PushBlock();

			m_Names = context.namelist().NAME()
				.Select(n => n.GetText())
				.Select(n => lcontext.Scope.DefineLocal(n))
				.ToArray();
			
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);

			m_StackFrame = lcontext.Scope.Pop();
		}

		public override void Compile(Chunk bc)
		{
			//for var_1, ···, var_n in explist do block end

			Loop L = new Loop()
			{
				Scope = m_StackFrame
			};
			bc.LoopTracker.Loops.Push(L);

			// get iterator tuple
			m_RValues.Compile(bc);

			// prepares iterator tuple - stack : iterator-tuple
			bc.IterPrep();

			// loop start - stack : iterator-tuple
			int start = bc.GetJumpPointForNextInstruction();
			bc.Enter(m_StackFrame);

			// push all iterating variables - stack : iterator-tuple, iter-var-symbols
			foreach (LRef s in m_Names)
				bc.Symbol(s);

			// expand the tuple - stack : iterator-tuple, iter-var-symbols, f, var, s
			bc.ExpTuple(m_Names.Length);  
			bc.Reverse(2);

			// calls f(s, var) - stack : iterator-tuple, iter-var-symbols, iteration result
			bc.Call(2);

			// assigns to iter-var-symbols - stack : iterator-tuple
			bc.Assign(m_Names.Length, 1);

			// repushes the main iterator var - stack : iterator-tuple, main-iterator-var
			bc.Load(m_Names[0]);

			// updates the iterator tuple - stack : iterator-tuple, main-iterator-var
			bc.IterUpd();

			// checks head, jumps if nil - stack : iterator-tuple, main-iterator-var
			var endjump = bc.Jump(OpCode.JNil, -1);

			// executes the stuff - stack : iterator-tuple
			m_Block.Compile(bc);

			// loop back again - stack : iterator-tuple
			bc.Leave(m_StackFrame);
			bc.Jump(OpCode.Jump, start);

			int exitpointLoopExit = bc.GetJumpPointForNextInstruction();
			bc.Leave(m_StackFrame);

			int exitpointBreaks = bc.GetJumpPointForNextInstruction();

			bc.Pop();

			foreach (Instruction i in L.BreakJumps)
				i.NumVal = exitpointBreaks;

			endjump.NumVal = exitpointLoopExit;
		}


	}
}
