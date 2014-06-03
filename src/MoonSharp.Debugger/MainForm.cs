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
			m_Debugger.Start();
		}

		void IDebugger.SetSourceCode(Chunk byteCode, string[] code)
		{
			codeView.SourceCode = byteCode.Code.Select(s => s.ToString()).ToArray();
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
			m_WaitBack.WaitOne();
		}


		void DebugMain()
		{
			m_Script.Execute(null);
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepIn });
		}
	}
}
