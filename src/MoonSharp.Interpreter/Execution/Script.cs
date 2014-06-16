using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Debugging;
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
		Processor m_Main;

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
			if (m_Main == null)
				m_Main = new Processor(m_GlobalChunk);
		}

		public RValue Execute(Table globalContext)
		{
			m_Main.Reset(globalContext ?? new Table());

			using (var _ = new CodeChrono("MoonSharpScript.Execute"))
			{
				return m_Main.InvokeRoot();
			}
		}


		public void AttachDebugger(IDebugger debugger)
		{
			if (debugger != null)
				debugger.SetSourceCode(m_GlobalChunk, null);

			m_Main.AttachDebugger(debugger);
		}


	}
}
