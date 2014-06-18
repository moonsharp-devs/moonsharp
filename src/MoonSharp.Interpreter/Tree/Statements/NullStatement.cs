using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class NullStatement : Statement
	{
		public NullStatement(LuaParser.Stat_nulstatementContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{ }


		public override void Compile(Execution.VM.ByteCode bc)
		{
		}
	}
}
