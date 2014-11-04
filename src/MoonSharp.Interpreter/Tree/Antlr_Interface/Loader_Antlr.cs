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
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Tree
{
	/// <summary>
	/// Class managing most of interactions with ANTLR.
	/// </summary>
	internal static class Loader_Antlr
	{

		internal static int LoadChunk(Script script, SourceCode source, ByteCode bytecode, Table globalContext)
		{
			AntlrErrorListener listener = new AntlrErrorListener(source);

			try
			{
				LuaParser parser = CreateParser(script, new AntlrInputStream(source.Code), source.SourceID, p => p.chunk(), listener);

				ScriptLoadingContext lcontext = CreateLoadingContext(source);
				ChunkStatement stat;

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.AstCreation))
					stat = new ChunkStatement(parser.chunk(), lcontext, globalContext);

				int beginIp = -1;

				//var srcref = new SourceRef(source.SourceID);

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.Compilation))
				using (bytecode.EnterSource(null))
				{
					bytecode.Emit_Nop(string.Format("Begin chunk {0}", source.Name));
					beginIp = bytecode.GetJumpPointForLastInstruction();
					stat.Compile(bytecode);
					bytecode.Emit_Nop(string.Format("End chunk {0}", source.Name));
				}

				Debug_DumpByteCode(bytecode, source.SourceID);

				return beginIp;
			}
			catch (ParseCanceledException ex)
			{
				HandleParserError(ex, listener);
				throw;
			}
		}

		internal static int LoadFunction(Script script, SourceCode source, ByteCode bytecode, Table globalContext)
		{
			AntlrErrorListener listener = new AntlrErrorListener(source);
			try
			{
				LuaParser parser = CreateParser(script, new AntlrInputStream(source.Code), source.SourceID, p => p.anonfunctiondef(), listener);

				ScriptLoadingContext lcontext = CreateLoadingContext(source);
				FunctionDefinitionExpression fndef;

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.AstCreation))
					fndef = new FunctionDefinitionExpression(parser.anonfunctiondef(), lcontext, false, globalContext);

				int beginIp = -1;

				// var srcref = new SourceRef(source.SourceID);

				using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.Compilation))
				using (bytecode.EnterSource(null))
				{
					bytecode.Emit_Nop(string.Format("Begin function {0}", source.Name));
					beginIp = fndef.CompileBody(bytecode, source.Name);
					bytecode.Emit_Nop(string.Format("End function {0}", source.Name));

					Debug_DumpByteCode(bytecode, source.SourceID);
				}

				return beginIp;
			}
			catch (ParseCanceledException ex)
			{
				HandleParserError(ex, listener);
				throw;
			}
		}

		private static void HandleParserError(ParseCanceledException ex, AntlrErrorListener listener)
		{
			string msg = listener.Message ?? (string.Format("Unknown syntax error. <eof> expected ? : {0}", ex.Message));

			throw new SyntaxErrorException(msg);
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


		private static ScriptLoadingContext CreateLoadingContext(SourceCode source)
		{
			return new ScriptLoadingContext()
			{
				Scope = new BuildTimeScope(),
				Source = source
			};
		}

		private static LuaParser CreateParser(Script script, ICharStream charStream, int sourceIdx, Func<LuaParser, IParseTree> dumper, AntlrErrorListener errorListener)
		{
			LuaLexer lexer;
			LuaParser parser;

			using (script.PerformanceStats.StartStopwatch(Diagnostics.PerformanceCounter.Parsing))
			{
				lexer = new LuaLexer(charStream);
				lexer.RemoveErrorListeners();
				lexer.AddErrorListener(errorListener);

				parser = new LuaParser(new CommonTokenStream(lexer))
				{
					ErrorHandler = new BailErrorStrategy(),
				};

				parser.Interpreter.PredictionMode = PredictionMode.Ll;
				parser.RemoveErrorListeners();
				parser.AddErrorListener(errorListener);
				//Debug_DumpAst(parser, sourceIdx, dumper);
			}


			return parser;
		}


	}
}
