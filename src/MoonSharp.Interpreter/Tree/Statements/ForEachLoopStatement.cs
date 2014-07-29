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
	class ForEachLoopStatement : Statement
	{
		RuntimeScopeBlock m_StackFrame;
		SymbolRef[] m_Names;
		IVariable[] m_NameExps;
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

			m_NameExps = m_Names
				.Select(s => new SymbolRefExpression(context, lcontext, s))
				.Cast<IVariable>()
				.ToArray();

			
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);

			m_StackFrame = lcontext.Scope.PopBlock();
		}

		public override void Compile(ByteCode bc)
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
			bc.Emit_IterPrep();

			// loop start - stack : iterator-tuple
			int start = bc.GetJumpPointForNextInstruction();
			bc.Emit_Enter(m_StackFrame);

			// expand the tuple - stack : iterator-tuple, f, var, s
			bc.Emit_ExpTuple(0);  

			// calls f(s, var) - stack : iterator-tuple, iteration result
			bc.Emit_Call(2);

			// perform assignment of iteration result- stack : iterator-tuple, iteration result
			for (int i = 0; i < m_NameExps.Length; i++)
				m_NameExps[i].CompileAssignment(bc, 0, i);

			// pops  - stack : iterator-tuple
			bc.Emit_Pop();

			// repushes the main iterator var - stack : iterator-tuple, main-iterator-var
			bc.Emit_Load(m_Names[0]);

			// updates the iterator tuple - stack : iterator-tuple, main-iterator-var
			bc.Emit_IterUpd();

			// checks head, jumps if nil - stack : iterator-tuple, main-iterator-var
			var endjump = bc.Emit_Jump(OpCode.JNil, -1);

			// executes the stuff - stack : iterator-tuple
			m_Block.Compile(bc);

			// loop back again - stack : iterator-tuple
			bc.Emit_Leave(m_StackFrame);
			bc.Emit_Jump(OpCode.Jump, start);

			bc.LoopTracker.Loops.Pop();

			int exitpointLoopExit = bc.GetJumpPointForNextInstruction();
			bc.Emit_Leave(m_StackFrame);

			int exitpointBreaks = bc.GetJumpPointForNextInstruction();

			bc.Emit_Pop();

			foreach (Instruction i in L.BreakJumps)
				i.NumVal = exitpointBreaks;

			endjump.NumVal = exitpointLoopExit;
		}


	}
}
