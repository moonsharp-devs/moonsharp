using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;

namespace MoonSharpTests
{
	public class HardwireAndLogPolicy : IRegistrationPolicy
	{
		public IUserDataDescriptor HandleRegistration(IUserDataDescriptor newDescriptor, IUserDataDescriptor oldDescriptor)
		{
			//if (oldDescriptor != null && oldDescriptor.Type.IsGenericType)
			//	return newDescriptor;

			if (oldDescriptor == null && newDescriptor != null)
			{
				var backupColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("Registering type {0} with {1}", newDescriptor.Type.Name, newDescriptor.GetType().Name);
				Console.ForegroundColor = backupColor;
				return newDescriptor;
			}

			return oldDescriptor;
		}

		public bool AllowTypeAutoRegistration(Type type)
		{
			return false;
		}

	}
}
