using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Common interface for all resources which are uniquely bound to a script.
	/// </summary>
	public interface IScriptPrivateResource
	{
		/// <summary>
		/// Gets the script owning this resource.
		/// </summary>
		/// <value>
		/// The script owning this resource.
		/// </value>
		Script OwnerScript { get; }
	}
}
