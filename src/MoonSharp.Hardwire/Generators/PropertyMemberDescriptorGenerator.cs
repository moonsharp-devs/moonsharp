using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;

namespace MoonSharp.Hardwire.Generators
{
	class PropertyMemberDescriptorGenerator : AssignableMemberDescriptorGeneratorBase
	{
		public override string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.PropertyMemberDescriptor"; }
		}

		protected override CodeExpression GetMemberAccessExpression(CodeExpression thisObj, string name)
		{
			return new CodePropertyReferenceExpression(thisObj, name);
		}

		protected override string GetPrefix()
		{
			return "PROP";
		}
	}
}
