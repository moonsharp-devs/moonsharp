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


		public string FormatTypeString(string typeString)
		{
			return string.Format("{0}: {1:X8}", typeString, m_RefID);
		}

	}
}
