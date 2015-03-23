using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// As a convenience, every type deriving from IUserDataType is "self-described". That is, no descriptor is needed/generated
	/// and the object itself is used to describe the type for interop. See also <seealso cref="UserData"/>, <seealso cref="IUserDataDescriptor"/> 
	/// and <seealso cref="StandardUserDataDescriptor"/> .
	/// </summary>
	public interface IUserDataType
	{
		/// <summary>
		/// Performs an "index" "get" operation.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="index">The index.</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		DynValue Index(Script script, DynValue index, bool isDirectIndexing);
		/// <summary>
		/// Performs an "index" "set" operation.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="index">The index.</param>
		/// <param name="value">The value to be set</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing);
		/// <summary>
		/// Gets the value of an hypothetical metatable for this userdata.
		/// NOT SUPPORTED YET.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="metaname">The name of the metamember.</param>
		/// <returns></returns>
		DynValue MetaIndex(Script script, string metaname);
	}
}
