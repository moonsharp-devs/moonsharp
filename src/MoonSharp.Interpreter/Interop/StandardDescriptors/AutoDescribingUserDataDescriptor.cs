using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Descriptor which acts as a non-containing adapter from IUserDataType to IUserDataDescriptor
	/// </summary>
	internal class AutoDescribingUserDataDescriptor : IUserDataDescriptor
	{
		private string m_FriendlyName;
		private Type m_Type;

		public AutoDescribingUserDataDescriptor(Type type, string friendlyName)
		{
			m_FriendlyName = friendlyName;
			m_Type = type;
		}

		public string Name
		{
			get { return m_FriendlyName; }
		}

		public Type Type
		{
			get { return m_Type; }
		}

		public DynValue Index(Script script, object obj, DynValue index, bool isNameIndex)
		{
			IUserDataType u = obj as IUserDataType;

			if (u != null)
				return u.Index(script, index, isNameIndex);

			return null;
		}

		public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isNameIndex)
		{
			IUserDataType u = obj as IUserDataType;

			if (u != null)
				return u.SetIndex(script, index, value, isNameIndex);

			return false;
		}

		public string AsString(object obj)
		{
			if (obj != null)
				return obj.ToString();
			else
				return null;
		}

		public DynValue MetaIndex(Script script, object obj, string metaname)
		{
			IUserDataType u = obj as IUserDataType;

			if (u != null)
				return u.MetaIndex(script, metaname);

			return null;
		}
	}
}
