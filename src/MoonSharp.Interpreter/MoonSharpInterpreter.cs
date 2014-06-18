using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter
{
	public static class MoonSharpInterpreter
	{
		public enum MoonSharpExecutionProfile
		{
			MoonSharp,
			LuaCompatibility,
			LuaStrict
		}


		public static void WarmUp()
		{
			//LoadFromString("return 1;");
		}
	}
}
