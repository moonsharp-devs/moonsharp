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
		public bool CannotBreakpoint { get; private set; }

		public string Type { get; set; }

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

		public int GetLocationDistance(int sourceIdx, int line, int col)
		{
			const int PER_LINE_FACTOR = 1600; // we avoid computing real lines length and approximate with heuristics..

			if (sourceIdx != SourceIdx)
				return int.MaxValue;

			if (FromLine == ToLine)
			{
				if (line == FromLine)
				{
					if (col >= FromChar && col <= ToChar)
						return 0;
					else if (col < FromChar)
						return FromChar - col;
					else
						return col - ToChar;
				}
				else
				{
					return Math.Abs(line - FromLine) * PER_LINE_FACTOR;
				}
			}
			else if (line == FromLine)
			{
				if (col < FromChar)
					return FromChar - col;
				else
					return 0;
			}
			else if (line == ToLine)
			{
				if (col > ToChar)
					return col - ToChar;
				else
					return 0;
			}
			else if (line > FromLine && line < ToLine)
			{
				return 0;
			}
			else if (line < FromLine)
			{
				return (FromLine - line) * PER_LINE_FACTOR;
			}
			else
			{
				return (line - ToLine) * PER_LINE_FACTOR;
			}
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




		public SourceRef SetNoBreakPoint()
		{
			CannotBreakpoint = true;
			return this;
		}
	}
}
