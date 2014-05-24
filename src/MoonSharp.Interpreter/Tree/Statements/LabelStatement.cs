using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class LabelStatement : Statement
	{
		public string Label { get; private set; }

		public LabelStatement(LuaParser.Stat_labelContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			Label = context.label().NAME().GetText();
		}

		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			return ExecutionFlow.None;
		}
	}
}
