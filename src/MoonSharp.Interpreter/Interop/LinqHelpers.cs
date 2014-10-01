using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// LINQ helper methods
	/// </summary>
	public static class LinqHelpers
	{
		public static IEnumerable<T> Convert<T>(this IEnumerable<DynValue> enumerable, DataType type)
		{
			return enumerable.Where(v => v.Type == type).Select(v => v.ToObject<T>());
		}

		public static IEnumerable<DynValue> OfDataType(this IEnumerable<DynValue> enumerable, DataType type)
		{
			return enumerable.Where(v => v.Type == type);
		}

		public static IEnumerable<object> AsObjects(this IEnumerable<DynValue> enumerable)
		{
			return enumerable.Select(v => v.ToObject());
		}

		public static IEnumerable<T> AsObjects<T>(this IEnumerable<DynValue> enumerable)
		{
			return enumerable.Select(v => v.ToObject<T>());
		}

	}
}
