using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Debugging
{
	/// <summary>
	/// Class providing services specific to debugger implementations.
	/// </summary>
	/// <seealso cref="MoonSharp.Interpreter.IScriptPrivateResource" />
	public sealed class DebugService : IScriptPrivateResource
	{
		Processor m_Processor;

		internal DebugService(Script script, Processor processor)
		{
			OwnerScript = script;
			m_Processor = processor;
		}

		/// <summary>
		/// Gets the script owning this resource.
		/// </summary>
		/// <value>
		/// The script owning this resource.
		/// </value>
		public Script OwnerScript { get; private set;  }

		/// <summary>
		/// Resets the break points for a given file. Supports only line-based breakpoints.
		/// </summary>
		/// <param name="src">The source.</param>
		/// <param name="breakpointLines">The breakpoint lines, each associated an optional breakpoint condition.</param>
		/// <returns>The lines for which breakpoints have been set</returns>
		public Dictionary<int, DynamicExpression> ResetBreakPoints(SourceCode src, Dictionary<int, DynamicExpression> breakpointLines)
		{
			return m_Processor.ResetBreakPoints(src, breakpointLines);
		}



	}
}
