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



		public override void Compile(ByteCode bc)
		{
			if (bc.LoopTracker.Loops.Count == 0)
				throw new SyntaxErrorException("<break> not inside a loop");

			ILoop loop = bc.LoopTracker.Loops.Peek();
	
			if (loop.IsBoundary())
				throw new SyntaxErrorException("<break> not inside a loop");

			loop.CompileBreak(bc);
		}
	}
}
