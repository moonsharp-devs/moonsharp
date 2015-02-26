using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	public interface IScriptLoader 
	{
		/// <summary>
		/// Loads the file.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		object LoadFile(string file, Table globalContext);
		/// <summary>
		/// Resolves a filename [applying paths, etc.]
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		string ResolveFileName(string filename, Table globalContext);
		/// <summary>
		/// Resolves the name of the module.
		/// </summary>
		/// <param name="modname">The modname.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		string ResolveModuleName(string modname, Table globalContext);
	}
}
