using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpUserDataAttribute : Attribute
	{
		public InteropAccessMode AccessMode { get; private set; }

		public MoonSharpUserDataAttribute()
		{
			AccessMode = InteropAccessMode.Default;
		}
	}
}
