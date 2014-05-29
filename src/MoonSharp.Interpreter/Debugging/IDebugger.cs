using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Debugging
{
	public interface IDebugger
	{
		void Init(IDebugGuest debugGuest);
		void OnBreak();
	}
}
