using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;

namespace MoonSharp.Hardwire
{
	public class HardwireGenerator
	{
		HardwireCodeGenerationContext m_Context;

		public HardwireGenerator(string namespaceName, string entryClassName, ICodeGenerationLogger logger)
		{
			m_Context = new HardwireCodeGenerationContext(namespaceName, entryClassName, logger);
		}

		public void BuildCodeModel(Table table)
		{
			m_Context.GenerateCode(table);
		}

		public string GenerateSourceCode(string language = "CSharp")
		{
			return GenerateSourceCode(CodeDomProvider.CreateProvider("CSharp"), new CodeGeneratorOptions());
		}

		public string GenerateSourceCode(CodeDomProvider codeDomProvider, CodeGeneratorOptions codeGeneratorOptions)
		{
			using (StringWriter sourceWriter = new StringWriter())
			{
				codeDomProvider.GenerateCodeFromCompileUnit(m_Context.CompileUnit, sourceWriter, codeGeneratorOptions);
				return sourceWriter.ToString();
			}
		}

		public CodeCompileUnit GetCodeDomCompileUnit()
		{
			return m_Context.CompileUnit;
		}
	}
}
