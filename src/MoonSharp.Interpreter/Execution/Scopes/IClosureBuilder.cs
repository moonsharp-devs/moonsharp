using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public interface IClosureBuilder
	{
		object UpvalueCreationTag { get; set; }
		SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol);

	}
}
