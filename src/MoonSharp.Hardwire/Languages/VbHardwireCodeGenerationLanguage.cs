using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Hardwire.Languages
{
	public class VbHardwireCodeGenerationLanguage : HardwireCodeGenerationLanguage
	{
		private CodeDomProvider m_CodeDomProvider;

		public VbHardwireCodeGenerationLanguage()
		{
			m_CodeDomProvider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("VB");
		}

		public override string Name
		{
			get { return "VB.NET"; }
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
			return SnippetExpression("NOT {0}", arg);
		}

		public override CodeExpression UnaryOneComplement(CodeExpression arg)
		{
			return SnippetExpression("NOT {0}", arg);
		}

		public override CodeExpression BinaryXor(CodeExpression arg1, CodeExpression arg2)
		{
			return SnippetExpression("{0} XOR {1}", arg1, arg2);
		}

		public override CodeExpression UnaryIncrement(CodeExpression arg)
		{
			return null;
		}

		public override CodeExpression UnaryDecrement(CodeExpression arg)
		{
			return null;
		}

		public override string[] GetInitialComment()
		{
			return new string[] {
				" *** WARNING *** : VB.NET support is experimental and", "is not officially supported."
			};
		}

		public override CodeExpression CreateMultidimensionalArray(string type, CodeExpression[] args)
		{
			var idxexp = new CodeSnippetExpression(string.Join(", ", args.Select(e => ExpressionToString(e)).ToArray()));

			return new CodeArrayCreateExpression(type, idxexp);
		}
	}
}
