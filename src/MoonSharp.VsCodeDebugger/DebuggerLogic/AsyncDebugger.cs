#if (!UNITY_5) || UNITY_STANDALONE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.VsCodeDebugger.DebuggerLogic
{
	internal class AsyncDebugger : IDebugger
	{
		private static volatile int s_AsyncDebuggerIdCounter = 0;

		readonly object m_ClientLock = new object();
		IAsyncDebuggerClient m_Client;

		readonly object m_ActionQueueLock = new object();
		readonly Queue<DebuggerAction> m_ActionQueue = new Queue<DebuggerAction>();

		int m_PrevInstructionPtr = -1;

		List<WatchItem>[] m_WatchItems;
		Dictionary<int, SourceCode> m_SourcesMap = new Dictionary<int, SourceCode>();
		Dictionary<int, string> m_SourcesOverride = new Dictionary<int, string>();



		public Script Script { get; }

		public Func<SourceCode, string> SourceFinder { get; }

		public string Name { get; set; }

		public DebugService DebugService { get; private set; }

		public Regex ErrorRegex { get; set; }

		public bool PauseRequested { get; set; }
		public bool IsStopped { get; private set; }

		public int Id { get; }


		public AsyncDebugger(Script script, Func<SourceCode, string> sourceFinder, string name)
		{
			Id = Interlocked.Increment(ref s_AsyncDebuggerIdCounter);
			SourceFinder = sourceFinder;
			ErrorRegex = new Regex(@"\A.*\Z");
			Script = script;
			m_WatchItems = new List<WatchItem>[(int)WatchType.MaxValue];
			Name = name;

			for (int i = 0; i < m_WatchItems.Length; i++)
				m_WatchItems[i] = new List<WatchItem>(64);
		}


		public IAsyncDebuggerClient Client
		{
			get => m_Client;
			set
			{
				if (m_Client != value)
				{
					lock (m_ClientLock)
					{
						IAsyncDebuggerClient previousClient = m_Client;

						m_Client = value;

						previousClient?.Unbind();

						if (m_Client != null)
						{
							for (int i = 0; i < Script.SourceCodeCount; i++)
								if (m_SourcesMap.ContainsKey(i))
									m_Client.OnSourceCodeChanged(i);
						}
					}
				}
			}
		}

		DebuggerAction IDebugger.GetAction(int ip, SourceRef sourceref)
		{
			var pauseRequested = PauseRequested;

			PauseRequested = false;
			IsStopped = true;

			if (pauseRequested || ip != m_PrevInstructionPtr)
			{
				lock (m_ActionQueue)
				{
					var nonExecutionActions = m_ActionQueue.Where(a => a.Action > DebuggerAction.ActionType.Run);
					m_ActionQueue.Clear();

					foreach (var action in nonExecutionActions)
					{
						m_ActionQueue.Enqueue(action);
					}
				}

				m_PrevInstructionPtr = ip;

				lock (m_ClientLock)
				{
					Client?.SendStopEvent();
				}
			}

			while (true)
			{
				if (Client == null || PauseRequested)
				{
					IsStopped = false;
					return new DebuggerAction { Action = DebuggerAction.ActionType.Run };
				}

				lock (m_ActionQueueLock)
				{
					if (m_ActionQueue.Count > 0)
					{
						var action = m_ActionQueue.Dequeue();
						IsStopped = action.Action != DebuggerAction.ActionType.Run;

						if (!IsStopped)
						{
							m_PrevInstructionPtr = -1;
						}

						return action;
					}
				}

				IsStopped = true;
				Sleep(10);
			}
		}


		public void QueueAction(DebuggerAction action)
		{
			lock (m_ActionQueueLock)
			{
				m_ActionQueue.Enqueue(action);
			}
		}

		private void Sleep(int v)
		{
			System.Threading.Thread.Sleep(10);
		}

		private DynamicExpression CreateDynExpr(string code)
		{
			try
			{
				return Script.CreateDynamicExpression(code);
			}
			catch (ScriptRuntimeException ex)
			{
				return Script.CreateConstantDynamicExpression(code, DynValue.NewString(ex.Message));
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
			m_SourcesOverride.Remove(sourceCode.SourceID);

			string file = SourceFinder(sourceCode);
			if (!string.IsNullOrEmpty(file) && file != sourceCode.Name)
			{
				m_SourcesOverride[sourceCode.SourceID] = file;
			}

			lock (m_ClientLock)
			{
				Client?.OnSourceCodeChanged(sourceCode.SourceID);
			}
		}

		public string GetSourceFile(int sourceId)
		{
			if (m_SourcesOverride.ContainsKey(sourceId))
				return m_SourcesOverride[sourceId];
			else if (m_SourcesMap.ContainsKey(sourceId))
				return m_SourcesMap[sourceId].Name;
			return null;
		}

		public bool IsSourceOverride(int sourceId)
		{
			return (m_SourcesOverride.ContainsKey(sourceId));
		}


		void IDebugger.SignalExecutionEnded()
		{
			lock (m_ClientLock)
			{
				Client?.OnExecutionEnded();
			}
		}

		bool IDebugger.SignalRuntimeException(ScriptRuntimeException ex)
		{
			lock (m_ClientLock)
			{
				if (Client == null)
				{
					return false;
				}

				Client.OnException(ex);
			}

			if (!ErrorRegex.IsMatch(ex.Message))
			{
				return false;
			}

			lock (m_ActionQueue)
			{
				var nonExecutionActions = m_ActionQueue.Where(a => a.Action > DebuggerAction.ActionType.Run);
				m_ActionQueue.Clear();

				foreach (var action in nonExecutionActions)
				{
					m_ActionQueue.Enqueue(action);
				}
			}

			PauseRequested = true;
			return PauseRequested;

		}

		void IDebugger.Update(WatchType watchType, IEnumerable<WatchItem> items, int stackFrameIndex)
		{
			var list = m_WatchItems[(int)watchType];

			list.Clear();
			list.AddRange(items);

			lock (m_ClientLock)
			{
				Client?.OnWatchesUpdated(watchType, stackFrameIndex);
			}
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

			foreach (var kvp in m_SourcesOverride)
			{
				if (kvp.Value.Replace('\\', '/').ToUpperInvariant() == path)
					return m_SourcesMap[kvp.Key];
			}

			return m_SourcesMap.Values.FirstOrDefault(s => s.Name.Replace('\\', '/').ToUpperInvariant() == path);
		}

		void IDebugger.SetDebugService(DebugService debugService)
		{
			DebugService = debugService;
		}

		public DynValue Evaluate(string expression, int stackFrameIndex)
		{
			var expr = CreateDynExpr(expression);
			var context = expr.OwnerScript.CreateDynamicExecutionContext(null, stackFrameIndex);
			return expr.Evaluate(context);
		}

		DebuggerCaps IDebugger.GetDebuggerCaps()
		{
			return DebuggerCaps.CanDebugSourceCode | DebuggerCaps.HasLineBasedBreakpoints;
		}
	}
}

#endif
