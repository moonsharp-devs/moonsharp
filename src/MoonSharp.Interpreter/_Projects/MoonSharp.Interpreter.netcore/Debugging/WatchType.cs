
namespace MoonSharp.Interpreter.Debugging
{
	/// <summary>
	/// Enumeration of the possible watch types
	/// </summary>
	public enum WatchType
	{
		/// <summary>
		/// A real variable watch
		/// </summary>
		Watches,
		/// <summary>
		/// The status of the v-stack
		/// </summary>
		VStack,
		/// <summary>
		/// The call stack
		/// </summary>
		CallStack,
		/// <summary>
		/// The list of coroutines
		/// </summary>
		Coroutines,

		/// <summary>
		/// The maximum value of this enum
		/// </summary>
		MaxValue
	}
}
