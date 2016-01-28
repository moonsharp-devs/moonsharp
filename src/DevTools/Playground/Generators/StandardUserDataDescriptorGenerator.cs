using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;

namespace Playground.Generators
{
	public class StandardUserDataDescriptorGenerator : IHardwireGenerator
	{
		public string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.StandardUserDataDescriptor"; }
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerator generator,
			CodeTypeMemberCollection members)
		{
			string type = (string)table["$key"];
			string className = "HardwiredDescriptor_" + type.Replace('.', '_');

			CodeTypeDeclaration classCode = new CodeTypeDeclaration(className);

			classCode.TypeAttributes = System.Reflection.TypeAttributes.NestedPrivate | System.Reflection.TypeAttributes.Sealed;

			classCode.BaseTypes.Add(typeof(HardwiredUserDataDescriptor));

			CodeConstructor ctor = new CodeConstructor();
			ctor.Attributes = MemberAttributes.Assembly;
			ctor.BaseConstructorArgs.Add(new CodeTypeOfExpression(type));

			classCode.Members.Add(ctor);

			generator.DispatchTablePairs(table.Get("members").Table,
				classCode.Members, exp =>
				{
					ctor.Statements.Add(new CodeMethodInvokeExpression(
						new CodeThisReferenceExpression(), "AddMember", exp));
				});

			generator.DispatchTablePairs(table.Get("metamembers").Table,
				classCode.Members, exp =>
				{
					ctor.Statements.Add(new CodeMethodInvokeExpression(
						new CodeThisReferenceExpression(), "AddMetaMember", exp));
				});

			members.Add(classCode);

			return new CodeExpression[] {
					new CodeObjectCreateExpression(className)
			};
		}



	}
}
