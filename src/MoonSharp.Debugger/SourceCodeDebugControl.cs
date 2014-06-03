using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MoonSharp.Debugger
{
	public partial class SourceCodeDebugControl : UserControl
	{
		private string[] m_SourceCode;
		private bool[] m_BreakPoints;
		int m_Line = 0;
		int m_XOffs = 0;
		int m_ActiveLine = -1;
		int m_CursorLine = 0;

		public int ActiveLine
		{
			get { return m_ActiveLine; }
			set { m_ActiveLine = value; Invalidate(); }
		}

		public int CursorLine
		{
			get { return m_CursorLine; }
			set { m_CursorLine = value; Invalidate(); }
		}

		public SourceCodeDebugControl()
		{
			InitializeComponent();
		}

		public string[] SourceCode
		{
			get { return m_SourceCode; }
			set 
			{
				if (value == null)
				{
					m_SourceCode = null;
					m_BreakPoints = null;
				}
				else
				{
					m_SourceCode = value.Select(s => s.Replace("\t", "    ")).ToArray();
					m_BreakPoints = new bool[m_SourceCode.Length];
				}

				OnSourceCodeChanged(); 
			}
		}

		private void OnSourceCodeChanged()
		{
			m_Line = 0;

			if (vertScroll != null)
			{
				vertScroll.Value = 0;
				vertScroll.Maximum = (m_SourceCode != null) ? m_SourceCode.Length : 10;
				horizScroll.Maximum = 300;
				horizScroll.Value = 0;
				m_CursorLine = 0;
				m_ActiveLine = -1;
			}

			Invalidate();
		}

		protected override bool DoubleBuffered
		{
			get { return true; }
			set { base.DoubleBuffered = true; }
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (m_SourceCode == null)
				return;

			int H = this.Font.Height;
			int W = (int)e.Graphics.MeasureString("X", this.Font).Width;
			int Y = 0;

			for (int i = m_Line; i < m_SourceCode.Length && H < this.Height; i++, Y += H)
			{
				if (m_ActiveLine == i)
				{
					e.Graphics.FillRectangle(Brushes.DarkCyan, 0, Y, Width, H);
				}

				if (m_BreakPoints[i])
				{
					e.Graphics.FillEllipse(Brushes.Red, 5, Y + 5, 10, 10);
					e.Graphics.DrawEllipse(Pens.DarkRed, 5, Y + 5, 10, 10);
//					e.Graphics.FillRectangle(Brushes.DarkRed, 0, Y, Width, H);
				}


				if (m_ActiveLine == i)
				{
					e.Graphics.FillRectangle(Brushes.Aqua, 3, Y + 8, 14, 6);
					e.Graphics.DrawRectangle(Pens.DarkBlue, 3, Y + 8, 14, 6);
				}

				if (i == m_CursorLine)
					e.Graphics.FillRectangle(Brushes.Black, -1, Y, Width + 1, H);

				string str = m_SourceCode[i];

				if (m_XOffs != 0)
				{
					if (m_XOffs >= str.Length)
						continue;
					else
						str = str.Substring(m_XOffs);
				}

				e.Graphics.DrawString(str, this.Font, Brushes.Gainsboro, 20, Y);
			}
		}

		private void vertScroll_Scroll(object sender, ScrollEventArgs e)
		{
			m_Line = Math.Min(m_SourceCode.Length - 1, Math.Max(0, e.NewValue));
			Invalidate();
		}

		private void vertScroll_ValueChanged(object sender, EventArgs e)
		{
			m_Line = Math.Min(m_SourceCode.Length - 1, Math.Max(0, vertScroll.Value));
			Invalidate();
		}

		private void horizScroll_Scroll(object sender, ScrollEventArgs e)
		{
			m_XOffs = horizScroll.Value;
			Invalidate();
		}

		private void horizScroll_ValueChanged(object sender, EventArgs e)
		{
			m_XOffs = horizScroll.Value;
			Invalidate();
		}

		public void SetBreakpoint(int i, bool val)
		{
			m_BreakPoints[i] = val;
		}

		private void SourceCodeDebugControl_MouseClick(object sender, MouseEventArgs e)
		{
			int Y = e.Y / this.Font.Height;

			Y += m_Line;

			m_CursorLine = Y;

			Invalidate();
		}


		private void SourceCodeDebugControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Up)
				m_CursorLine = Math.Max(0, m_CursorLine - 1);
			if (e.KeyCode == Keys.Down)
				m_CursorLine = Math.Min(m_SourceCode.Length - 1, m_CursorLine + 1);
			if (e.KeyCode == Keys.F9)
				m_BreakPoints[m_CursorLine] = !m_BreakPoints[m_CursorLine];
		
		}
	}
}
