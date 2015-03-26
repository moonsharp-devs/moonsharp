using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Marks a method as the handler of metamethods of a userdata type
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpUserDataMetamethodAttribute : Attribute
	{
		/// <summary>
		/// The interop access mode
		/// </summary>
		public string Name { get; private set; }

		public MoonSharpUserDataMetamethodAttribute(string name)
		{
			Name = name;
		}
	}

}
