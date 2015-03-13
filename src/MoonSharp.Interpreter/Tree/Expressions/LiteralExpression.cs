using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class LiteralExpression: Expression
	{
		DynValue m_Value;

		public DynValue Value
		{
			get { return m_Value; }
		}

		private string RemoveHexHeader(string s)
		{
			s = s.ToUpperInvariant();
			if (s.StartsWith("0X"))
				s = s.Substring(2);

			return s;
		}

		public LiteralExpression(ScriptLoadingContext lcontext, DynValue value)
			: base(lcontext)
		{
			m_Value = value;
		}


		public LiteralExpression(ScriptLoadingContext lcontext, Token t)
			: base(lcontext)
		{
			switch (t.Type)
			{
				case TokenType.Number:
					TryParse(t.Text, s => double.Parse(s, CultureInfo.InvariantCulture));
					break;
				case TokenType.Number_Hex:
					TryParse(t.Text, s => (double)ulong.Parse(RemoveHexHeader(s), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
					break;
				case TokenType.Number_HexFloat:
					TryParse(t.Text, s => ParseHexFloat(s));
					break;
				case TokenType.String:
					m_Value = DynValue.NewString(t.Text).AsReadOnly();
					break;
				case TokenType.String_Long:
					m_Value = DynValue.NewString(t.Text).AsReadOnly();
					break;
				case TokenType.True:
					m_Value = DynValue.True;
					break;
				case TokenType.False:
					m_Value = DynValue.False;
					break;
				case TokenType.Nil:
					m_Value = DynValue.Nil;
					break;
				default:
					throw new InternalErrorException("Type mismatch");
			}

			if (m_Value == null)
				throw new SyntaxErrorException("unknown number format near '{0}'", t.Text);
		}

		private void TryParse(string txt, Func<string, double> parser)
		{
			double val = parser(txt);
			m_Value = DynValue.NewNumber(val).AsReadOnly();
		}


		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_Literal(m_Value);
		}

		private double ParseHexFloat(string s)
		{
			throw new SyntaxErrorException("hex floats are not supported: '{0}'", s);
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			return m_Value;
		}
	}
}
