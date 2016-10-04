using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.VsCodeDebugger.SDK;

namespace MoonSharp.DebuggerKit
{
	public class AsyncDebugger : IDebugger
	{
		public bool PauseRequested { get; set; }


		List<DynamicExpression> m_Watches = new List<DynamicExpression>();
		HashSet<string> m_WatchesChanging = new HashSet<string>();
		Script m_Script;
		string m_AppName;
		object m_Lock = new object();
		BlockingQueue<DebuggerAction> m_QueuedActions = new BlockingQueue<DebuggerAction>();
		SourceRef m_LastSentSourceRef = null;
		bool m_InGetActionLoop = false;
		bool m_HostBusySent = false;
		string[] m_CachedWatches = new string[(int)WatchType.MaxValue];
		bool m_FreeRunAfterAttach = true;
		Regex m_ErrorRegEx = new Regex(@"\A.*\Z");
		readonly TimeSpan m_TimeOut;
		private IAsyncDebuggerClient m_Client__;

		List<WatchItem>[] m_WatchItems;
		Dictionary<int, SourceCode> m_SourcesMap = new Dictionary<int, SourceCode>();

		public DebugService DebugService { get; private set; }

		public AsyncDebugger(Script script, TimeSpan? timeOut = null)
		{
			m_Script = script;

			m_WatchItems = new List<WatchItem>[(int)WatchType.MaxValue];

			for (int i = 0; i < m_WatchItems.Length; i++)
				m_WatchItems[i] = new List<WatchItem>(64);

			m_TimeOut = timeOut ?? TimeSpan.MaxValue;
		}


		public IAsyncDebuggerClient Client
		{
			get { return m_Client__; }
			set
			{
				var old = m_Client__;
				m_Client__ = value;

				if (value != null)
				{
					for (int i = 0; i < m_Script.SourceCodeCount; i++)
						((IDebugger)this).SetSourceCode(m_Script.GetSourceCode(i));
				}
			}
		}

		DebuggerAction IDebugger.GetAction(int ip, SourceRef sourceref)
		{
			try
			{
				if (m_FreeRunAfterAttach)
				{
					m_FreeRunAfterAttach = false;
					return new DebuggerAction() { Action = DebuggerAction.ActionType.Run };
				}

				m_InGetActionLoop = true;
				PauseRequested = false;

				if (m_HostBusySent)
				{
					m_HostBusySent = false;

					if (Client != null)
						Client.SendHostReady(true);
				}

				if (sourceref != m_LastSentSourceRef)
				{
					if (Client != null)
						Client.SendSourceRef(sourceref);
				}

				while (true)
				{
					DebuggerAction da = m_QueuedActions.Dequeue();

					if (da.Action == DebuggerAction.ActionType.Refresh || da.Action == DebuggerAction.ActionType.HardRefresh)
					{
						lock (m_Lock)
						{
							HashSet<string> existing = new HashSet<string>();

							// remove all not present anymore
							m_Watches.RemoveAll(de => !m_WatchesChanging.Contains(de.ExpressionCode));

							// add all missing
							existing.UnionWith(m_Watches.Select(de => de.ExpressionCode));

							m_Watches.AddRange(m_WatchesChanging
								.Where(code => !existing.Contains(code))
								.Select(code => CreateDynExpr(code)));
						}

						return da;
					}

					if (da.Action == DebuggerAction.ActionType.ToggleBreakpoint 
						|| da.Action == DebuggerAction.ActionType.SetBreakpoint
						|| da.Action == DebuggerAction.ActionType.ClearBreakpoint
						|| da.Action == DebuggerAction.ActionType.ResetBreakpoints)
						return da;

					if (da.Age < m_TimeOut)
						return da;
				}
			}
			finally
			{
				m_InGetActionLoop = false;
			}
		}

		public void QueueAction(DebuggerAction action)
		{
			m_QueuedActions.Enqueue(action);
		}

		private DynamicExpression CreateDynExpr(string code)
		{
			try
			{
				return m_Script.CreateDynamicExpression(code);
			}
			catch (Exception ex)
			{
				return m_Script.CreateConstantDynamicExpression(code, DynValue.NewString(ex.Message));
			}
		}

		List<DynamicExpression> IDebugger.GetWatchItems()
		{
			return new List<DynamicExpression>();
		}

		bool IDebugger.IsPauseRequested()
		{
			return PauseRequested;
		}

		void IDebugger.RefreshBreakpoints(IEnumerable<SourceRef> refs)
		{
			
		}

		void IDebugger.SetByteCode(string[] byteCode)
		{
			
		}

		void IDebugger.SetSourceCode(SourceCode sourceCode)
		{
			m_SourcesMap[sourceCode.SourceID] = sourceCode;

			if (Client != null)
				Client.OnSourceCodeChanged(sourceCode.SourceID);
		}

		void IDebugger.SignalExecutionEnded()
		{
			if (Client != null)
				Client.OnExecutionEnded();
		}

		bool IDebugger.SignalRuntimeException(ScriptRuntimeException ex)
		{
			return false;
		}

		void IDebugger.Update(WatchType watchType, IEnumerable<WatchItem> items)
		{
			var list = m_WatchItems[(int)watchType];

			list.Clear();
			list.AddRange(items);

			if (Client != null)
				Client.OnWatchesUpdated(watchType);
		}


		public List<WatchItem> GetWatches(WatchType watchType)
		{
			return m_WatchItems[(int)watchType];
		}

		public SourceCode GetSource(int id)
		{
			if (m_SourcesMap.ContainsKey(id))
				return m_SourcesMap[id];

			return null;
		}

		public SourceCode FindSourceByName(string path)
		{
			// we use case insensitive match - be damned if you have files which differ only by 
			// case in the same directory on Unix.
			path = path.Replace('\\', '/').ToUpperInvariant();
			return m_SourcesMap.Values.FirstOrDefault(s => s.Name.Replace('\\', '/').ToUpperInvariant() == path);
		}

		void IDebugger.SetDebugService(DebugService debugService)
		{
			DebugService = debugService;
		}
	}
}
