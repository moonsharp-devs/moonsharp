using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Interface used by MoonSharp to access objects of a given type from scripts.
	/// </summary>
	public interface IUserDataDescriptor
	{
		/// <summary>
		/// Gets the name of the descriptor (usually, the name of the type described).
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets the type this descriptor refers to
		/// </summary>
		Type Type { get; }
		/// <summary>
		/// Performs an "index" "get" operation.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="index">The index.</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing);
		/// <summary>
		/// Performs an "index" "set" operation.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="index">The index.</param>
		/// <param name="value">The value to be set</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing);
		/// <summary>
		/// Converts this userdata to string
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		string AsString(object obj);
		/// <summary>
		/// Gets the value of an hypothetical metatable for this userdata.
		/// NOT SUPPORTED YET.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="metaname">The name of the metamember.</param>
		/// <returns></returns>
		DynValue MetaIndex(Script script, object obj, string metaname);
	}
}
