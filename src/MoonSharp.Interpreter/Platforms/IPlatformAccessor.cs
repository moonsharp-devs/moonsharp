using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	public interface IPlatformAccessor
	{
		/// <summary>
		/// A function used to open files in the 'io' and 'os' modules. 
		/// It should return a correctly initialized Stream for the given file and access
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="mode">The mode (as per Lua usage - e.g. 'w+', 'rb', etc.).</param>
		/// <returns></returns>
		Stream OpenFileForIO(Script script, string filename, Encoding encoding, string mode);

		/// <summary>
		/// Opens a file for reading the script code.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		object LoadFile(Script script, string file, Table globalContext);

		/// <summary>
		/// Resolves the name of the module.
		/// </summary>
		/// <param name="modname">The modname.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		string ResolveModuleName(Script script, string modname, Table globalContext);

		/// <summary>
		/// Filters the CoreModules enumeration to exclude non-supported operations
		/// </summary>
		/// <param name="module">The requested modules.</param>
		/// <returns>The requested modules, with unsupported modules filtered out.</returns>
		CoreModules FilterSupportedCoreModules(CoreModules module);

		/// <summary>
		/// Gets an environment variable.
		/// </summary>
		/// <param name="envvarname">The envvarname.</param>
		/// <returns>The environment variable value, or null if not found</returns>
		string GetEnvironmentVariable(string envvarname);

		/// <summary>
		/// Checks if a file exists
		/// </summary>
		/// <param name="name">The file name.</param>
		bool FileExists(string name);

		/// <summary>
		/// Determines whether the application is running in AOT (ahead-of-time) mode
		/// </summary>
		bool IsRunningOnAOT();


		string GetPlatformName();

		Stream GetStandardStream(StandardFileType type);



		void DefaultPrint(string content);
		string DefaultInput();


	}
}
