#if !PCL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Loaders
{
	/// <summary>
	/// A script loader loading scripts directly from the file system (does not go through platform object)
	/// AND starts with module paths taken from environment variables (again, not going through the platform object)
	/// </summary>
	public class ReplInterpreterScriptLoader : FileSystemScriptLoader
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReplInterpreterScriptLoader"/> class.
		/// </summary>
		public ReplInterpreterScriptLoader()
		{
			string env = Environment.GetEnvironmentVariable("MOONSHARP_PATH");
			if (!string.IsNullOrEmpty(env)) ModulePaths = UnpackStringPaths(env);

			if (ModulePaths == null)
			{
				env = Environment.GetEnvironmentVariable("LUA_PATH");
				if (!string.IsNullOrEmpty(env)) ModulePaths = UnpackStringPaths(env);
			}

			if (ModulePaths == null)
			{
				ModulePaths = UnpackStringPaths("?;?.lua");
			}
		}
	}
}


#endif