using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Loaders;

namespace MoonSharp.Interpreter
{
	public class ScriptOptions
	{
		internal ScriptOptions()
		{
			
		}

		internal ScriptOptions(ScriptOptions defaults)
		{
			this.DebugInput = defaults.DebugInput;
			this.DebugPrint = defaults.DebugPrint;
			this.ScriptLoader = defaults.ScriptLoader;
		}


		/// <summary>
		/// Gets or sets the script loader to use. A script loader wraps all code loading from files, so that access
		/// to the filesystem can be completely overridden.
		/// </summary>
		/// <value>
		/// The current script loader.
		/// </value>
		public IScriptLoader ScriptLoader
		{
			get; set;
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

	}
}
