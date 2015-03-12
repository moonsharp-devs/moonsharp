using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ChunkStatement : Statement, IClosureBuilder
	{
		Statement m_Block;
		RuntimeScopeFrame m_StackFrame;
		Table m_GlobalEnv;
		SymbolRef m_Env;
		SymbolRef m_VarArgs;

		public ChunkStatement(ScriptLoadingContext lcontext, Table globalEnv)
			: base(lcontext)
		{
			lcontext.Scope.PushFunction(this, true);
			m_Env = lcontext.Scope.DefineLocal(WellKnownSymbols.ENV);
			m_VarArgs = lcontext.Scope.DefineLocal(WellKnownSymbols.VARARGS);

			m_GlobalEnv = globalEnv;

			m_Block = new CompositeStatement(lcontext);

			Token T = lcontext.Lexer.Current();

			while (T.Type == TokenType.SemiColon)
				T = lcontext.Lexer.Next();

			if (T.Type != TokenType.Eof)
			{
				throw new SyntaxErrorException("<eof> expected near '{0}'", T.Text);
			}

			m_StackFrame = lcontext.Scope.PopFunction();
		}


		public ChunkStatement(LuaParser.ChunkContext context, ScriptLoadingContext lcontext, Table globalEnv)
			: base(context, lcontext)
		{
			lcontext.Scope.PushFunction(this, true);
			m_Env = lcontext.Scope.DefineLocal(WellKnownSymbols.ENV);
			m_VarArgs = lcontext.Scope.DefineLocal(WellKnownSymbols.VARARGS);
			
			m_GlobalEnv = globalEnv;

			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.PopFunction();
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			Instruction meta = bc.Emit_FuncMeta("<chunk-root>");
			int metaip = bc.GetJumpPointForLastInstruction();

			bc.Emit_BeginFn(m_StackFrame);
			bc.Emit_Args(m_VarArgs);

			bc.Emit_Literal(DynValue.NewTable(m_GlobalEnv));
			bc.Emit_Store(m_Env, 0, 0);
			bc.Emit_Pop();

			m_Block.Compile(bc);
			bc.Emit_Ret(0);

			meta.NumVal = bc.GetJumpPointForLastInstruction() - metaip;
		}

		public SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol)
		{
			return null;
		}
	}
}
