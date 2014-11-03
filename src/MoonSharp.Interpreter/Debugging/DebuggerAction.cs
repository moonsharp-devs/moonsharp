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
		public DateTime TimeStampUTC { get; private set; }

		public DebuggerAction()
		{
			TimeStampUTC = DateTime.UtcNow;
		}

		public TimeSpan Age { get { return DateTime.UtcNow - TimeStampUTC; } }


		public override string ToString()
		{
			return string.Format("{0}({1})", Action, InstructionPtr);
		}
	}
}
