#define DEBUG_COMPILER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ChunkStatement : Statement
	{
		Statement m_Block;
		RuntimeScopeFrame m_StackFrame;

		public ChunkStatement(LuaParser.ChunkContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			lcontext.Scope.PushFunction();
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.PopFunction();
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.BeginFn(m_StackFrame, "<chunk-root>");
			m_Block.Compile(bc);
			bc.Ret(0);
			//bc.Leave(m_StackFrame);
		}

		internal static int LoadFromICharStream(ICharStream charStream, string chunkName, ByteCode bytecode)
		{
			LuaLexer lexer;
			LuaParser parser;

			using (var _ = new CodeChrono("ChunkStatement.LoadFromICharStream/Parsing"))
			{
				lexer = new LuaLexer(charStream);
				parser = new LuaParser(new CommonTokenStream(lexer));
			}
#if DEBUG_COMPILER
			AstDump astDump = new AstDump();
			astDump.DumpTree(parser.chunk(), @"c:\temp\treedump.txt");
			parser.Reset();
#endif
			ChunkStatement stat;
			using (var _ = new CodeChrono("ChunkStatement.LoadFromICharStream/AST"))
			{
				var lcontext = new ScriptLoadingContext() { Scope = new BuildTimeScope() };
				stat = new ChunkStatement(parser.chunk(), lcontext);
			}

			int beginIp = -1;

			using (var _ = new CodeChrono("ChunkStatement.LoadFromICharStream/Compile"))
			{
				bytecode.Nop(string.Format("Begin chunk {0}", chunkName));
				beginIp = bytecode.GetJumpPointForLastInstruction();
				stat.Compile(bytecode);
				bytecode.Nop(string.Format("End chunk {0}", chunkName));
#if DEBUG_COMPILER
				bytecode.Dump(@"c:\temp\codedump.txt");
#endif
			}

			return beginIp;
		}
	}
}
