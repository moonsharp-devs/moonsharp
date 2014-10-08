using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Diagnostics.PerformanceCounters;

namespace MoonSharp.Interpreter.Diagnostics
{
	/// <summary>
	/// A single object of this type exists for every script and gives access to performance statistics.
	/// </summary>
	public class PerformanceStatistics
	{
		IPerformanceStopwatch[] m_Stopwatches = new IPerformanceStopwatch[(int)PerformanceCounter.LastValue];
		static IPerformanceStopwatch[] m_GlobalStopwatches = new IPerformanceStopwatch[(int)PerformanceCounter.LastValue];

		static PerformanceStatistics()
		{
			m_GlobalStopwatches[(int)PerformanceCounter.AdaptersCompilation] = new GlobalPerformanceStopwatch(PerformanceCounter.AdaptersCompilation);
		}

		internal PerformanceStatistics()
		{
			for (int i = 0; i < (int)PerformanceCounter.LastValue; i++)
				m_Stopwatches[i] = m_GlobalStopwatches[i] ?? new PerformanceStopwatch((PerformanceCounter)i);
		}

		/// <summary>
		/// Gets the result of the specified performance counter .
		/// </summary>
		/// <param name="pc">The PerformanceCounter.</param>
		/// <returns></returns>
		public PerformanceResult GetPerformanceCounterResult(PerformanceCounter pc)
		{
			return m_Stopwatches[(int)pc].GetResult();
		}

		/// <summary>
		/// Starts a stopwatch.
		/// </summary>
		/// <returns></returns>
		internal IDisposable StartStopwatch(PerformanceCounter pc)
		{
			return m_Stopwatches[(int)pc].Start();
		}

		/// <summary>
		/// Starts a stopwatch.
		/// </summary>
		/// <returns></returns>
		internal static IDisposable StartGlobalStopwatch(PerformanceCounter pc)
		{
			return m_GlobalStopwatches[(int)pc].Start();
		}

		/// <summary>
		/// Gets a string with a complete performance log.
		/// </summary>
		/// <returns></returns>
		public string GetPerformanceLog()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < (int)PerformanceCounter.LastValue; i++)
				sb.AppendLine(this.GetPerformanceCounterResult((PerformanceCounter)i).ToString());

			return sb.ToString();
		}
	}
}
