using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
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

			if (lcontext.Lexer.Current.Type != TokenType.Eof)
				throw new SyntaxErrorException("<eof> expected near '{0}'", lcontext.Lexer.Current.Text);

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
