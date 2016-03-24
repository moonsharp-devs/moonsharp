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
		List<DynamicExpression> m_Watches = new List<DynamicExpression>();

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			m_Ctx = SynchronizationContext.Current;
			Script.WarmUp();
			//Script.DefaultOptions.TailCallOptimizationThreshold = 1;
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
			m_Script = new Script(CoreModules.Basic | CoreModules.Table | CoreModules.TableIterators | CoreModules.Metatables);
			// m_Script.Options.UseLuaErrorLocations = true;
			m_Script.Options.DebugPrint = s => { Console_WriteLine("{0}", s); };

			// ((ScriptLoaderBase)m_Script.Options.ScriptLoader).ModulePaths = ScriptLoaderBase.UnpackStringPaths("Modules/?;Modules/?.lua");

			DynValue fn;

			try
			{
				fn = m_Script.LoadFile(filename, null, filename.Replace(':', '|'));
			}
			catch (Exception ex)
			{
				txtOutput.Text = "";
				Console_WriteLine("{0}", ex.Message);
				return;
			}

			m_Script.AttachDebugger(this);

			Thread m_Debugger = new Thread(() => DebugMain(fn));
			m_Debugger.Name = "MoonSharp Execution Thread";
			m_Debugger.IsBackground = true;
			m_Debugger.Start();
		}



		public void SetSourceCode(SourceCode sourceCode)
		{
		}

		void IDebugger.SetByteCode(string[]  byteCode)
		{
			string[] source = byteCode.Select((s, i) => string.Format("{0:X8}  {1}", i, s)).ToArray();

			m_Ctx.Send(o =>
				{
					codeView.SourceCode = source;
				}, null);
		}

		DebuggerAction m_NextAction;
		AutoResetEvent m_WaitLock = new AutoResetEvent(false);
		AutoResetEvent m_WaitBack = new AutoResetEvent(false);

		DebuggerAction IDebugger.GetAction(int ip, SourceRef sourceCodeRef)
		{
			m_Ctx.Post(o =>
			{
				codeView.ActiveLine = ip;
				RefreshCodeView(sourceCodeRef);
			}, null);

			m_WaitLock.WaitOne();

			DebuggerAction action = m_NextAction;
			m_NextAction = null;

			m_WaitBack.Set();

			return action;
		}

		SourceRef m_PrevRef = null;

		private void RefreshCodeView(SourceRef sourceCodeRef)
		{
			if (sourceCodeRef == m_PrevRef)
				return;

			m_PrevRef = sourceCodeRef;

			if (sourceCodeRef == null)
			{
				txtCodeView.Text = "!! NULL !!";
			}
			else
			{
				SourceCode sc = m_Script.GetSourceCode(sourceCodeRef.SourceIdx);
				//txtCodeView.Text = sc.Lines[sourceCodeRef.FromLine + 1] + "\n" +
				//	sourceCodeRef.ToString();
				txtCodeView.Text = sc.GetCodeSnippet(sourceCodeRef) + "\r\n\r\n" + sourceCodeRef.ToString();
			}
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


		void DebugMain(DynValue fn)
		{
			try
			{
				fn.Function.Call();
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
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.ByteCodeStepIn });
		}

		private void StepOVER()
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.ByteCodeStepOver });
		}

		private void GO()
		{
			DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.Run });
		}


		void IDebugger.Update(WatchType watchType, IEnumerable<WatchItem> items)
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
			IEnumerable<WatchItem> items = (IEnumerable<WatchItem>)o;

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
			IEnumerable<WatchItem> items = (IEnumerable<WatchItem>)o;

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
			IEnumerable<WatchItem> items = (IEnumerable<WatchItem>)o;

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




		List<DynamicExpression> IDebugger.GetWatchItems()
		{
			return m_Watches;
		}

		private void btnAddWatch_Click(object sender, EventArgs e)
		{
			string text = WatchInputDialog.GetNewWatchName();

			if (!string.IsNullOrEmpty(text))
			{
				string[] codeToAdd = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				m_Watches.AddRange(codeToAdd.Select(code => m_Script.CreateDynamicExpression(code)));
				DebugAction(new DebuggerAction() { Action = DebuggerAction.ActionType.Refresh });
			}
		}

		private void btnRemoveWatch_Click(object sender, EventArgs e)
		{
			HashSet<string> itemsToRemove = new HashSet<string>(lvWatches.SelectedItems.OfType<ListViewItem>().Select(lvi => lvi.Text));

			int i = m_Watches.RemoveAll(w => itemsToRemove.Contains(w.ExpressionCode));

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




		void IDebugger.SetSourceCode(SourceCode sourceCode)
		{
			
		}

		bool IDebugger.IsPauseRequested()
		{
			return false;
		}


		public void SignalExecutionEnded()
		{
		}


		public void RefreshBreakpoints(IEnumerable<SourceRef> refs)
		{
		}


		public bool SignalRuntimeException(ScriptRuntimeException ex)
		{
			Console_WriteLine("Error: {0}", ex.DecoratedMessage);
			return true;
		}

		private void btnOpenTest_Click(object sender, EventArgs e)
		{

		}
	}
}
