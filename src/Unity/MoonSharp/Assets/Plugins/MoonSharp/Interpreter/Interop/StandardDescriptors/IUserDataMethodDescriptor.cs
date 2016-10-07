using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public interface IUserDataMethodDescriptor : IUserDataMemberDescriptor
	{
		StandardUserDataParameter[] Parameters { get; }
	}
}
