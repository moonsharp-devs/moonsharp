using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	internal static class List_ExtensionMethods
	{

		public static void CropAtCount<T>(this List<T> list, int count)
		{
			list.RemoveRange(count - 1, list.Count - count);
		}

		public static T Peek<T>(this List<T> list, int idxofs = 0)
		{
			T item = list[list.Count - 1 - idxofs];
			return item;
		}
		public static void RemoveLast<T>(this List<T> list, int cnt = 1)
		{
			if (cnt == 1)
				list.RemoveAt(list.Count - 1);
			else
				for (int i = 0; i < cnt; i++)
					list.RemoveAt(list.Count - 1);
		}
		public static T Pop<T>(this List<T> list)
		{
			T item = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return item;
		}
		public static T Push<T>(this List<T> list, T item)
		{
			list.Add(item);
			return item;
		}



	}
}
