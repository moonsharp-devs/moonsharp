using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Hardwire
{
	public interface ICodeGenerationLogger
	{
		void LogError(string message);
		void LogWarning(string message);
		void LogMinor(string message);
	}
}
