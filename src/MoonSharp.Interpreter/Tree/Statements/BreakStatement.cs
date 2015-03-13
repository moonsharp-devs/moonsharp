using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;


namespace MoonSharp.Interpreter.Tree.Statements
{
	class BreakStatement : Statement
	{
		SourceRef m_Ref;

		public BreakStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			CheckTokenType(lcontext, TokenType.Break);
		}



		public override void Compile(ByteCode bc)
		{
			using (bc.EnterSource(m_Ref))
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
}
