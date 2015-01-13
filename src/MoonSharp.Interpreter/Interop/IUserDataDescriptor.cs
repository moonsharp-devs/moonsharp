using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public interface IUserDataDescriptor
	{
		string Name { get; }
		Type Type { get; }
		DynValue Index(Script script, object obj, DynValue index);
		bool SetIndex(Script script, object obj, DynValue index, DynValue value);
		string AsString(object obj);
		DynValue MetaIndex(Script script, object obj, string metaname);
	}
}
