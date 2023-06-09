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
	class FieldMemberDescriptorGenerator : AssignableMemberDescriptorGeneratorBase
	{
		public override string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.FieldMemberDescriptor"; }
		}

		protected override CodeExpression GetMemberAccessExpression(CodeExpression thisObj, string name)
		{
			return new CodeFieldReferenceExpression(thisObj, name);
		}

		protected override string GetPrefix()
		{
			return "FLDV";
		}
	}
}
