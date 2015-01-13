using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Enumerations of the possible policies to handle UserData type registrations
	/// See also : <seealso cref="UserData"/> .
	/// </summary>
	public enum InteropRegistrationPolicy
	{
		/// <summary>
		/// Types must be explicitly registered. If a base type or interface is registered, that is used.
		/// </summary>
		Explicit,
		/// <summary>
		/// Types are automatically registered if not found in the registry. This is easier to use but potentially unsafe.
		/// </summary>
		Automatic,
	}
}
