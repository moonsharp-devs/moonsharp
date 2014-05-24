using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class ExecutionFlow
	{
		public ExecutionFlowType Type { get; private set; }

		public string GoToLabel { get; private set; }

		public RValue ReturnValue { get; private set; }

		private ExecutionFlow()
		{ }

		public static ExecutionFlow GoTo(string label)
		{
			return new ExecutionFlow()
			{
				GoToLabel = label,
				Type = ExecutionFlowType.GoTo
			};
		}

		public static ExecutionFlow Return(RValue value)
		{
			return new ExecutionFlow()
			{
				ReturnValue = value,
				Type = ExecutionFlowType.Return
			};
		}

		private static ExecutionFlow s_None = new ExecutionFlow() { Type = ExecutionFlowType.None };
		private static ExecutionFlow s_Break = new ExecutionFlow() { Type = ExecutionFlowType.Break };
		private static ExecutionFlow s_Continue = new ExecutionFlow() { Type = ExecutionFlowType.Continue };

		public static ExecutionFlow None { get { return s_None; } }
		public static ExecutionFlow Break { get { return s_Break; } }
		public static ExecutionFlow Continue { get { return s_Continue; } }


		public bool ChangesFlow() { return this.Type != ExecutionFlowType.None; }

		public override string ToString()
		{
			switch (this.Type)
			{
				case ExecutionFlowType.None:
					return "none";
				case ExecutionFlowType.GoTo:
					return string.Format("goto {0}", this.GoToLabel);
				case ExecutionFlowType.Break:
					return "break";
				case ExecutionFlowType.Continue:
					return "continue";
				case ExecutionFlowType.Return:
					return string.Format("return {0}", this.ReturnValue);
				default:
					return "!!UNKNOWN FLOW!!";
			}
		}

	}
}
