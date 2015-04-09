using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.DataStructs
{
	/// <summary>
	/// Implementation of IEqualityComparer enforcing reference equality
	/// </summary>
	internal class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			return object.ReferenceEquals(x, y);
		}

		int IEqualityComparer<object>.GetHashCode(object obj)
		{
			return obj.GetHashCode();
		}
	}
}
