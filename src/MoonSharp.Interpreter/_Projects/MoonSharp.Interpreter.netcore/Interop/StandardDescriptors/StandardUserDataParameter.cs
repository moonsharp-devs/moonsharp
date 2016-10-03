using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public class StandardUserDataParameter
	{
		public Type ParameterType { get; private set; }
		public bool IsOptional { get; private set; }
		public object DefaultValue { get; private set; }


		public StandardUserDataParameter(ParameterInfo pi)
		{
			ParameterType = pi.ParameterType;
			IsOptional = pi.DefaultValue.IsDbNull();
			DefaultValue = pi.DefaultValue;
		}





	}
}
