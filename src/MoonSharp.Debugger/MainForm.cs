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
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Loaders;

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
			Script.WarmUp();
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


		DynValue Assert(ScriptExecutionContext executionContext, CallbackArguments values)
		{
			if (!values[0].CastToBool())
				Console_WriteLine("ASSERT FAILED!");

			return DynValue.Nil;
		}

		DynValue XAssert(ScriptExecutionContext executionContext, CallbackArguments values)
		{
			if (!values[1].CastToBool())
				Console_WriteLine("ASSERT FAILED! : {0}", values[0].ToString());

			return DynValue.Nil;
		}

		private void Console_WriteLine(string fmt, params object[] args)
		{
			fmt = string.Format(fmt, args);

			m_Ctx.Post(str =>
			{
				txtOutput.Text = txtOutput.Text + fmt.ToString().Replace("\n", "\r\n") + "\r\n";
				txtOutput.SelectionStart = txtOutput.Text.Length - 1;
				txtOutput.SelectionLength = 0;
				txtOutput.ScrollToCaret();
			}, fmt);
		}


		private void DebugScript(string filename)
		{
			m_Script = new Script(CoreModules.Preset_Complete);

			m_Script.DebugPrint = s => { Console_WriteLine("{0}", s); };
			//m_Script.Globals["assert"] = DynValue.NewCallback(Assert);
			m_Script.Globals.Set("xassert", DynValue.NewCallback(XAssert));

			var L = new ClassicLuaScriptLoader();
			L.ModulePaths = L.UnpackStringPaths("Modules/?;Modules/?.lua");
			m_Script.ScriptLoader = L;

			try
			{
				m_Script.LoadFile(filename);
			}
			catch (Exception ex)
			{
				txtOutput.Text = "";
				Console_WriteLine("{0}", ex.Message);
				return;
			}

			m_Script.AttachDebugger(this);

			Thread m_Debugger = new Thread(DebugMain);
			m_Debugger.Name = "Moon# Execution Thread";
			m_Debugger.IsBackground = true;
			m_Debugger.Start();
		}

		void IDebugger.SetSourceCode(ByteCode byteCode, string[] code)
		{
			string[] source = new string[byteCode.Code.Count];

			for (int i = 0; i < byteCode.Code.Count; i++)
			{
				source[i] = string.Format("{0:X8}  {1}", i, byteCode.Code[i]);
			}

			m_Ctx.Send(o =>
				{
					codeView.SourceCode = source;
				}, null);
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

			m_WaitBack.Set();

			return action;
		}

		void DebugAction(DebuggerAction action)
		{
			bool savedState = timerFollow.Enabled;
			timerFollow.Enabled = false;

			m_NextAction = action;
			m_WaitLock.Set();

			if (!m_WaitBack.WaitOne(1000))
			{
				MessageBox.Show(this, "Operation timed out", "Timeout");
			}
			else
			{
				timerFollow.Enabled = savedState;
			}
		}


		void DebugMain()
		{
			try
			{
				m_Script.Call(m_Script.GetMainChunk());
			}
			catch (ScriptRuntimeException ex)
			{
				timerFollow.Enabled = false;
				Console_WriteLine("Guest raised unhandled CLR exception: {0} -@{3:X8} {2}\n{1}\n", ex.GetType(), ex.ToString(), ex.DecoratedMessage, ex.InstructionPtr);
			}
			catch (Exception ex)
			{
				timerFollow.Enabled = false;
				Console_WriteLine("Guest raised unhandled CLR exception: {0} \n{1}\n", ex.GetType(), ex.ToString());
			}
		}

		private void StepIN()
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepIn });
		}

		private void StepOVER()
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepOver });
		}

		private void GO()
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.Run });
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
				lvVStack.Add(
					item.Address.ToString("X4"),
					(item.Value != null) ? item.Value.Type.ToString() : "(undefined)",
					(item.Value != null) ? item.Value.ToString() : "(undefined)"
					).Tag = item.Value;
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
				lvWatches.Add(
					item.Name ?? "(???)",
					(item.Value != null) ? item.Value.Type.ToLuaTypeString() : "(undefined)",
					(item.Value != null) ? item.Value.ToString() : "(undefined)",
					(item.LValue != null) ? item.LValue.ToString() : "(undefined)"
					).Tag = item.Value;
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
				lvCallStack.Add(
					item.Address.ToString("X8"),
					item.Name ?? ((item.RetAddress < 0) ? "<chunk-root>" : "<??unknown??>"),
					item.RetAddress.ToString("X8"),
					item.BasePtr.ToString("X8")
					).Tag = item.Address;
			}

			lvCallStack.Add("---", "<CLR>", "---", "---");

			lvCallStack.EndUpdate();
		}




		List<string> IDebugger.GetWatchItems()
		{
			return m_Watches;
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
		private void stepInToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StepIN();
		}

		private void btnOpenFile_Click(object sender, EventArgs e)
		{
			openToolStripMenuItem.PerformClick();
		}

		private void stepOverToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StepOVER();
		}

		private void toolGO_Click(object sender, EventArgs e)
		{
			GO();
		}

		private void gOToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GO();
		}
		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			StepIN();
		}

		private void toolStepOver_Click(object sender, EventArgs e)
		{
			StepOVER();
		}

		private void btnViewVStk_Click(object sender, EventArgs e)
		{
			ValueBrowser.StartBrowse(lvVStack.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).Cast<DynValue>().FirstOrDefault());
		}

		private void lvVStack_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ValueBrowser.StartBrowse(lvVStack.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).Cast<DynValue>().FirstOrDefault());
		}

		private void btnViewWatch_Click(object sender, EventArgs e)
		{
			ValueBrowser.StartBrowse(lvWatches.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).Cast<DynValue>().FirstOrDefault());
		}


		private void lvWatches_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ValueBrowser.StartBrowse(lvWatches.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).Cast<DynValue>().FirstOrDefault());
		}

		private void toolGoToCodeVStack_Click(object sender, EventArgs e)
		{
			var v = lvVStack.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).Cast<DynValue>().FirstOrDefault();

			if (v != null && v.Type == DataType.Function)
				GotoBytecode(v.Function.EntryPointByteCodeLocation);
		}

		private void toolGoToCodeWatches_Click(object sender, EventArgs e)
		{
			var v = lvWatches.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).Cast<DynValue>().FirstOrDefault();

			if (v != null && v.Type == DataType.Function)
				GotoBytecode(v.Function.EntryPointByteCodeLocation);
		}
		private void toolGoToCodeXStack_Click(object sender, EventArgs e)
		{
			var v = lvCallStack.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Tag).OfType<int>().FirstOrDefault();

			if (v != 0)
				GotoBytecode(v);
		}

		private void GotoBytecode(int code)
		{
			codeView.CursorLine = code;
		}

		private void timerFollow_Tick(object sender, EventArgs e)
		{
			toolStepIN.PerformClick();
		}

		private void btnFollow_Click(object sender, EventArgs e)
		{
			timerFollow.Start();
		}

		private void btnFastHack_Click(object sender, EventArgs e)
		{
			DebugScript(@"C:\temp\test.lua");
		}



	}
}
