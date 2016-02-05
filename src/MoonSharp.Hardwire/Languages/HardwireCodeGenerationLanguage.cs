using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Hardwire.Languages
{
	public abstract class HardwireCodeGenerationLanguage
	{
		public static HardwireCodeGenerationLanguage CSharp
		{
			get { return new CSharpHardwireCodeGenerationLanguage(); }
		}

		public static HardwireCodeGenerationLanguage VB
		{
			get { return new VbHardwireCodeGenerationLanguage(); }
		}


		public abstract string Name { get; }

		public abstract CodeDomProvider CodeDomProvider { get; }

		public abstract CodeExpression UnaryPlus(CodeExpression arg);
		public abstract CodeExpression UnaryIncrement(CodeExpression arg);
		public abstract CodeExpression UnaryDecrement(CodeExpression arg);
		public abstract CodeExpression UnaryNegation(CodeExpression arg);
		public abstract CodeExpression UnaryLogicalNot(CodeExpression arg);
		public abstract CodeExpression UnaryOneComplement(CodeExpression arg);
		public abstract CodeExpression BinaryXor(CodeExpression arg1, CodeExpression arg2);
		public abstract CodeExpression CreateMultidimensionalArray(string type, CodeExpression[] args);

		public abstract string[] GetInitialComment();

		
		protected string ExpressionToString(CodeExpression exp)
		{
			using (StringWriter sourceWriter = new StringWriter())
			{
				CodeDomProvider.GenerateCodeFromExpression(exp, sourceWriter, new CodeGeneratorOptions());
				return sourceWriter.ToString();
			}
		}

		protected CodeExpression SnippetExpression(string format, params CodeExpression[] args)
		{
			string fmt = "(" + format + ")";
			string res = string.Format(fmt, args.Select(e => ExpressionToString(e)).OfType<object>().ToArray());
			return new CodeSnippetExpression(res);
		}
	}


}
