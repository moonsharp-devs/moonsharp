using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Loaders;
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

			this.ScriptLoader = defaults.ScriptLoader;

			this.CheckThreadAccess = defaults.CheckThreadAccess;
		}

		/// <summary>
		/// Gets or sets the current script-loader.
		/// </summary>
		public IScriptLoader ScriptLoader { get; set; }

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

	}
}
