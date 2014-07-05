using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Debugging
{
	public class SourceCode
	{
		public string Name { get; private set; }
		public string Code { get; private set; }

		internal SourceCode(string name, string code)
		{
			Name = name;
			Code = code;
		}
	}
}
