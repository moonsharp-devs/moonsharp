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

		private static Script LoadFromICharStream(ICharStream charStream)
		{
			LuaLexer lexer;
			LuaParser parser;

			using (var _ = new CodeChrono("MoonSharpScript.LoadFromICharStream/AST"))
			{
				lexer = new LuaLexer(charStream);
				parser = new LuaParser(new CommonTokenStream(lexer));
			}
#if DEBUG
			AstDump astDump = new AstDump();
			astDump.DumpTree(parser.chunk(), @"c:\temp\treedump.txt");
			parser.Reset();
#endif
			using (var _ = new CodeChrono("MoonSharpScript.LoadFromICharStream/EXE"))
			{
				var lcontext = new ScriptLoadingContext() { Scope = new BuildTimeScope() };
				ChunkStatement stat = new ChunkStatement(parser.chunk(), lcontext);
				return new Script(stat, lcontext);
			}
		}


		public static Script LoadFromFile(string filename)
		{
			return LoadFromICharStream(new AntlrFileStream(filename));
		}

		public static Script LoadFromString(string text)
		{
			return LoadFromICharStream(new AntlrInputStream(text));
		}



		public static void WarmUp()
		{
			LoadFromString("return 1;");
		}
	}
}
