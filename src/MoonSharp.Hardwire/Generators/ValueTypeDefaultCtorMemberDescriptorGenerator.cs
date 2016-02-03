using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;

namespace MoonSharp.Hardwire.Generators
{
	class ValueTypeDefaultCtorMemberDescriptorGenerator : IHardwireGenerator
	{
		public string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.ValueTypeDefaultCtorMemberDescriptor"; }
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerationContext generator, CodeTypeMemberCollection members)
		{
			MethodMemberDescriptorGenerator mgen = new MethodMemberDescriptorGenerator("VTDC");

			Table mt = new Table(null);

			mt["params"] = new Table(null);
			mt["name"] = "__new";
			mt["type"] = table["type"];
			mt["ctor"] = true;
			mt["extension"] = false;
			mt["decltype"] = table["type"];
			mt["ret"] = table["type"];
			mt["special"] = false;


			return mgen.Generate(mt, generator, members);
		}
	}
}
