using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Tree
{
	internal class AntlrErrorListener : BaseErrorListener, IAntlrErrorListener<int>
	{
		string m_Msg = null;
		SourceCode m_Source;


		public AntlrErrorListener(SourceCode source)
		{
			m_Source = source;
		}

		public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
		{
			if (m_Msg == null) m_Msg = "";

			m_Msg += string.Format("{0}[{1},{2}] : Syntax error near '{3} : {4}'\n",
				m_Source.Name, line, charPositionInLine, offendingSymbol, msg);

			m_Msg += UnderlineError(offendingSymbol.StartIndex, offendingSymbol.StopIndex, line, charPositionInLine);
		}

		public string Message { get { return m_Msg; } }

		public override string ToString()
		{
			return m_Msg ?? "(null)";
		}

		public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
		{
			if (m_Msg == null) m_Msg = "";

			m_Msg += string.Format("{0}[{1},{2}] : Syntax error : {3}'\n",
				m_Source.Name, line, charPositionInLine, msg);

			m_Msg += UnderlineError(-1, -1, line, charPositionInLine);
		}

		protected string UnderlineError(int startIndex, int stopIndex, int line, int charPositionInLine)
		{
			string[] lines = m_Source.Lines;
			StringBuilder errorMessage = new StringBuilder();
			errorMessage.AppendLine(lines[line].Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '));

			for (int i = 0; i < charPositionInLine; i++)
			{
				errorMessage.Append(' ');
			}

			if (startIndex >= 0 && stopIndex >= 0)
			{
				for (int i = startIndex; i <= stopIndex; i++)
					errorMessage.Append('^');
			}
			else
			{
				errorMessage.Append("^...");
			}

			errorMessage.AppendLine();
			return errorMessage.ToString();
		}
	}

}
