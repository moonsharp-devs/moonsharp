using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Platforms;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// This class contains options to customize behaviour of Script objects.
	/// </summary>
	public class ScriptOptions
	{
		internal ScriptOptions()
		{

		}

		internal ScriptOptions(ScriptOptions defaults)
		{
			this.DebugInput = defaults.DebugInput;
			this.DebugPrint = defaults.DebugPrint;

			this.UseLuaErrorLocations = defaults.UseLuaErrorLocations;
			this.Stdin = defaults.Stdin;
			this.Stdout = defaults.Stdout;
			this.Stderr = defaults.Stderr;
			this.ModulesPaths = defaults.ModulesPaths;

			this.CheckThreadAccess = defaults.CheckThreadAccess;
		}


		/// <summary>
		/// Gets or sets the debug print handler
		/// </summary>
		public Action<string> DebugPrint { get; set; }

		/// <summary>
		/// Gets or sets the debug input handler
		/// </summary>
		public Func<string> DebugInput { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether error messages will use Lua error locations instead of MoonSharp 
		/// improved ones. Use this for compatibility with legacy Lua code which parses error messages.
		/// </summary>
		public bool UseLuaErrorLocations { get; set; }

		/// <summary>
		/// Gets or sets the stream used as stdin. If null, a default stream is used.
		/// </summary>
		public Stream Stdin { get; set; }

		/// <summary>
		/// Gets or sets the stream used as stdout. If null, a default stream is used.
		/// </summary>
		public Stream Stdout { get; set; }

		/// <summary>
		/// Gets or sets the stream used as stderr. If null, a default stream is used.
		/// </summary>
		public Stream Stderr { get; set; }


		/// <summary>
		/// Gets or sets the modules paths used by the "require" function. If null, the default paths are used (using
		/// environment variables etc.). Note that this behaviour is subject to the implementation of the current
		/// Platform.
		/// </summary>
		/// <value>
		/// The modules path.
		/// </value>
		public string[] ModulesPaths { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether the thread check is enabled.
		/// A "lazy" thread check is performed everytime execution is entered to ensure that no two threads
		/// calls MoonSharp execution concurrently. However 1) the check is performed best effort (thus, it might
		/// not detect all issues) and 2) it might trigger in very odd legal situations (like, switching threads 
		/// inside a CLR-callback without actually having concurrency.
		/// 
		/// Disable this option if the thread check is giving problems in your scenario, but please check that
		/// you are not calling MoonSharp execution concurrently as it is not supported.
		/// </summary>
		public bool CheckThreadAccess { get; set; }





		/// <summary>
		/// Unpacks a string path to an array
		/// </summary>
		public static string[] UnpackStringPaths(string str)
		{
			return str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrEmpty(s))
				.ToArray();
		}

		/// <summary>
		/// Gets the default environment paths.
		/// </summary>
		internal static string[] GetDefaultEnvironmentPaths()
		{
			string[] modulePaths = null;

			if (modulePaths == null)
			{
				string env = Script.Platform.GetEnvironmentVariable("MOONSHARP_PATH");
				if (!string.IsNullOrEmpty(env)) modulePaths = UnpackStringPaths(env);

				if (modulePaths == null)
				{
					env = Script.Platform.GetEnvironmentVariable("LUA_PATH");
					if (!string.IsNullOrEmpty(env)) modulePaths = UnpackStringPaths(env);
				}

				if (modulePaths == null)
					modulePaths = UnpackStringPaths("?;?.lua");
			}

			return modulePaths;
		}

	}
}
