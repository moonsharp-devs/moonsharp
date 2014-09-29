using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	public class RefIdObject
	{
		private static int s_RefIDCounter = 0;
		private int m_RefID = ++s_RefIDCounter;

		public int ReferenceID { get { return m_RefID; } }


		public string FormatTypeString(DataType t)
		{
			return string.Format("{0}: {1:X8}", t.ToLuaTypeString(), m_RefID);
		}

	}
}
