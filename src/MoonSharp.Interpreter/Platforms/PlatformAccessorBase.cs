using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	/// <summary>
	/// An abstract class which offers basic services on top of IPlatformAccessor to provide easier implementation of platforms.
	/// </summary>
	public abstract class PlatformAccessorBase : IPlatformAccessor
	{
		private bool? m_IsAOT = null;


		/// <summary>
		/// Resolves the name of a module on a set of paths.
		/// </summary>
		/// <param name="modname">The modname.</param>
		/// <param name="paths">The paths.</param>
		/// <returns></returns>
		protected virtual string ResolveModuleName(string modname, string[] paths)
		{
			modname = modname.Replace('.', '/');

			foreach (string path in paths)
			{
				string file = path.Replace("?", modname);

				if (ScriptFileExists(file))
					return file;
			}

			return null;
		}

		/// <summary>
		/// Resolves the name of a module to a filename (which will later be passed to OpenScriptFile).
		/// The resolution happens first on paths included in the LUA_PATH global variable, and - 
		/// if the variable does not exist - by consulting the
		/// ScriptOptions.ModulesPaths array. Override to provide a different behaviour.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="modname">The modname.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		public virtual string ResolveModuleName(Script script, string modname, Table globalContext)
		{
			DynValue s = (globalContext ?? script.Globals).RawGet("LUA_PATH");

			if (s != null && s.Type == DataType.String)
				return ResolveModuleName(modname, ScriptOptions.UnpackStringPaths(s.String));

			return ResolveModuleName(modname, script.Options.ModulesPaths);
		}


		/// <summary>
		/// Determines whether the application is running in AOT (ahead-of-time) mode
		/// </summary>
		/// <returns></returns>
		public virtual bool IsRunningOnAOT()
		{
			if (m_IsAOT.HasValue) 
				return m_IsAOT.Value;

			try
			{
				System.Linq.Expressions.Expression e = System.Linq.Expressions.Expression.Constant(5, typeof(int));
				var lambda = System.Linq.Expressions.Expression.Lambda<Func<int>>(e);
				lambda.Compile();
				m_IsAOT = false;
			}
			catch (Exception)
			{
				m_IsAOT = true;
			}

			return m_IsAOT.Value;
		}


		/// <summary>
		/// Checks if a script file exists. 
		/// </summary>
		/// <param name="name">The script filename.</param>
		/// <returns></returns>
		public abstract bool ScriptFileExists(string name);

		/// <summary>
		/// Opens a file for reading the script code.
		/// It can return either a string, a byte[] or a Stream.
		/// If a byte[] is returned, the content is assumed to be a serialized (dumped) bytecode. If it's a string, it's
		/// assumed to be either a script or the output of a string.dump call. If a Stream, autodetection takes place.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="file">The file.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns>
		/// A string, a byte[] or a Stream.
		/// </returns>
		public abstract object OpenScriptFile(Script script, string file, Table globalContext);

		/// <summary>
		/// Gets the platform name prefix
		/// </summary>
		/// <returns></returns>
		public abstract string GetPlatformNamePrefix();

		/// <summary>
		/// Gets the name of the platform (used for debug purposes).
		/// </summary>
		/// <returns>
		/// The name of the platform (used for debug purposes)
		/// </returns>
		public string GetPlatformName()
		{
			string suffix = null;

			if (PlatformAutoSelector.IsRunningOnUnity)
			{
				if (PlatformAutoSelector.IsRunningOnMono)
					suffix = "unity.mono";
				else
					suffix = "unity.webp";
			}
			else if (PlatformAutoSelector.IsRunningOnMono)
				suffix = "mono";
			else
				suffix = "dotnet";

			if (PlatformAutoSelector.IsPortableFramework)
				suffix = suffix + ".portable";
			
			if (PlatformAutoSelector.IsRunningOnClr4)
				suffix = suffix + ".clr4";
			else
				suffix = suffix + ".clr2";

			if (IsRunningOnAOT())
				suffix = suffix + ".aot";

			return GetPlatformNamePrefix() + "." + suffix;
		}

		/// <summary>
		/// Default handler for 'print' calls. Can be customized in ScriptOptions
		/// </summary>
		/// <param name="content">The content.</param>
		public abstract void DefaultPrint(string content);

		/// <summary>
		/// Default handler for interactive line input calls. Can be customized in ScriptOptions.
		/// If an inheriting class whants to give a meaningful implementation, this method MUST be overridden.
		/// </summary>
		/// <returns>null</returns>
		public virtual string DefaultInput()
		{
			return null;
		}

		/// <summary>
		/// A function used to open files in the 'io' module. 
		/// Can have an invalid implementation if 'io' module is filtered out.
		/// It should return a correctly initialized Stream for the given file and access
		/// </summary>
		/// <param name="script"></param>
		/// <param name="filename">The filename.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="mode">The mode (as per Lua usage - e.g. 'w+', 'rb', etc.).</param>
		/// <returns></returns>
		public abstract Stream IO_OpenFile(Script script, string filename, Encoding encoding, string mode);


		/// <summary>
		/// Gets a standard stream (stdin, stdout, stderr).
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public abstract Stream IO_GetStandardStream(StandardFileType type);


		/// <summary>
		/// Gets a temporary filename. Used in 'io' and 'os' modules.
		/// Can have an invalid implementation if 'io' and 'os' modules are filtered out.
		/// </summary>
		/// <returns></returns>
		public abstract string IO_OS_GetTempFilename();


		/// <summary>
		/// Exits the process, returning the specified exit code.
		/// Can have an invalid implementation if the 'os' module is filtered out.
		/// </summary>
		/// <param name="exitCode">The exit code.</param>
		public abstract void OS_ExitFast(int exitCode);


		/// <summary>
		/// Checks if a file exists. Used by the 'os' module.
		/// Can have an invalid implementation if the 'os' module is filtered out.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <returns>
		/// True if the file exists, false otherwise.
		/// </returns>
		public abstract bool OS_FileExists(string file);


		/// <summary>
		/// Deletes the specified file. Used by the 'os' module.
		/// Can have an invalid implementation if the 'os' module is filtered out.
		/// </summary>
		/// <param name="file">The file.</param>
		public abstract void OS_FileDelete(string file);


		/// <summary>
		/// Moves the specified file. Used by the 'os' module.
		/// Can have an invalid implementation if the 'os' module is filtered out.
		/// </summary>
		/// <param name="src">The source.</param>
		/// <param name="dst">The DST.</param>
		public abstract void OS_FileMove(string src, string dst);


		/// <summary>
		/// Executes the specified command line, returning the child process exit code and blocking in the meantime.
		/// Can have an invalid implementation if the 'os' module is filtered out.
		/// </summary>
		/// <param name="cmdline">The cmdline.</param>
		/// <returns></returns>
		public abstract int OS_Execute(string cmdline);


		/// <summary>
		/// Filters the CoreModules enumeration to exclude non-supported operations
		/// </summary>
		/// <param name="module">The requested modules.</param>
		/// <returns>
		/// The requested modules, with unsupported modules filtered out.
		/// </returns>
		public abstract CoreModules FilterSupportedCoreModules(CoreModules module);

		/// <summary>
		/// Gets an environment variable. Must be implemented, but an implementation is allowed
		/// to always return null if a more meaningful implementation cannot be achieved or is
		/// not desired.
		/// </summary>
		/// <param name="envvarname">The envvarname.</param>
		/// <returns>
		/// The environment variable value, or null if not found
		/// </returns>
		public abstract string GetEnvironmentVariable(string envvarname);

	}
}
