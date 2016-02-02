using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Hardwire.Utils;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Serialization;

namespace MoonSharp.Hardwire.Generators
{
	public class ArrayMemberDescriptorGenerator : IHardwireGenerator
	{
		public string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.ArrayMemberDescriptor"; }
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerationContext generatorContext, CodeTypeMemberCollection members)
		{
			string className = "AIDX_" + Guid.NewGuid().ToString("N");
			string name = table.Get("name").String;
			bool setter = table.Get("setter").Boolean;

			CodeTypeDeclaration classCode = new CodeTypeDeclaration(className);

			classCode.TypeAttributes = System.Reflection.TypeAttributes.NestedPrivate | System.Reflection.TypeAttributes.Sealed;

			classCode.BaseTypes.Add(typeof(ArrayMemberDescriptor));

			CodeConstructor ctor = new CodeConstructor();
			ctor.Attributes = MemberAttributes.Assembly;
			classCode.Members.Add(ctor);

			ctor.BaseConstructorArgs.Add(new CodePrimitiveExpression(name));
			ctor.BaseConstructorArgs.Add(new CodePrimitiveExpression(setter));
			
			DynValue vparams = table.Get("params");

			if (vparams.Type == DataType.Table)
			{
				List<HardwireParameterDescriptor> paramDescs = HardwireParameterDescriptor.LoadDescriptorsFromTable(vparams.Table);

				ctor.BaseConstructorArgs.Add(new CodeArrayCreateExpression(typeof(ParameterDescriptor), paramDescs.Select(e => e.Expression).ToArray()));
			}

			members.Add(classCode);
			return new CodeExpression[] { new CodeObjectCreateExpression(className) };
		}
	}
}
