using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MoonSharp.Interpreter.Diagnostics
{
	class CodeChrono : IDisposable
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void WriteLine(string source, string msg, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format(msg, args));
		}

		string m_Desc;
		Stopwatch m_Stopwatch;

		public CodeChrono(string descFormat, params object[] args)
		{
			m_Desc = string.Format(descFormat, args);
			m_Stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			m_Stopwatch.Stop();
			WriteLine("CodeChrono", "Activity {0} took {1}ms", m_Desc, m_Stopwatch.ElapsedMilliseconds);
		}
	}
}
