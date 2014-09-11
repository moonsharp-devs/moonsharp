using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public class UserDataMethodDescriptor
	{
		public MethodInfo MethodInfo { get; private set; }
		public UserDataDescriptor UserDataDescriptor { get; private set; }
		public bool IsStatic { get; private set; }
		public string Name { get; private set; }

		internal UserDataMethodDescriptor(MethodInfo mi, UserDataDescriptor userDataDescriptor)
		{
			this.MethodInfo = mi;
			this.UserDataDescriptor = userDataDescriptor;
			this.Name = mi.Name;
			this.IsStatic = mi.IsStatic;
		}


		internal Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(object obj)
		{
			throw new NotImplementedException();
		}
	}
}
