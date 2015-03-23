using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Platforms;

namespace MoonSharp.Interpreter
{
	public class ScriptGlobalOptions
	{
		internal ScriptGlobalOptions()
		{
			Platform = PlatformAutoDetector.GetDefaultPlatform();
			CustomConverters = new CustomConvertersCollection();
		}

		/// <summary>
		/// Gets or sets the custom converters.
		/// </summary>
		public CustomConvertersCollection CustomConverters { get; set; }

		/// <summary>
		/// Gets or sets the platform abstraction to use.
		/// </summary>
		/// <value>
		/// The current platform abstraction.
		/// </value>
		public IPlatformAccessor Platform { get; set; }


	}
}
