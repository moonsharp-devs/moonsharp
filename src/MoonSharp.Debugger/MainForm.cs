using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Debugger
{
	public partial class MainForm : Form, IDebugger
	{
		List<string> m_Watches = new List<string>();

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			m_Ctx = SynchronizationContext.Current;
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Load script";
			ofd.DefaultExt = "lua";
			ofd.Filter = "Lua files (*.lua)|*.lua|All files (*.*)|*.*";

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				DebugScript(ofd.FileName);
				openToolStripMenuItem.Enabled = false;
			}
		}

		Script m_Script;
		SynchronizationContext m_Ctx;

		private void DebugScript(string filename)
		{
			m_Script = MoonSharpInterpreter.LoadFromFile(filename);
			m_Script.AttachDebugger(this);

			Thread m_Debugger = new Thread(DebugMain);
			m_Debugger.Name = "Moon# Execution Thread";
			m_Debugger.IsBackground = true;
			m_Debugger.Start();

		}

		void IDebugger.SetSourceCode(Chunk byteCode, string[] code)
		{
			string[] source = new string[byteCode.Code.Count];

			for (int i = 0; i < byteCode.Code.Count; i++)
			{
				source[i] = string.Format("{0:X8}  {1}", i, byteCode.Code[i]);
			}

			codeView.SourceCode = source;
		}

		DebuggerAction m_NextAction;
		AutoResetEvent m_WaitLock = new AutoResetEvent(false);
		AutoResetEvent m_WaitBack = new AutoResetEvent(false);

		DebuggerAction IDebugger.GetAction(int ip)
		{
			m_Ctx.Post(o =>
			{
				codeView.ActiveLine = ip;
			}, null);

			m_WaitLock.WaitOne();

			DebuggerAction action = m_NextAction;
			m_NextAction = null;

			Debug.WriteLine("Sending " + action.ToString());

			m_WaitBack.Set();

			return action;
		}

		void DebugAction(DebuggerAction action)
		{
			m_NextAction = action;
			m_WaitLock.Set();

			if (!m_WaitBack.WaitOne(1000))
				MessageBox.Show(this, "Operation timed out", "Timeout");
		}


		void DebugMain()
		{
			m_Script.Execute(null);
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			StepIN();
		}

		private void StepIN()
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepIn });
		}


		void IDebugger.Update(WatchType watchType, List<WatchItem> items)
		{
			if (watchType == WatchType.CallStack)
				m_Ctx.Post(UpdateCallStack, items);
			if (watchType == WatchType.Watches)
				m_Ctx.Post(UpdateWatches, items);
			if (watchType == WatchType.VStack)
				m_Ctx.Post(UpdateVStack, items);
		}
		void UpdateVStack(object o)
		{
			List<WatchItem> items = (List<WatchItem>)o;

			lvVStack.BeginUpdate();
			lvVStack.Items.Clear();

			foreach (var item in items)
			{
				var lvi = BuildListViewItem(
					item.Address.ToString("X4"),
					(item.Value != null) ? item.Value.Type.ToString() : "(undefined)",
					(item.Value != null) ? item.Value.ToString() : "(undefined)"
					);
				lvVStack.Items.Add(lvi);
			}

			lvVStack.EndUpdate();

		}


		void UpdateWatches(object o)
		{
			List<WatchItem> items = (List<WatchItem>)o;

			lvWatches.BeginUpdate();
			lvWatches.Items.Clear();

			foreach (var item in items)
			{
				var lvi = BuildListViewItem(
					item.Name ?? "(???)",
					(item.Value != null) ? item.Value.Type.ToLuaTypeString() : "(undefined)",
					(item.Value != null) ? item.Value.ToString() : "(undefined)",
					(item.LValue != null) ? item.LValue.ToString() : "(undefined)"
					);
				lvWatches.Items.Add(lvi);
			}

			lvWatches.EndUpdate();

		}

		void UpdateCallStack(object o)
		{
			List<WatchItem> items = (List<WatchItem>)o;

			lvCallStack.BeginUpdate();
			lvCallStack.Items.Clear();
			foreach (var item in items)
			{
				var lvi = BuildListViewItem(
					item.Address.ToString("X8"),
					item.Name ?? "(???)",
					item.RetAddress.ToString("X8"),
					item.BasePtr.ToString("X8")
					);
				lvCallStack.Items.Add(lvi);
			}

			lvCallStack.Items.Add(BuildListViewItem("---", "(main)", "---", "---"));

			lvCallStack.EndUpdate();
		}

		private ListViewItem BuildListViewItem(params string[] texts)
		{
			ListViewItem lvi = new ListViewItem();
			lvi.Text = texts[0];

			for (int i = 1; i < texts.Length; i++)
			{
				ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
				lvsi.Text = texts[i];
				lvi.SubItems.Add(lvsi);
			}

			return lvi;
		}


		List<string> IDebugger.GetWatchItems()
		{
			return m_Watches;
		}

		private void stepInToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StepIN();
		}

		private void btnAddWatch_Click(object sender, EventArgs e)
		{
			string text = WatchInputDialog.GetNewWatchName();

			if (!string.IsNullOrEmpty(text))
			{
				m_Watches.AddRange(text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
				DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.Refresh });
			}
		}

		private void btnRemoveWatch_Click(object sender, EventArgs e)
		{
			HashSet<string> itemsToRemove = new HashSet<string>(lvWatches.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Text));

			int i = m_Watches.RemoveAll(w => itemsToRemove.Contains(w));

			if (i != 0)
				DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.Refresh });
		}
	}
}
