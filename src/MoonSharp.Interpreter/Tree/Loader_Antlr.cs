//#define DEBUG_COMPILER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Tree
{
	internal static class Loader_Antlr
	{
		internal static int LoadChunk(Script script, string code, ByteCode bytecode, string sourceName, int sourceIdx, Table globalContext)
		{
			try
			{
				LuaParser parser = CreateParser(script, new AntlrInputStream(code), sourceIdx, p => p.chunk());

				ScriptLoadingContext lcontext = CreateLoadingContext(sourceName, sourceIdx);
				ChunkStatement stat;

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.AstCreation))
					stat = new ChunkStatement(parser.chunk(), lcontext, globalContext);

				int beginIp = -1;

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.Compilation))
				{
					bytecode.Emit_Nop(string.Format("Begin chunk {0}", sourceName));
					beginIp = bytecode.GetJumpPointForLastInstruction();
					stat.Compile(bytecode);
					bytecode.Emit_Nop(string.Format("End chunk {0}", sourceName));
				}

				Debug_DumpByteCode(bytecode, sourceIdx);

				return beginIp;
			}
			catch (ParseCanceledException ex)
			{
				HandleParserError(ex);
				throw;
			}
		}

		internal static int LoadFunction(Script script, string code, ByteCode bytecode, string sourceName, int sourceIdx, Table globalContext)
		{
			try
			{
				LuaParser parser = CreateParser(script, new AntlrInputStream(code), sourceIdx, p => p.anonfunctiondef());

				ScriptLoadingContext lcontext = CreateLoadingContext(sourceName, sourceIdx);
				FunctionDefinitionExpression fndef;

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.AstCreation))
					fndef = new FunctionDefinitionExpression(parser.anonfunctiondef(), lcontext, false, globalContext);

				int beginIp = -1;

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.Compilation))
				{
					bytecode.Emit_Nop(string.Format("Begin function {0}", sourceName));
					beginIp = fndef.CompileBody(bytecode, sourceName);
					bytecode.Emit_Nop(string.Format("End function {0}", sourceName));

					Debug_DumpByteCode(bytecode, sourceIdx);
				}

				return beginIp;
			}
			catch (ParseCanceledException ex)
			{
				HandleParserError(ex);
				throw;
			}
		}

		private static void HandleParserError(ParseCanceledException ex)
		{
			throw new SyntaxErrorException("{0}", ex.Message);
		}


		[Conditional("DEBUG_COMPILER")]
		private static void Debug_DumpByteCode(ByteCode bytecode, int sourceIdx)
		{
			//bytecode.Dump(string.Format(@"c:\temp\codedump_{0}.txt", sourceIdx));
		}

		//[Conditional("DEBUG_COMPILER")]
		private static void Debug_DumpAst(LuaParser parser, int sourceIdx, Func<LuaParser, IParseTree> dumper)
		{
			try
			{
				AstDump astDump = new AstDump();
				// astDump.DumpTree(dumper(parser), string.Format(@"c:\temp\treedump_{0:000}.txt", sourceIdx));
				astDump.WalkTreeForWaste(dumper(parser));
			}
			catch { }
			parser.Reset();
		}


		private static ScriptLoadingContext CreateLoadingContext(string sourceName, int sourceIdx)
		{
			return new ScriptLoadingContext()
			{
				Scope = new BuildTimeScope(),
				SourceIdx = sourceIdx,
				SourceName = sourceName,
			};
		}

		private static LuaParser CreateParser(Script script, ICharStream charStream, int sourceIdx, Func<LuaParser, IParseTree> dumper)
		{
			LuaLexer lexer;
			LuaParser parser;

			using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.Parsing))
			{
				lexer = new LuaLexer(charStream);
				parser = new LuaParser(new CommonTokenStream(lexer))
				{
					ErrorHandler = new BailErrorStrategy(),
				};

				parser.Interpreter.PredictionMode = PredictionMode.Sll;
				Debug_DumpAst(parser, sourceIdx, dumper);
			}


			return parser;
		}

	}
}
