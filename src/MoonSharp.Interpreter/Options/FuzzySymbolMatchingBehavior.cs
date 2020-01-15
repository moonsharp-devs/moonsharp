using System;

namespace MoonSharp.Interpreter {

	/// <summary>
	/// A flag that controls if/how symbols (method, property, userdata) are fuzzily matched when they do not exist. Flags can be combined for multiple checks.
	/// </summary>
	[Flags]
	public enum FuzzySymbolMatchingBehavior {

		/// <summary>No fuzzy matching is performed.</summary>
		None = 0,

		/// <summary>The first letter of a symbol will be uppercased (to check for common C# naming conventions). For example, testMethod() becomes TestMethod()</summary>
		UpperFirstLetter = 1,

		/// <summary>Underscores in symbols are converted to camelcase. For example, test_method() becomes testMethod()</summary>
		Camelify = 2,

		/// <summary>
		/// Converts a symbol to pascal case. For example, test_Method_two() becomes TestMethodTwo()
		/// </summary>
		PascalCase = 4

	}

}
