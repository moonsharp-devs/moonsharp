using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Tree;

namespace MoonSharp.Interpreter.Execution
{
	class ScriptLoadingContext
	{
		public Script Script { get; private set; }
		public BuildTimeScope Scope { get; set; }
		public SourceCode Source { get; set; }
		public bool Anonymous { get; set; }
		public bool IsDynamicExpression { get; set; }
		public Lexer Lexer { get; set; }

		public ScriptLoadingContext(Script s)
		{
			Script = s;
		}

		public void EnterLevel()
		{
			//if (++ls.L.nCcalls > LUAI_MAXCCALLS)
			//	LuaXLexError(ls, "chunk has too many syntax levels", 0);
		}

		public void LeaveLevel()
		{
			//ls.L.nCcalls--;
		}


	}
}
