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
			ByteCodeStepIn,
			ByteCodeStepOver,
			ByteCodeStepOut,
			StepIn,
			StepOver,
			StepOut,
			Run,
			ToggleBreakpoint,
			SetBreakpoint,
			ClearBreakpoint,
			Refresh,
			HardRefresh,
			None,
		}

		public ActionType Action { get; set; }
		public DateTime TimeStampUTC { get; private set; }

		public int SourceID { get; set; }
		public int SourceLine { get; set; }
		public int SourceCol { get; set; }

		public DebuggerAction()
		{
			TimeStampUTC = DateTime.UtcNow;
		}

		public TimeSpan Age { get { return DateTime.UtcNow - TimeStampUTC; } }


		public override string ToString()
		{
			if (Action == ActionType.ToggleBreakpoint || Action == ActionType.SetBreakpoint || Action == ActionType.ClearBreakpoint)
			{
				return string.Format("{0} {1}:({2},{3})", Action, SourceID, SourceLine, SourceCol);
			}
			else
			{
				return Action.ToString();
			}
		}
	}
}
