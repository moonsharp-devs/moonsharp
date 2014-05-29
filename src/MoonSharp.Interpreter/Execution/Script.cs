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
		CompositeStatement m_Script;
		ScriptLoadingContext m_LoadingContext;
		RuntimeScope runtimeScope;
		Chunk m_GlobalChunk;

		internal Script(CompositeStatement stat, ScriptLoadingContext lcontext)
		{
			m_Script = stat;
			m_LoadingContext = lcontext;
			runtimeScope = m_LoadingContext.Scope.SpawnRuntimeScope();
			Compile();
		}

		public void Compile()
		{
			m_GlobalChunk = new Chunk();
			m_GlobalChunk.Nop("Script start");
			m_Script.Compile(m_GlobalChunk);
			m_GlobalChunk.Nop("Script end");

			m_GlobalChunk.Dump(@"c:\temp\codedump.txt");
		}

		public RValue Execute()
		{

			VmExecutor executor = new VmExecutor(m_GlobalChunk, runtimeScope);

			using (var _ = new CodeChrono("MoonSharpScript.Execute"))
			{
				return executor.Execute();
			}

			//using (var _ = new CodeChrono("MoonSharpScript.Execute"))
			//{
			//	return m_Script.ExecRoot(runtimeScope);
			//}
		}

		public RValue OldExecute()
		{
			using (var _ = new CodeChrono("MoonSharpScript.Execute"))
			{
				return m_Script.ExecRoot(runtimeScope);
			}
		}

	}
}
