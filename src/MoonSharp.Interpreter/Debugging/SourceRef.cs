using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Debugging
{
	public class SourceRef
	{
		public int SourceIdx { get; private set; }
		public int FromChar { get; private set; }
		public int ToChar { get; private set; }
		public int Line { get; private set; }
		public bool IsStatement { get; private set; }

		internal SourceRef(int sourceIdx, int from, int to, int line, bool isStatement)
		{
			SourceIdx = sourceIdx;
			FromChar = from;
			ToChar = to;
			Line = line;
			IsStatement = isStatement;
		}
	}
}
