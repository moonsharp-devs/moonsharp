using System;

namespace MoonSharp.Interpreter {

	/// <summary>
	/// A flag that controls if/how symbols (method, property, userdata) are fuzzily matched when they do not exist. Flags can be combined for multiple checks.
	/// </summary>
	[Flags]
	public enum FuzzySymbolMatchingBehaviour
	{

		/// <summary>No fuzzy matching is performed.</summary>
		None = 0,

		/// <summary>The first letter of a symbol will be uppercased (to check for common C# naming conventions). For example, testMethod() becomes TestMethod()</summary>
		UpperFirstLeter = 1,

		/// <summary>Underscores in symbols are converted to camelcase. For example, test_method() becomes testMethod()</summary>
		Camelify = 2,

		/// <summary>
		/// Combines both <see cref="UpperFirstLeter"/> and <see cref="Camelify"/>. For example, test_Method_two() becomes TestMethodTwo()
		/// </summary>
		All = UpperFirstLeter | Camelify

	}

}