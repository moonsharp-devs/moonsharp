using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	internal class CompositeUserDataDescriptor : IUserDataDescriptor
	{
		private List<IUserDataDescriptor> m_Descriptors;
		private Type m_Type;

		public CompositeUserDataDescriptor(List<IUserDataDescriptor> descriptors, Type type)
		{
			m_Descriptors = descriptors;
			m_Type = type;
		}

		public string Name
		{
			get { return "^" + m_Type.FullName; }
		}

		public Type Type
		{
			get { return m_Type; }
		}

		public DynValue Index(Script script, object obj, DynValue index, bool isNameIndex)
		{
			foreach (IUserDataDescriptor dd in m_Descriptors)
			{
				DynValue v = dd.Index(script, obj, index, isNameIndex);

				if (v != null)
					return v;
			}
			return null;
		}

		public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isNameIndex)
		{
			foreach (IUserDataDescriptor dd in m_Descriptors)
			{
				if (dd.SetIndex(script, obj, index, value, isNameIndex))
					return true;
			}
			return false;
		}

		public string AsString(object obj)
		{
			return (obj != null) ? obj.ToString() : null;
		}


		public DynValue MetaIndex(Script script, object obj, string metaname)
		{
			foreach (IUserDataDescriptor dd in m_Descriptors)
			{
				DynValue v = dd.MetaIndex(script, obj, metaname);

				if (v != null)
					return v;
			}
			return null;
		}
	}
}
