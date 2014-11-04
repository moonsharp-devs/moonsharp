using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Debugging
{
	public interface IDebugger
	{
		void SetSourceCode(SourceCode sourceCode);
		void SetByteCode(string[] byteCode);
		bool IsPauseRequested();
		DebuggerAction GetAction(int ip, SourceRef sourceref);
		void SignalExecutionEnded();
		void Update(WatchType watchType, IEnumerable<WatchItem> items);
		List<string> GetWatchItems();
		void RefreshBreakpoints(IEnumerable<SourceRef> refs);
	}
}
