using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop.BasicDescriptors
{
	/// <summary>
	/// Permissions for members access
	/// </summary>
	[Flags]
	public enum MemberDescriptorAccess
	{
		/// <summary>
		/// The member can be read from
		/// </summary>
		CanRead,
		/// <summary>
		/// The member can be written to
		/// </summary>
		CanWrite,
		/// <summary>
		/// The can be invoked
		/// </summary>
		CanExecute
	}







}
