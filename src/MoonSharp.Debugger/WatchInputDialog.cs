using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MoonSharp.Debugger
{
	public partial class WatchInputDialog : Form
	{
		public WatchInputDialog()
		{
			InitializeComponent();
		}

		public static string GetNewWatchName()
		{
			WatchInputDialog dlg = new WatchInputDialog();
			var res = dlg.ShowDialog();

			if (res == DialogResult.OK)
				return dlg.txtWatch.Text;

			return null;
		}


	}
}
