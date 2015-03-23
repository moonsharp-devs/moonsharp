using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Marks a type of automatic registration as userdata (which happens only if UserData.RegisterAssembly is called).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpUserDataAttribute : Attribute
	{
		/// <summary>
		/// The interop access mode
		/// </summary>
		public InteropAccessMode AccessMode { get; set; }

		public MoonSharpUserDataAttribute()
		{
			AccessMode = InteropAccessMode.Default;
		}
	}
}
