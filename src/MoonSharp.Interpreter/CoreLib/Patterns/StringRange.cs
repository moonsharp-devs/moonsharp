using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.Patterns
{
	public class StringRange
	{
		public int Start;
		public int End;

		public StringRange()
		{
			Start = 0;
			End = 0;
		}

		public StringRange(int start, int end)
		{
			Start = start;
			End = end;
		}

		public static StringRange FromLuaRange(DynValue start, DynValue end, int? defaultEnd = null)
		{
			int i = start.IsNil() ? 1 : (int)start.Number;
			int j = end.IsNil() ? (defaultEnd ?? i) : (int)end.Number;

			return FromLuaRange(i, j);
		}

		public static StringRange FromLuaRange(int start, int end)
		{
			StringRange range = new StringRange();
			range.Start = (start > 0) ? start - 1 : start;
			range.End = (end > 0) ? end - 1 : end;

			return range;
		}

		public void MapToString(String value)
		{
			if (Start < 0)
			{
				Start = value.Length + Start;
			}

			if (Start < 0)
			{
				Start = 0;
			}

			if (End < 0)
			{
				End = value.Length + End;
			}

			if (End >= value.Length)
			{
				End = value.Length - 1;
			}
		}

		public int Length()
		{
			return (End - Start) + 1;
		}
	}
}
