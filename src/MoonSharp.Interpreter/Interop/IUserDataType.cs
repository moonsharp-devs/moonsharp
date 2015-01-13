using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public interface IUserDataType
	{
		DynValue Index(Script script, DynValue index);
		bool SetIndex(Script script, DynValue index, DynValue value);
		DynValue MetaIndex(Script script, string metaname);
	}
}
