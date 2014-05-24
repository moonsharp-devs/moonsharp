using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class RepeatStatement : Statement
	{
		Expression m_Condition;
		Statement m_Block;
		RuntimeScopeFrame m_StackFrame;

		public RepeatStatement(LuaParser.Stat_repeatuntilloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			lcontext.Scope.PushBlock();
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_Condition = NodeFactory.CreateExpression(context.exp(), lcontext);
			m_StackFrame = lcontext.Scope.Pop();
		}

		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			bool condition = true;

			while (condition)
			{
				scope.PushFrame(m_StackFrame);

				ExecutionFlow flow = m_Block.Exec(scope);

				if (flow.Type == ExecutionFlowType.Break)
				{
					scope.PopFrame(m_StackFrame);
					return ExecutionFlow.None;
				}
				else if (flow.Type == ExecutionFlowType.Return)
				{
					scope.PopFrame(m_StackFrame);
					return flow;
				}

				condition = !m_Condition.Eval(scope).TestAsBoolean();

				scope.PopFrame(m_StackFrame);
			}

			return ExecutionFlow.None;
		}

	}
}
