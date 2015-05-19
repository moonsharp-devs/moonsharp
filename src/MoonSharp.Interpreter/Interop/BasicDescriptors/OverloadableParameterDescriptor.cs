using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop.BasicDescriptors
{
	public sealed class ParameterDescriptor
	{
		public readonly string Name;
		public readonly Type Type;
		public readonly bool HasDefaultValue;

		public ParameterDescriptor(string name, Type type, bool hasDefaultValue)
		{
			Name = name;
			Type = type;
			HasDefaultValue = hasDefaultValue;
		}

		public ParameterDescriptor(ParameterInfo pi)
			: this(pi.Name, pi.ParameterType, !(pi.DefaultValue.IsDbNull()))
		{ }


		public override string ToString()
		{
			return string.Format("{0} {1}{2}", Type.Name, Name, HasDefaultValue ? " = ..." : "");
		}



	}
}
