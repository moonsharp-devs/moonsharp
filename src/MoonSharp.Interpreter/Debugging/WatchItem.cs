using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Debugging
{
	public class WatchItem
	{
		public int Address { get; set; }
		public int BasePtr { get; set; }
		public int RetAddress { get; set; }
		public string Name { get; set; }
		public RValue Value { get; set; }
		public LRef LValue { get; set; }
	}
}
