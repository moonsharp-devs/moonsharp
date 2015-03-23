using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// In a module type, mark fields with this attribute to have them exposed as a module constant.
	/// 
	/// See <seealso cref="MoonSharpModuleAttribute"/> for more information about modules.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpModuleConstantAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
