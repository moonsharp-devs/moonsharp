using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop.BasicDescriptors
{
	public interface ISerializableReflectionDescriptor
	{
		void Serialize(Table t);
	}
}
