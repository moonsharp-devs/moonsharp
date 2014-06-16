using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpMethodAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
