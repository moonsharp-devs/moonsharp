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

		public ChunkStatement(LuaParser.ChunkContext context, ScriptLoadingContext lcontext, Table globalEnv)
			: base(context, lcontext)
		{
			lcontext.Scope.PushFunction(this, false);
			m_Env = lcontext.Scope.DefineLocal(WellKnownSymbols.ENV);
			
			m_GlobalEnv = globalEnv;

			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);
			m_StackFrame = lcontext.Scope.PopFunction();
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_BeginFn(m_StackFrame, "<chunk-root>");

			bc.Emit_Literal(DynValue.NewTable(m_GlobalEnv));
			bc.Emit_Store(m_Env, 0, 0);
			bc.Emit_Pop();

			m_Block.Compile(bc);
			bc.Emit_Ret(0);
		}

		public SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol)
		{
			return null;
		}
	}
}
