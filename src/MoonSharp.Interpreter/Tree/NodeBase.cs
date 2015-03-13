using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree
{
	abstract class NodeBase
	{
		public NodeBase(ScriptLoadingContext lcontext)
		{ 
		
		}

		protected ScriptLoadingContext LoadingContext { get; private set; }

		public Exception SyntaxError(string format, params object[] args)
		{
			return new SyntaxErrorException(format, args);
		}

		public void SyntaxAssert(bool condition, string format, params object[] args)
		{
			if (!condition)
				throw  SyntaxError(format, args);
		}

		public abstract void Compile(ByteCode bc);

		//protected SourceRef BuildSourceRef(IToken token, ITerminalNode terminalNode)
		//{
		//	return RegisterSourceRef(new SourceRef(LoadingContext.Source.SourceID, token.Column, token.Column + terminalNode.GetText().Length, token.Line, token.Line, true));
		//}

		//protected SourceRef BuildSourceRef(IToken token1, IToken token2 = null)
		//{
		//	token2 = token2 ?? token1;
		//	return RegisterSourceRef(new SourceRef(LoadingContext.Source.SourceID, token1.Column, token2.Column + token2.Text.Length, token1.Line, token2.Line, true));
		//}

		//protected SourceRef BuildSourceRef(ITerminalNode terminalNode)
		//{
		//	return BuildSourceRef(terminalNode.Symbol, terminalNode);
		//}

		private SourceRef RegisterSourceRef(SourceRef sourceRef)
		{
			LoadingContext.Source.Refs.Add(sourceRef);
			sourceRef.Type = this.GetType().Name;
			return sourceRef;
		}

		protected static Token CheckTokenType(ScriptLoadingContext lcontext, TokenType tokenType)
		{
			Token t = lcontext.Lexer.Current;
			if (t.Type != tokenType)
				throw new SyntaxErrorException("Unexpected token '{0}'", t.Text);

			lcontext.Lexer.Next();

			return t;
		}

		protected static Token CheckTokenType(ScriptLoadingContext lcontext, TokenType tokenType1, TokenType tokenType2)
		{
			Token t = lcontext.Lexer.Current;
			if (t.Type != tokenType1 && t.Type != tokenType2)
				throw new SyntaxErrorException("Unexpected token '{0}'", t.Text);

			lcontext.Lexer.Next();

			return t;
		}
		protected static Token CheckTokenType(ScriptLoadingContext lcontext, TokenType tokenType1, TokenType tokenType2, TokenType tokenType3)
		{
			Token t = lcontext.Lexer.Current;
			if (t.Type != tokenType1 && t.Type != tokenType2 && t.Type != tokenType3)
				throw new SyntaxErrorException("Unexpected token '{0}'", t.Text);

			lcontext.Lexer.Next();

			return t;
		}

		protected static void CheckTokenTypeNotNext(ScriptLoadingContext lcontext, TokenType tokenType)
		{
			Token t = lcontext.Lexer.Current;
			if (t.Type != tokenType)
				throw new SyntaxErrorException("Unexpected token '{0}'", t.Text);
		}

		protected static void CheckMatch(ScriptLoadingContext lcontext, string tokenDesc, TokenType tokenType)
		{
			if (lcontext.Lexer.Current.Type != tokenType)
				throw new SyntaxErrorException("Mismatched '{0}' near '{1}'", tokenDesc, lcontext.Lexer.Current.Text);

			lcontext.Lexer.Next();
		}
	}
}
