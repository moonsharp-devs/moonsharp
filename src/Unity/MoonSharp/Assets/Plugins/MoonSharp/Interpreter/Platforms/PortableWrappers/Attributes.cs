#if PCL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reflection
{
	internal class ComVisibleAttribute : Attribute
	{
		public ComVisibleAttribute(bool dummy)
		{ }
	}
	internal class GuidAttribute : Attribute
	{
		public GuidAttribute(string dummy)
		{ }
	}
}

namespace System
{
	internal class SerializableAttribute : Attribute
	{
		public SerializableAttribute()
		{ }
	}
}

#endif
