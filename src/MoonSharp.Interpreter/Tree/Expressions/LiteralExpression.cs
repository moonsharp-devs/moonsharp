using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class LiteralExpression : Expression
	{
		RValue m_Value;

		public LiteralExpression(IParseTree context, ScriptLoadingContext lcontext, RValue rvalue)
			: base(context, lcontext)
		{
			m_Value = rvalue.AsReadOnly();
		}


		public LiteralExpression(LuaParser.NumberContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			if (m_Value == null) TryParse(context.FLOAT(), s => double.Parse(s, CultureInfo.InvariantCulture));
			if (m_Value == null) TryParse(context.HEX(), s => (double)ulong.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
			if (m_Value == null) TryParse(context.INT(), s => double.Parse(s, CultureInfo.InvariantCulture));
		}


		public LiteralExpression(LuaParser.StringContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			ITerminalNode charStr = context.CHARSTRING();
			ITerminalNode longStr = context.LONGSTRING();
			ITerminalNode normStr = context.NORMALSTRING();

			if (charStr != null)
				m_Value = new RValue(NormalizeCharStr(charStr.GetText())).AsReadOnly();
			else if (longStr != null)
				m_Value = new RValue(NormalizeLongStr(longStr.GetText())).AsReadOnly();
			else if (normStr != null)
				m_Value = new RValue(NormalizeNormStr(normStr.GetText())).AsReadOnly();
		}

		private string NormalizeNormStr(string str)
		{
			return str.Substring(1, str.Length - 2);
		}

		private string NormalizeLongStr(string str)
		{
			int lenOfPrefix = 0;
			int squareBracketsFound = 0;
			str = str.Trim();

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (c == '[')
					++squareBracketsFound;

				++lenOfPrefix;

				if (squareBracketsFound == 2)
					break;
			}

			str = str.Substring(lenOfPrefix, str.Length - lenOfPrefix * 2);
			return str;
		}

		private string NormalizeCharStr(string str)
		{
			return str.Substring(1, str.Length - 2);
		}

		private void TryParse(ITerminalNode terminalNode, Func<string, double> parser)
		{
			if (terminalNode == null)
				return;

			string txt = terminalNode.GetText();
			double val = parser(txt);
			m_Value = new RValue(val).AsReadOnly();
		}

		public override void Compile(Execution.VM.Chunk bc)
		{
			bc.Literal(m_Value);
		}
	}
}
