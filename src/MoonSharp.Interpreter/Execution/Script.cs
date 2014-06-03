using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Execution
{
	public class Script
	{
		ChunkStatement m_Script;
		ScriptLoadingContext m_LoadingContext;
		Chunk m_GlobalChunk;

		internal Script(ChunkStatement stat, ScriptLoadingContext lcontext)
		{
			m_Script = stat;
			m_LoadingContext = lcontext;
			Compile();
		}

		public void Compile()
		{
			m_GlobalChunk = new Chunk();
			m_GlobalChunk.Nop("Script start");
			m_Script.Compile(m_GlobalChunk);
			m_GlobalChunk.Nop("Script end");

#if DEBUG
			m_GlobalChunk.Dump(@"c:\temp\codedump.txt");
#endif
		}

		RuntimeScope scope = new RuntimeScope();
		VmExecutor executor;

		public RValue Execute(Table globalContext)
		{
			scope.GlobalTable = globalContext ?? new Table();

			if (executor == null)
			{
				executor= new VmExecutor(m_GlobalChunk, scope);
			}
			executor.Reset();

			using (var _ = new CodeChrono("MoonSharpScript.Execute"))
			{
				return executor.Execute();
			}
		}


	}
}
