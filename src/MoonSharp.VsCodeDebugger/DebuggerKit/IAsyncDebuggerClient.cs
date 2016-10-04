using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.DebuggerKit
{
	public interface IAsyncDebuggerClient
	{
		void SendHostReady(bool hostReady);
		void SendSourceRef(SourceRef sourceref);
		void OnWatchesUpdated(WatchType watchType);
		void OnSourceCodeChanged(int sourceID);
		void OnExecutionEnded();
	}
}
