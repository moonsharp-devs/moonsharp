using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.DataTypes
{
	public interface IScriptPrivateResource
	{
		Script OwnerScript { get; }
	}
}
