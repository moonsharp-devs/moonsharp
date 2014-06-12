using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
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

		public override void Compile(Execution.VM.Chunk bc)
		{
			bc.BeginFn(m_StackFrame, "<chunk-root>");
			m_Block.Compile(bc);
			bc.Ret(0);
			//bc.Leave(m_StackFrame);
		}

	}
}
