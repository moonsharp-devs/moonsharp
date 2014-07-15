using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter
{
	public class StackTraceItem
	{
		public int CurrentInstruction { get; internal set; }
		public int EntryPoint { get; internal set; } 
		public int BasePtr { get; internal set; } 
		public int RetAddress { get; internal set; } 
		public string Name { get; internal set; } 
		public SourceRef SourceRef { get; internal set; } 
	}
}
