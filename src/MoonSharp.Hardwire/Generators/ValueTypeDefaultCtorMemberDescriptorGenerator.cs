using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
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
			string type = (string)table["$key"];
			string className = "VTDC_" + Guid.NewGuid().ToString("N");

			CodeTypeDeclaration classCode = new CodeTypeDeclaration(className);

			classCode.Comments.Add(new CodeCommentStatement("Descriptor of " + type));


			classCode.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Descriptor of " + type));

			classCode.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, string.Empty));


			classCode.TypeAttributes = System.Reflection.TypeAttributes.NestedPrivate | System.Reflection.TypeAttributes.Sealed;

			classCode.BaseTypes.Add(typeof(HardwiredMethodMemberDescriptor));

			CodeConstructor ctor = new CodeConstructor();
			ctor.Attributes = MemberAttributes.Assembly;
			ctor.BaseConstructorArgs.Add(new CodeTypeOfExpression(type));

			classCode.Members.Add(ctor);

			MethodMemberDescriptorGenerator mgen = new MethodMemberDescriptorGenerator();

			Table mt = new Table(null);

			mt["params"] = new Table(null);
			mt["name"] = "__new";
			mt["type"] = table["type"];
			mt["ctor"] = true;
			mt["extension"] = false;
			mt["decltype"] = table["type"];
			mt["ret"] = table["type"];
			mt["special"] = false;


			var exp = mgen.Generate(mt, generator, classCode.Members)[0];


			ctor.Statements.Add(new CodeMethodInvokeExpression(
				new CodeThisReferenceExpression(), "AddMember", 
				new CodePrimitiveExpression("__new"), exp));


			members.Add(classCode);

			return new CodeExpression[] {
					new CodeObjectCreateExpression(className)
			};
		}
	}
}
