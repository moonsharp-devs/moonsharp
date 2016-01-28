using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors
{
	public abstract class HardwiredMemberDescriptor : IMemberDescriptor
	{
		protected HardwiredMemberDescriptor(string name, bool isStatic, MemberDescriptorAccess access)
		{
			IsStatic = isStatic;
			Name = name;
			MemberAccess = access;
		}

		public bool IsStatic { get; private set; }

		public string Name { get; private set; }

		public MemberDescriptorAccess MemberAccess { get; private set; }

		public virtual DynValue GetValue(Script script, object obj)
		{
			throw new InvalidOperationException("GetValue on write-only hardwired descriptor " + Name);
		}

		public void SetValue(Script script, object obj, DynValue value)
		{
			throw new InvalidOperationException("SetValue on read-only hardwired descriptor " + Name);
		}
	}
}
