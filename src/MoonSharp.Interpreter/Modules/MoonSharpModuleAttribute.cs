using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpModuleAttribute : Attribute
	{
		public string Namespace { get; set; }
	}
}
