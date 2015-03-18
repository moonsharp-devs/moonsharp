using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Debugger
{
	class DoubleBufferedListView : System.Windows.Forms.ListView
	{
		public DoubleBufferedListView()
			: base()
		{
			this.DoubleBuffered = true;
		}
	}
}
