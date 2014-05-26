using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ExecutionFlowStatement : Statement
	{
		ExecutionFlow m_Flow;

		public ExecutionFlowStatement(LuaParser.Stat_breakContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Flow = ExecutionFlow.Break;
		}

		public ExecutionFlowStatement(LuaParser.Stat_gotoContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Flow = ExecutionFlow.GoTo(context.NAME().GetText());
			throw new NotImplementedException("GoTo not implemented yet!");
		}

		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			return m_Flow;
		}

		public override void Compile(Chunk bc)
		{
			bc.LoopTracker.Loops.Peek().CompileBreak(bc);
		}
	}
}
