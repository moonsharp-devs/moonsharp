using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Flags]
	public enum UserDataAccessMode
	{
		/// <summary>
		/// Optimization is not performed and reflection is used everytime to access members.
		/// This is the slowest approach but saves a lot of memory if members are seldomly used.
		/// </summary>
		Reflection,
		/// <summary>
		/// Optimization is done on the fly the first time a member is accessed.
		/// </summary>
		LazyOptimized,
		/// <summary>
		/// Optimization is done at registration time
		/// </summary>
		Preoptimized,
		/// <summary>
		/// Optimization is done in a background thread which starts at registration time. 
		/// If a member is accessed before optimization is completed, reflection is used.
		/// </summary>
		BackgroundOptimized,
		/// <summary>
		/// No optimization is done, and members are not accessible at all.
		/// </summary>
		HideMembers,
		/// <summary>
		/// Use the default access mode set in the registry
		/// </summary>
		Default
	}
}
