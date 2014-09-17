using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpVisibleAttribute : Attribute
	{
		public bool Visible { get; private set; }

		public MoonSharpVisibleAttribute(bool visible)
		{
			Visible = visible;
		}
	}
}
