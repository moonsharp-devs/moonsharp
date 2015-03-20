using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	internal interface IClosureBuilder
	{
		SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol);

	}
}
