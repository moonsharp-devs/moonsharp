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
		public DynValue Value { get; set; }
		public SymbolRef LValue { get; set; }
		public bool IsError { get; set; }

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}:{3}:{4}:{5}",
				Address, BasePtr, RetAddress, Name ?? "(null)",
				Value != null ? Value.ToString() : "(null)",
				LValue != null ? LValue.ToString() : "(null)");
		}

	}
}
