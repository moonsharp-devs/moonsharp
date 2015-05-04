using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class X1
	{
		public class MyException : Exception
		{

		}

		class BreakAfterManyInstructionsDebugger : IDebugger
		{
			int m_InstructionCounter = 0;
			List<DynamicExpression> m_Dynamics = new List<DynamicExpression>();

			public void SetSourceCode(SourceCode sourceCode)
			{
			}

			public void SetByteCode(string[] byteCode)
			{
			}

			public bool IsPauseRequested()
			{
				return true;
			}

			public bool SignalRuntimeException(ScriptRuntimeException ex)
			{
				return false;
			}

			public DebuggerAction GetAction(int ip, SourceRef sourceref)
			{
				m_InstructionCounter += 1;

				if ((m_InstructionCounter % 1000) == 0)
					Console.Write(".");

				if (m_InstructionCounter > 50000)
					throw new MyException();

				return new DebuggerAction()
				{
					Action = DebuggerAction.ActionType.StepIn,
				};
			}

			public void SignalExecutionEnded()
			{
			}

			public void Update(WatchType watchType, IEnumerable<WatchItem> items)
			{
			}

			public List<DynamicExpression> GetWatchItems()
			{
				return m_Dynamics;
			}

			public void RefreshBreakpoints(IEnumerable<SourceRef> refs)
			{
			}
		}







		[Tutorial]
		static void BreakAfterManyInstructions()
		{
			Script script = new Script();
			try
			{
				script.AttachDebugger(new BreakAfterManyInstructionsDebugger());

				script.DoString(@"
				x = 0;
				while true do x = x + 1; end;
				");

			}
			catch (MyException)
			{
				Console.WriteLine("Done. x = {0}", script.Globals["x"]);

			}


		}



	}
}
