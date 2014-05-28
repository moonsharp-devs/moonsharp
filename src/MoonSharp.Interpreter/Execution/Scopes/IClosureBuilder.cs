using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public interface IClosureBuilder
	{
		object UpvalueCreationTag { get; set; }
		LRef CreateUpvalue(BuildTimeScope scope, LRef symbol);

	}
}
