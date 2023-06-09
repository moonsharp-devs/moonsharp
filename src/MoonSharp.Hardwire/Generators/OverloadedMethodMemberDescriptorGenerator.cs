using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Hardwire.Generators
{
	class OverloadedMethodMemberDescriptorGenerator : IHardwireGenerator
	{
		public string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.OverloadedMethodMemberDescriptor"; }
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerationContext generator,
			CodeTypeMemberCollection members)
		{
			List<CodeExpression> initializers = new List<CodeExpression>();

			generator.DispatchTablePairs(table.Get("overloads").Table, members, exp =>
			{
				initializers.Add(exp);
			});

			var name = new CodePrimitiveExpression((table["name"] as string));
			var type = new CodeTypeOfExpression(table["decltype"] as string);

			var array = new CodeArrayCreateExpression(typeof(IOverloadableMemberDescriptor), initializers.ToArray());

			return new CodeExpression[] {
					new CodeObjectCreateExpression(typeof(OverloadedMethodMemberDescriptor), name, type, array)
			};
		}
	}
}
