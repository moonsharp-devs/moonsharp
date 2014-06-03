using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class BreakStatement : Statement
	{
		public BreakStatement(LuaParser.Stat_breakContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
		}



		public override void Compile(Chunk bc)
		{
			bc.LoopTracker.Loops.Peek().CompileBreak(bc);
		}
	}
}
