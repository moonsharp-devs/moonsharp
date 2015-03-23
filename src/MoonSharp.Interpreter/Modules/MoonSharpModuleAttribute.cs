using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Marks a CLR type to be a MoonSharp module.
	/// Modules are the fastest way to bring interop between scripts and CLR code, albeit at the cost of a very increased
	/// complexity in writing them. Modules is what's used for the standard library, for maximum efficiency.
	/// 
	/// Modules are basically classes containing only static methods, with the callback function signature.
	/// 
	/// See <seealso cref="Table"/> and <seealso cref="ModuleRegister"/> for (extension) methods used to register modules to a 
	/// table.
	/// 
	/// See <seealso cref="CallbackFunction"/> for information regarding the standard callback signature along with easier ways
	/// to marshal methods.
	/// 
	/// See <seealso cref="UserData"/> for easier object marshalling.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MoonSharpModuleAttribute : Attribute
	{
		public string Namespace { get; set; }
	}
}
