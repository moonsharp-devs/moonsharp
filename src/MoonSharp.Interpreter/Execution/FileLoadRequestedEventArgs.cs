using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public enum FileLoadRequestResponseType
	{
		FilePath,
		ScriptCode,
		RegisteredModule
	}

	public class FileLoadRequestedEventArgs : EventArgs 
	{
		public string Name { get; internal set; }
		public bool IsModule { get; internal set; }
		public Script OriginatingScript { get; internal set; }
		public DynValue Coroutine { get; internal set; }

		public bool Handled { get; set; }
		public string Response { get; set; }
		public FileLoadRequestResponseType ResponseType { get; set; }
	}
}
