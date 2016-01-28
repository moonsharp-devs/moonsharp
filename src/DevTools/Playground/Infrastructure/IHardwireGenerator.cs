using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Playground
{
	public interface IHardwireGenerator
	{
		string ManagedType { get; }

		CodeExpression[] Generate(Table table, HardwireCodeGenerator generator, 
			CodeTypeMemberCollection members);
	}

}
