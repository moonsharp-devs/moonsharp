using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MoonSharp.Debugger
{
	static class Ext_Methods
	{
		public static ListViewItem Add(this ListView lv, params object[] texts)
		{
			ListViewItem lvi = new ListViewItem();
			lvi.Text = texts[0].ToString();

			for (int i = 1; i < texts.Length; i++)
			{
				ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
				lvsi.Text = texts[i].ToString();
				lvi.SubItems.Add(lvsi);
			}

			lv.Items.Add(lvi);

			return lvi;
		}
	}
}
