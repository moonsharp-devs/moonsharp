using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	public enum SymbolRefType
	{
		Local,
		Upvalue,
		Global,
		DefaultEnv,
	}
}
