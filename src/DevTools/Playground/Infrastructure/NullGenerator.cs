using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;

namespace Playground
{
	public class NullGenerator : IHardwireGenerator
	{
		public NullGenerator()
		{
			ManagedType = "";
		}

		public NullGenerator(string type)
		{
			ManagedType = type;
		}

		public string ManagedType
		{
			get;
			private set;
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerator generator, CodeTypeMemberCollection members)
		{
			generator.Error("WARNING: Generation of '{0}' not supported.", ManagedType);

			return new CodeExpression[0];
		}
	}
}
