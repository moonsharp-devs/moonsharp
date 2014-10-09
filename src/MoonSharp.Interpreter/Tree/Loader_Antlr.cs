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
	/// <summary>
	/// Class managing most of interactions with ANTLR.
	/// </summary>
	internal static class Loader_Antlr
	{
		private class StringAccumulatorErrorListener : BaseErrorListener, IAntlrErrorListener<int> 
		{
			string m_Msg = null;
			string m_File;
			string m_Code = null;

			public StringAccumulatorErrorListener(string filename, string code)
			{
				m_File = filename;
				m_Code = code;
			}

			public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
			{
				if (m_Msg == null) m_Msg = "";

				m_Msg += string.Format("{0}[{1},{2}] : Syntax error near '{3} : {4}'\n",
					m_File, line, charPositionInLine, offendingSymbol, msg);

				m_Msg += UnderlineError(offendingSymbol.StartIndex, offendingSymbol.StopIndex, line, charPositionInLine);
			}

			public string Message { get { return m_Msg; } }

			public override string ToString()
			{
				return m_Msg ?? "(null)";
			}

			public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
			{
				if (m_Msg == null) m_Msg = "";

				m_Msg += string.Format("{0}[{1},{2}] : Syntax error : {3}'\n",
					m_File, line, charPositionInLine, msg);

				m_Msg += UnderlineError(-1, -1, line, charPositionInLine);
			}

			protected string UnderlineError(int startIndex, int stopIndex, int line, int charPositionInLine)
			{
				string input = m_Code;
				string[] lines = input.Split('\n');
				StringBuilder errorMessage = new StringBuilder();
				errorMessage.AppendLine(lines[line - 1].Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '));

				for (int i = 0; i < charPositionInLine; i++)
				{
					errorMessage.Append(' ');
				}

				if (startIndex >= 0 && stopIndex >= 0)
				{
					for (int i = startIndex; i <= stopIndex; i++)
						errorMessage.Append('^');
				}
				else
				{
					errorMessage.Append("^...");
				}

				errorMessage.AppendLine();
				return errorMessage.ToString();
			}
		}

		internal static int LoadChunk(Script script, string code, ByteCode bytecode, string sourceName, int sourceIdx, Table globalContext)
		{
			StringAccumulatorErrorListener listener = new StringAccumulatorErrorListener(sourceName, code);

			try
			{
				LuaParser parser = CreateParser(script, new AntlrInputStream(code), sourceIdx, p => p.chunk(), listener);

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
				HandleParserError(ex, listener);
				throw;
			}
		}

		internal static int LoadFunction(Script script, string code, ByteCode bytecode, string sourceName, int sourceIdx, Table globalContext)
		{
			StringAccumulatorErrorListener listener = new StringAccumulatorErrorListener(sourceName, code);
			try
			{
				LuaParser parser = CreateParser(script, new AntlrInputStream(code), sourceIdx, p => p.anonfunctiondef(), listener);

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
				HandleParserError(ex, listener);
				throw;
			}
		}

		private static void HandleParserError(ParseCanceledException ex, StringAccumulatorErrorListener listener)
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


		private static ScriptLoadingContext CreateLoadingContext(string sourceName, int sourceIdx)
		{
			return new ScriptLoadingContext()
			{
				Scope = new BuildTimeScope(),
				SourceIdx = sourceIdx,
				SourceName = sourceName,
			};
		}

		private static LuaParser CreateParser(Script script, ICharStream charStream, int sourceIdx, Func<LuaParser, IParseTree> dumper, StringAccumulatorErrorListener errorListener)
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
