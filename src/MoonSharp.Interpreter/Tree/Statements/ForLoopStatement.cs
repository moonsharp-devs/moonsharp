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
		RuntimeScopeFrame m_StackFrame;
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
				m_Step = new LiteralExpression(context, lcontext, new RValue(1));

			lcontext.Scope.PushBlock();
			m_VarName = lcontext.Scope.DefineLocal(context.NAME().GetText());
			m_InnerBlock = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.Pop();
		}

		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			RValue startv = m_Start.Eval(scope).AsNumber();
			RValue stopv = m_End.Eval(scope).AsNumber();
			RValue stepv = m_Step.Eval(scope).AsNumber();

			RuntimeAssert(startv.Type == DataType.Number, "'for' initial value must be a number");
			RuntimeAssert(stopv.Type == DataType.Number, "'for' stop value must be a number");
			RuntimeAssert(stepv.Type == DataType.Number, "'for' step value must be a number");

			RValue v = new RValue(startv.Number);

			for (double d = startv.Number;
				(stepv.Number > 0) ? d <= stopv.Number : d >= stopv.Number;
				d += stepv.Number)
			{
				v.Assign(d);

				ExecutionFlow flow = ExecuteStatementInBlockScope(m_InnerBlock, scope, m_StackFrame, m_VarName, v);

				if (flow.Type == ExecutionFlowType.Break)
					return ExecutionFlow.None;
				else if (flow.Type == ExecutionFlowType.Return)
					return flow;
			}

			return ExecutionFlow.None;
		}

		public override void Compile(Chunk bc)
		{
			m_End.Compile(bc);
			bc.ToNum();
			m_Step.Compile(bc);
			bc.ToNum();
			m_Start.Compile(bc);
			bc.ToNum();

			int start = bc.GetJumpPointForNextInstruction();
			var jumpend = bc.Jump(OpCode.JFor, -1);
			bc.Enter(m_StackFrame);
			bc.NSymStor(m_VarName);
			m_InnerBlock.Compile(bc);
			bc.Debug("..end");
			bc.Leave(m_StackFrame);
			bc.Incr(1);
			bc.Jump(OpCode.Jump, start);
			jumpend.NumVal = bc.GetJumpPointForNextInstruction();
			bc.Pop(3);
		}

	}
}
