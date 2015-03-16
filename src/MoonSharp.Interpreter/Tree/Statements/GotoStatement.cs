using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class GotoStatement : Statement
	{
		SourceRef m_Ref;
		public string Label { get; private set; }

		public GotoStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			m_Ref = CheckTokenType(lcontext, TokenType.Goto).GetSourceRef();
			Token name = CheckTokenType(lcontext, TokenType.Name);

			Label = name.Text;
		}

		public override void Compile(ByteCode bc)
		{
			
		}





		internal void ResolveLabel(LabelStatement labelStatement)
		{
			throw new NotImplementedException();
		}
	}
}
