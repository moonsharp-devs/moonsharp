using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// State of coroutines
	/// </summary>
	public enum CoroutineState
	{
		/// <summary>
		/// This is the main coroutine
		/// </summary>
		Main,
		/// <summary>
		/// Coroutine has not started yet
		/// </summary>
		NotStarted,
		/// <summary>
		/// Coroutine is suspended
		/// </summary>
		Suspended,
		/// <summary>
		/// Coroutine is running
		/// </summary>
		Running,
		/// <summary>
		/// Coroutine has terminated
		/// </summary>
		Dead
	}
}
