using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Internal type used by <seealso cref="AnonWrapper{T}"/> for registration
	/// </summary>
	public class AnonWrapper
	{
	}

	/// <summary>
	/// Wrapper which allows for easier management of userdata without registering a new userdata type - useful 
	/// if a type which is not exposed to scripts but can be managed as a "black box" by scripts is desired.
	/// </summary>
	/// <typeparam name="T">The type to wrap</typeparam>
	public class AnonWrapper<T> : AnonWrapper
	{
		public AnonWrapper()
		{
		}

		public AnonWrapper(T o)
		{
			Value = o;
		}

		public T Value { get; set; }
	}

}
