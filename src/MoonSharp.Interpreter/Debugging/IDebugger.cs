using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Debugging
{
	public interface IDebugger
	{
		void SetSourceCode(ByteCode byteCode, string[] code);
		DebuggerAction GetAction(int ip, SourceRef sourceref);
		void Update(WatchType watchType, List<WatchItem> items);
		List<string> GetWatchItems();
	}
}
