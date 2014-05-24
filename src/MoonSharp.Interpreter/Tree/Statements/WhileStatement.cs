using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class WhileStatement : Statement
	{
		Expression m_Condition;
		Statement m_Block;
		RuntimeScopeFrame m_StackFrame;

		public WhileStatement(LuaParser.Stat_whiledoloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Condition = NodeFactory.CreateExpression(context.exp(), lcontext);

			lcontext.Scope.PushBlock();
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.Pop();
		}

		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			while (m_Condition.Eval(scope).TestAsBoolean())
			{
				ExecutionFlow flow = ExecuteStatementInBlockScope(m_Block, scope, m_StackFrame);

				if (flow.Type == ExecutionFlowType.Break)
					return ExecutionFlow.None;
				else if (flow.Type == ExecutionFlowType.Return)
					return flow;
			}

			return ExecutionFlow.None;
		}

	}
}
