using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Hardwire.Languages
{
	public class CSharpHardwireCodeGenerationLanguage : HardwireCodeGenerationLanguage
	{
		private CodeDomProvider m_CodeDomProvider;

		public CSharpHardwireCodeGenerationLanguage()
		{
			m_CodeDomProvider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp");
		}

		public override string Name
		{
			get { return "C#"; }
		}

		public override CodeDomProvider CodeDomProvider
		{
			get { return m_CodeDomProvider; }
		}

		public override CodeExpression UnaryPlus(CodeExpression arg)
		{
			return SnippetExpression("+{0}", arg);
		}

		public override CodeExpression UnaryNegation(CodeExpression arg)
		{
			return SnippetExpression("-{0}", arg);
		}

		public override CodeExpression UnaryLogicalNot(CodeExpression arg)
		{
			return SnippetExpression("!{0}", arg);
		}

		public override CodeExpression UnaryOneComplement(CodeExpression arg)
		{
			return SnippetExpression("~{0}", arg);
		}

		public override CodeExpression BinaryXor(CodeExpression arg1, CodeExpression arg2)
		{
			return SnippetExpression("{0} ^ {1}", arg1, arg2);
		}

		public override CodeExpression UnaryIncrement(CodeExpression arg)
		{
			return SnippetExpression("++{0}", arg);
		}

		public override CodeExpression UnaryDecrement(CodeExpression arg)
		{
			return SnippetExpression("--{0}", arg);
		}

		public override string[] GetInitialComment()
		{
			return null;
		}

		public override CodeExpression CreateMultidimensionalArray(string type, CodeExpression[] args)
		{
			var idxexp = new CodeSnippetExpression(string.Join(", ", args.Select(e => ExpressionToString(e)).ToArray()));

			return new CodeArrayCreateExpression(type, idxexp);
		}
	}
}
