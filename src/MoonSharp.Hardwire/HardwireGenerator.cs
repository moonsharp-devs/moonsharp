using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Hardwire.Languages;
using MoonSharp.Interpreter;

namespace MoonSharp.Hardwire
{
	public class HardwireGenerator
	{
		HardwireCodeGenerationContext m_Context;
		HardwireCodeGenerationLanguage m_Language;

		public HardwireGenerator(string namespaceName, string entryClassName, ICodeGenerationLogger logger,
			HardwireCodeGenerationLanguage language = null)
		{
			m_Language = language ?? HardwireCodeGenerationLanguage.CSharp;
			m_Context = new HardwireCodeGenerationContext(namespaceName, entryClassName, logger, language);
		}

		public void BuildCodeModel(Table table)
		{
			m_Context.GenerateCode(table);
		}

		public string GenerateSourceCode()
		{
			var codeDomProvider = m_Language.CodeDomProvider;
			var codeGeneratorOptions = new CodeGeneratorOptions();

			using (StringWriter sourceWriter = new StringWriter())
			{
				codeDomProvider.GenerateCodeFromCompileUnit(m_Context.CompileUnit, sourceWriter, codeGeneratorOptions);
				return sourceWriter.ToString();
			}
		}

		public bool AllowInternals
		{
			get { return m_Context.AllowInternals; }
			set { m_Context.AllowInternals = value; }
		}
	}
}
