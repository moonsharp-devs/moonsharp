using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MoonSharp.RemoteDebugger.Network
{
	static class XmlWriter_Extensions
	{
		private class RaiiExecutor : IDisposable
		{
			Action m_Action;

			public RaiiExecutor(Action a)
			{
				m_Action = a;
			}

			public void Dispose()
			{
				m_Action();
			}
		}

		public static IDisposable Element(this XmlWriter xw, string name)
		{
			xw.WriteStartElement(name);
			return new RaiiExecutor(() => xw.WriteEndElement());
		}

		public static XmlWriter Attribute(this XmlWriter xw, string name, string val)
		{
			if (val == null) val = "(null)";
			xw.WriteAttributeString(name, val);
			return xw;
		}

		public static XmlWriter Attribute(this XmlWriter xw, string name, object val)
		{
			if (val == null) val = "(null)";
			xw.WriteAttributeString(name, val.ToString());
			return xw;
		}

		public static XmlWriter Element(this XmlWriter xw, string name, string val)
		{
			if (val == null) val = "(null)";
			xw.WriteElementString(name, val);
			return xw;
		}

		public static XmlWriter ElementCData(this XmlWriter xw, string name, string val)
		{
			if (val == null) val = "(null)";

			xw.WriteStartElement(name);
			xw.WriteCData(val);
			xw.WriteEndElement();
			return xw;
		}

		public static XmlWriter Comment(this XmlWriter xw, object text)
		{
			if (text == null) return xw;
			xw.WriteComment(text.ToString());
			return xw;
		}

		public static XmlWriter Attribute(this XmlWriter xw, string name, string format, params object[] args)
		{
			xw.WriteAttributeString(name, string.Format(format, args));
			return xw;
		}

		public static XmlWriter Element(this XmlWriter xw, string name, string format, params object[] args)
		{
			xw.WriteElementString(name, string.Format(format, args));
			return xw;
		}

		public static XmlWriter ElementCData(this XmlWriter xw, string name, string format, params object[] args)
		{
			xw.WriteStartElement(name);
			xw.WriteCData(string.Format(format, args));
			xw.WriteEndElement();
			return xw;
		}

		public static XmlWriter Comment(this XmlWriter xw, string format, params object[] args)
		{
			xw.WriteComment(string.Format(format, args));
			return xw;
		}


	}
}
