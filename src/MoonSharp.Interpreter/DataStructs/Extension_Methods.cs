using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Extension methods used in the whole project.
	/// </summary>
	internal static class Extension_Methods
	{
		/// <summary>
		/// Gets a value from the dictionary or returns the default value
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="dictionary">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue v;

			if (dictionary.TryGetValue(key, out v))
				return v;

			return default(TValue);
		}



	}
}
