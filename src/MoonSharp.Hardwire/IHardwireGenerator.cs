using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace MoonSharp.Hardwire
{
	/// <summary>
	/// Interface to be implemented by all hardwire code generators
	/// </summary>
	public interface IHardwireGenerator
	{
		/// <summary>
		/// Gets the type which is managed by this generator. Should be an exact match with the 'class' entry in the 
		/// codegen table.
		/// </summary>
		string ManagedType { get; }

		/// <summary>
		/// Generates code from a dump of the type. 
		/// </summary>
		/// <param name="table">The table containing the data to be parsed.</param>
		/// <param name="generatorContext">The generator context.</param>
		/// <param name="members">The CodeTypeMemberCollection which can be used to add newly defined types.</param>
		/// <returns>Zero or more expressions which can be used by the parent generator to use the generated code.</returns>
		CodeExpression[] Generate(Table table, HardwireCodeGenerationContext generatorContext, CodeTypeMemberCollection members);
	}

}
