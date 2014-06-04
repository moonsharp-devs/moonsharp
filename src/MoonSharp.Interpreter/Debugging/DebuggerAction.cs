using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Debugging
{
	public class DebuggerAction
	{
		public enum ActionType
		{
			StepIn,
			StepOver,
			Run,
			ToggleBreakpoint,
			Refresh,
			None,
		}

		public int InstructionPtr { get; set; }
		public ActionType Action { get; set; }

		public override string ToString()
		{
			return string.Format("{0}({1})", Action, InstructionPtr);
		}
	}
}
