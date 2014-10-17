using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Debugging
{
	public class SourceCode : IScriptPrivateResource
	{
		public string Name { get; private set; }
		public string Code { get; private set; }
		public string[] Lines { get; private set; }
		public Script OwnerScript { get; private set; }
		public int SourceID { get; private set; }

		internal SourceCode(string name, string code, int sourceID, Script ownerScript)
		{
			List<string> lines = new List<string>();

			Name = name;
			Code = code;

			lines.Add(string.Format("-- Begin of chunk : {0} ", name));

			lines.AddRange(Code.Split('\n'));

			Lines = lines.ToArray();

			OwnerScript = ownerScript;
			SourceID = sourceID;
		}

		public string GetCodeSnippet(SourceRef sourceCodeRef)
		{
			if (sourceCodeRef.FromLine == sourceCodeRef.ToLine)
			{
				int from = AdjustStrIndex(Lines[sourceCodeRef.FromLine], sourceCodeRef.FromChar);
				int to = AdjustStrIndex(Lines[sourceCodeRef.FromLine], sourceCodeRef.ToChar);
				return Lines[sourceCodeRef.FromLine].Substring(from, to - from);
			}

			StringBuilder sb = new StringBuilder();

			for (int i = sourceCodeRef.FromLine; i <= sourceCodeRef.ToLine; i++)
			{
				if (i == sourceCodeRef.FromLine)
				{
					int from = AdjustStrIndex(Lines[i], sourceCodeRef.FromChar);
					sb.Append(Lines[i].Substring(from));
				}
				else if (i == sourceCodeRef.ToLine)
				{
					int to = AdjustStrIndex(Lines[i], sourceCodeRef.ToChar);
					sb.Append(Lines[i].Substring(0, to + 1));
				}
				else
				{
					sb.Append(Lines[i]);
				}
			}

			return sb.ToString();
		}

		private int AdjustStrIndex(string str, int loc)
		{
			return Math.Max(Math.Min(str.Length, loc), 0);
		}
	}
}
