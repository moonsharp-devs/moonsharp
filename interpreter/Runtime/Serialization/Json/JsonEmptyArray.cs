using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Serialization.Json
{
	/// <summary>
	/// UserData representing an empty array in a table converted from Json
	/// </summary>
	public sealed class JsonEmptyArray
	{
		public static bool isEmptyArray() { return true; }

		[MoonSharpHidden]
		public static bool IsJsonEmptyArray(DynValue v)
		{
			return v.Type == DataType.UserData &&
				v.UserData.Descriptor != null &&
				v.UserData.Descriptor.Type == typeof(JsonEmptyArray);
		}

		[MoonSharpHidden]
		public static DynValue Create()
		{
			return UserData.CreateStatic<JsonEmptyArray>();
		}
	}
}
