using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Loaders
{
	public interface IScriptLoader 
	{
		bool HasCustomFileLoading();
		string LoadFile(string file, Table globalContext);
		string ResolveFileName(string filename, Table globalContext);
		string ResolveModuleName(string modname, Table globalContext);
	}
}
