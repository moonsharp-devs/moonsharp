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
		public int FromLine { get; private set; }
		public int ToLine { get; private set; }
		public bool IsStepStop { get; private set; }

		public bool Breakpoint;

		internal SourceRef(int sourceIdx, int from, int to, int fromline, int toline, bool isStepStop)
		{
			SourceIdx = sourceIdx;
			FromChar = from;
			ToChar = to;
			FromLine = fromline;
			ToLine = toline;
			IsStepStop = isStepStop;
		}


		public override string ToString()
		{
			return string.Format("[{0}]{1} ({2}, {3}) -> ({4}, {5})",
				SourceIdx, IsStepStop ? "*" : " ",
				FromLine, FromChar,
				ToLine, ToChar);
		}

		public bool IncludesLocation(int sourceIdx, int line, int col)
		{
			if (sourceIdx != SourceIdx || line < FromLine || line > ToLine)
				return false;

			if (FromLine == ToLine)
				return col >= FromChar && col <= ToChar;
			if (line == FromLine)
				return col >= FromChar;
			if (line == ToLine)
				return col <= ToChar;

			return true;
		}


	}
}
