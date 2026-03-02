#if (!PCL) && ((!UNITY_5) || UNITY_STANDALONE)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.VsCodeDebugger.SDK;

namespace MoonSharp.VsCodeDebugger.DebuggerLogic
{
	internal class ScriptDebugSession : DebugSession
	{
		const int SCOPE_GLOBAL = 0;
		const int SCOPE_SELF = 1;
		const int SCOPE_LOCAL = 2;
		const int SCOPE_CLOSURE = 3;

		const int SOURCE_REFERENCE_SOURCE_MASK = 0xFFFF;
		const int SOURCE_REFERENCE_THREAD_OFFSET = 16;
		const int SOURCE_REFERENCE_THREAD_MASK = 0x7FFF0000;

		const int STACK_FRAME_LEVEL_MASK = 0xFFF;
		const int STACK_FRAME_THREAD_OFFSET = 12;
		const int STACK_FRAME_THREAD_MASK = 0x7FFFF000;

		const int STOP_REASON_PAUSED = 0;
		const int STOP_REASON_STEP = 1;
		const int STOP_REASON_BREAKPOINT = 2;
		const int STOP_REASON_EXCEPTION = 3;

		readonly object m_Lock = new object();
		readonly object m_SendLock = new object();
		readonly Dictionary<int, ThreadState> m_ThreadsById = new Dictionary<int, ThreadState>();
		readonly Dictionary<Script, int> m_ThreadIdByScript = new Dictionary<Script, int>();
		readonly Dictionary<int, VariableReferenceState> m_VariableReferencesById = new Dictionary<int, VariableReferenceState>();

		int m_NextThreadId = 1;
		int m_NextVariableReferenceId = 1;
		int m_SelectedThreadId = -1;
		bool m_NotifyExecutionEnd;

		public string Name => "Multi-Script";

		internal ScriptDebugSession(int port, MoonSharpVsCodeDebugServer server)
		{
			_ = port;
			_ = server;
		}

		protected override void SendMessage(ProtocolMessage message)
		{
			lock (m_SendLock)
			{
				base.SendMessage(message);
			}
		}

		void SendTerminateEvent(bool restart)
		{
			SendEvent(new TerminatedEvent(restart ? new {} : null));
		}

		public int? SelectedDebuggerId
		{
			get
			{
				lock (m_Lock)
				{
					return m_SelectedThreadId > 0 ? m_SelectedThreadId : (int?) null;
				}
			}
		}

		public Script SelectedScript
		{
			get
			{
				lock (m_Lock)
				{
					return m_ThreadsById.TryGetValue(m_SelectedThreadId, out var threadState) ? threadState.Debugger?.Script : null;
				}
			}
		}

		public bool HasScript(Script script)
		{
			lock (m_Lock)
			{
				return script != null && m_ThreadIdByScript.ContainsKey(script);
			}
		}

		public IEnumerable<KeyValuePair<int, string>> GetAttachedDebuggersByIdAndName()
		{
			lock (m_Lock)
			{
				return m_ThreadsById
					.OrderBy(p => p.Key)
					.Select(p => new KeyValuePair<int, string>(p.Key, p.Value.Debugger?.Name ?? p.Value.Name))
					.ToList();
			}
		}

		public bool SelectDebugger(int? threadId)
		{
			lock (m_Lock)
			{
				if (threadId == null)
				{
					m_SelectedThreadId = m_ThreadsById.Keys.OrderBy(id => id).FirstOrDefault();
					return true;
				}

				if (!m_ThreadsById.ContainsKey(threadId.Value))
				{
					return false;
				}

				m_SelectedThreadId = threadId.Value;
				return true;
			}
		}

		public bool TryGetThreadIdForScript(Script script, out int threadId)
		{
			lock (m_Lock)
			{
				return m_ThreadIdByScript.TryGetValue(script, out threadId);
			}
		}

		public bool TryGetDebugger(int threadId, out AsyncDebugger debugger)
		{
			lock (m_Lock)
			{
				if (m_ThreadsById.TryGetValue(threadId, out var state))
				{
					debugger = state.Debugger;
					return debugger != null;
				}
			}

			debugger = null;
			return false;
		}

		public int AddDebugger(AsyncDebugger debugger)
		{
			if (debugger == null || debugger.Script == null)
			{
				throw new ArgumentException("Cannot attach null debugger or script.");
			}

			lock (m_Lock)
			{
				if (m_ThreadIdByScript.ContainsKey(debugger.Script))
				{
					throw new ArgumentException("Script already attached to this debug server.");
				}

				int threadId = m_NextThreadId++;
				var state = new ThreadState(threadId, debugger, new DebuggerClientProxy(this, threadId));
				m_ThreadsById[threadId] = state;
				m_ThreadIdByScript[debugger.Script] = threadId;

				if (m_SelectedThreadId <= 0)
				{
					m_SelectedThreadId = threadId;
				}

				debugger.Script.AttachDebugger(debugger);
				if (ClientConnected)
				{
					debugger.PauseRequested = true;
					debugger.Client = state.ClientProxy;
					SendEvent(new ThreadEvent("started", threadId));
				}

				return threadId;
			}
		}

		public bool ReplaceDebugger(Script previousScript, AsyncDebugger debugger)
		{
			if (previousScript == null)
			{
				return false;
			}

			lock (m_Lock)
			{
				if (!m_ThreadIdByScript.TryGetValue(previousScript, out var threadId) || !m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return false;
				}

				state.Debugger?.Script?.DetachDebugger();
				state.Debugger.Client = null;
				m_ThreadIdByScript.Remove(previousScript);
				ClearVariableReferencesForThread(state);

				if (debugger == null || debugger.Script == null)
				{
					m_ThreadsById.Remove(threadId);
					if (m_SelectedThreadId == threadId)
					{
						m_SelectedThreadId = m_ThreadsById.Keys.OrderBy(id => id).FirstOrDefault();
					}

					SendEvent(new ThreadEvent("exited", threadId));
					return true;
				}

				if (m_ThreadIdByScript.ContainsKey(debugger.Script))
				{
					throw new ArgumentException("Replacement script already attached to this debug server.");
				}

				state.Debugger = debugger;
				state.Name = debugger.Name;
				state.RuntimeException = null;
				state.StopReason = STOP_REASON_PAUSED;
				state.PendingStackFrame = -1;
				state.CurrentStackFrame = -1;
				state.CurrentCallStack.Clear();
				state.LocalScope.Reset();
				state.ClosureScope.Reset();

				m_ThreadIdByScript[debugger.Script] = threadId;

				debugger.Script.AttachDebugger(debugger);
				if (ClientConnected)
				{
					debugger.PauseRequested = true;
					debugger.Client = state.ClientProxy;
				}

				return true;
			}
		}

		public bool RemoveDebugger(Script script)
		{
			lock (m_Lock)
			{
				if (!m_ThreadIdByScript.TryGetValue(script, out var threadId) || !m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return false;
				}

				state.Debugger.Client = null;
				state.Debugger.Script.DetachDebugger();
				ClearVariableReferencesForThread(state);
				m_ThreadIdByScript.Remove(script);
				m_ThreadsById.Remove(threadId);

				if (m_SelectedThreadId == threadId)
				{
					m_SelectedThreadId = m_ThreadsById.Keys.OrderBy(id => id).FirstOrDefault();
				}

				SendEvent(new ThreadEvent("exited", threadId));
				return true;
			}
		}

		public override void Initialize(Response response, Table args)
		{
			SendResponse(response, new Capabilities(
				true,
				false,
				true,
				true,
				new object[0],
				true,
				true,
				true
			));

#if DOTNET_CORE
			SendText("Connected to MoonSharp {0} [{1}]",
					 Script.VERSION,
					 Script.GlobalOptions.Platform.GetPlatformName());
#else
			SendText("Connected to MoonSharp {0} [{1}] on process {2} (PID {3})",
					 Script.VERSION,
					 Script.GlobalOptions.Platform.GetPlatformName(),
					 System.Diagnostics.Process.GetCurrentProcess().ProcessName,
					 System.Diagnostics.Process.GetCurrentProcess().Id);
#endif

			SendText("Debugging multiple Lua scripts via DAP threads.");
			SendText("Type '!help' in the Debug Console for available commands.");

			lock (m_Lock)
			{
				foreach (var state in m_ThreadsById.Values)
				{
					state.Debugger.Client = state.ClientProxy;
				}
			}

			SendEvent(new InitializedEvent());
		}

		public override void Launch(Response response, Table arguments)
		{
			SendResponse(response);
		}

		public override void Attach(Response response, Table arguments)
		{
			SendResponse(response);
		}

		public override void Disconnect(Response response, Table arguments)
		{
			lock (m_Lock)
			{
				foreach (var state in m_ThreadsById.Values)
				{
					state.Debugger.Client = null;
					ClearVariableReferencesForThread(state);
				}
			}

			SendResponse(response);
		}

		public override void ConfigurationDone(Response response, Table arguments)
		{
			lock (m_Lock)
			{
				foreach (var state in m_ThreadsById.Values)
				{
					state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.Run });
				}
			}

			SendResponse(response);
		}

		public override void Continue(Response response, Table arguments)
		{
			int requestedThreadId = getInt(arguments, "threadId", 0);

			lock (m_Lock)
			{
				if (requestedThreadId > 0)
				{
					if (!m_ThreadsById.TryGetValue(requestedThreadId, out var state))
					{
						SendErrorResponse(response, 1, "Unknown thread");
						return;
					}

					ClearVariableReferencesForThread(state);
					state.StopReason = STOP_REASON_BREAKPOINT;
					state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.Run });
				}
				else
				{
					foreach (var state in m_ThreadsById.Values)
					{
						ClearVariableReferencesForThread(state);
						state.StopReason = STOP_REASON_BREAKPOINT;
						state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.Run });
					}
				}
			}

			SendResponse(response);
		}

		public override void Next(Response response, Table arguments)
		{
			if (!TryGetThreadState(arguments, out var state))
			{
				SendErrorResponse(response, 1, "Unknown thread");
				return;
			}

			ClearVariableReferencesForThread(state);
			state.StopReason = STOP_REASON_STEP;
			state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.StepOver });
			SendResponse(response);
		}

		public override void StepIn(Response response, Table arguments)
		{
			if (!TryGetThreadState(arguments, out var state))
			{
				SendErrorResponse(response, 1, "Unknown thread");
				return;
			}

			ClearVariableReferencesForThread(state);
			state.StopReason = STOP_REASON_STEP;
			state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.StepIn });
			SendResponse(response);
		}

		public override void StepOut(Response response, Table arguments)
		{
			if (!TryGetThreadState(arguments, out var state))
			{
				SendErrorResponse(response, 1, "Unknown thread");
				return;
			}

			ClearVariableReferencesForThread(state);
			state.StopReason = STOP_REASON_STEP;
			state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.StepOut });
			SendResponse(response);
		}

		public override void Pause(Response response, Table arguments)
		{
			int requestedThreadId = getInt(arguments, "threadId", 0);

			lock (m_Lock)
			{
				if (requestedThreadId > 0)
				{
					if (!m_ThreadsById.TryGetValue(requestedThreadId, out var state))
					{
						SendErrorResponse(response, 1, "Unknown thread");
						return;
					}

					state.StopReason = STOP_REASON_PAUSED;
					state.Debugger.PauseRequested = true;
				}
				else
				{
					foreach (var state in m_ThreadsById.Values)
					{
						state.StopReason = STOP_REASON_PAUSED;
						state.Debugger.PauseRequested = true;
					}
				}
			}

			SendResponse(response);
			SendText("Pause pending -- will pause at first script statement.");
		}

		public override void Source(Response response, Table arguments)
		{
			int sourceReference = getInt(arguments, "sourceReference", 0);

			if (sourceReference <= 0)
			{
				var sourceArgs = arguments.Get("source").Table;

				if (sourceArgs != null)
				{
					sourceReference = getInt(sourceArgs, "sourceReference", 0);
				}
			}

			if (sourceReference <= 0)
			{
				SendErrorResponse(response, 3010, "source: property 'sourceReference' is empty or malformed", null, false, true);
				return;
			}

			DecodeSourceReference(sourceReference, out var threadId, out var sourceId);

			ThreadState state;

			lock (m_Lock)
			{
				m_ThreadsById.TryGetValue(threadId, out state);
			}

			SourceCode source = state?.Debugger?.GetSource(sourceId);

			if (source == null)
			{
				SendErrorResponse(response, 1020, "No source available");
				return;
			}

			SendResponse(response, new SourceResponseBody(source.Code ?? string.Empty, "text/x-lua"));
		}

		public override void Threads(Response response, Table arguments)
		{
			var threads = new List<Thread>();

			lock (m_Lock)
			{
				foreach (var pair in m_ThreadsById.OrderBy(p => p.Key))
				{
					threads.Add(new Thread(pair.Key, pair.Value.Debugger?.Name ?? pair.Value.Name));
				}
			}

			SendResponse(response, new ThreadsResponseBody(threads));
		}

		public override void StackTrace(Response response, Table args)
		{
			if (!TryGetThreadState(args, out var state))
			{
				SendResponse(response, new StackTraceResponseBody(new List<StackFrame>(), 0));
				return;
			}

			var stack = state.Debugger.GetWatches(WatchType.CallStack);
			var coroutine = state.Debugger.GetWatches(WatchType.Threads).LastOrDefault();

			int startFrame = getInt(args, "startFrame", 0);
			int maxLevels = getInt(args, "levels", stack.Count + 2);

			var stackFrames = new List<StackFrame>();
			int stackMax = Math.Min(startFrame + maxLevels, stack.Count);

			for (int level = startFrame; level < stackMax; level++)
			{
				WatchItem frame = stack[level];
				SourceRef sourceRef = frame.Location ?? DefaultSourceRef;
				int sourceIdx = sourceRef.SourceIdx;
				string sourceFile = state.Debugger.GetSourceFile(sourceIdx);
				SourceCode sourceCode = state.Debugger.GetSource(sourceIdx);
				bool sourceAvailable = !sourceRef.IsClrLocation && sourceCode != null;
				int sourceReference = sourceAvailable ? EncodeSourceReference(state.ThreadId, sourceIdx) : 0;
				string sourcePath = sourceRef.IsClrLocation ? "(native)" : (sourceFile != null ? ConvertDebuggerPathToClient(sourceFile) : null);
				string sourceName = sourceRef.IsClrLocation
					? sourcePath
					: (!string.IsNullOrEmpty(sourcePath) ? Path.GetFileName(sourcePath) : sourceCode?.Name ?? "(source)");
				string sourceHint = sourceRef.IsClrLocation ? SDK.Source.HINT_DEEMPHASIZE : (level == 0 ? SDK.Source.HINT_EMPHASIZE : SDK.Source.HINT_NORMAL);
				var source = sourceAvailable ? new Source(sourceName, sourcePath, sourceReference, sourceHint) : null;
				int frameId = EncodeStackFrameId(state.ThreadId, level);
				string stackHint = sourceRef.IsClrLocation ? StackFrame.HINT_LABEL : (sourceFile != null ? StackFrame.HINT_NORMAL : StackFrame.HINT_SUBTLE);

				stackFrames.Add(new StackFrame(frameId, frame.Name, source,
					ConvertDebuggerLineToClient(sourceRef.FromLine), sourceRef.FromChar,
					ConvertDebuggerLineToClient(sourceRef.ToLine), sourceRef.ToChar,
					stackHint));
			}

			if (stackFrames.Count < maxLevels)
			{
				if (coroutine != null)
				{
					stackFrames.Add(new StackFrame(EncodeStackFrameId(state.ThreadId, stack.Count), "(" + coroutine.Name + ")", null, 0, 0, 0, 0, SDK.Source.HINT_DEEMPHASIZE));
				}
				else
				{
					stackFrames.Add(new StackFrame(EncodeStackFrameId(state.ThreadId, stack.Count), "(main coroutine)", null, 0, 0, 0, 0, SDK.Source.HINT_DEEMPHASIZE));
				}

				if (stackFrames.Count < maxLevels)
				{
					stackFrames.Add(new StackFrame(EncodeStackFrameId(state.ThreadId, stack.Count + 1), "(native)", null, 0, 0, 0, 0, SDK.Source.HINT_DEEMPHASIZE));
				}
			}

			SendResponse(response, new StackTraceResponseBody(stackFrames, stack.Count + 2));
		}

		public override void Scopes(Response response, Table arguments)
		{
			int frameId = getInt(arguments, "frameId", 0);
			DecodeStackFrameId(frameId, out var threadId, out var stackFrameIndex);

			var scopes = new List<Scope>();

			lock (m_Lock)
			{
				if (threadId > 0 && stackFrameIndex >= 0 && TryGetThreadState(threadId, out var state))
				{
					if (stackFrameIndex < state.Debugger.GetWatches(WatchType.CallStack).Count)
					{
						scopes.Add(new Scope("Local", CreateScopeReference(state, SCOPE_LOCAL, stackFrameIndex)));
						scopes.Add(new Scope("Closure", CreateScopeReference(state, SCOPE_CLOSURE, stackFrameIndex)));
						scopes.Add(new Scope("Global", CreateScopeReference(state, SCOPE_GLOBAL, stackFrameIndex), true));
						scopes.Add(new Scope("Self", CreateScopeReference(state, SCOPE_SELF, stackFrameIndex)));
					}
				}
			}

			SendResponse(response, new ScopesResponseBody(scopes));
		}

		public override void Variables(Response response, Table arguments)
		{
			int variablesReference = getInt(arguments, "variablesReference", 0);

			if (variablesReference <= 0)
			{
				SendResponse(response, new VariablesResponseBody(new List<Variable>()));
				return;
			}

			lock (m_Lock)
			{
				if (!TryGetVariableReference(variablesReference, out var reference, out var state))
				{
					SendResponse(response, new VariablesResponseBody(new List<Variable>()));
					return;
				}

				if (reference.Kind == VariableReferenceKind.Object)
				{
					SendResponse(response, new VariablesResponseBody(InspectVariableWithReferences(state, reference.Value)));
					return;
				}

				int scope = reference.Scope;
				int frameId = reference.FrameId;

				if (scope == SCOPE_LOCAL || scope == SCOPE_CLOSURE)
				{
					VariablesScopeState scopeState = scope == SCOPE_LOCAL ? state.LocalScope : state.ClosureScope;

					if (frameId != scopeState.CurrentStackFrame)
					{
						if (scopeState.PendingResponse != null)
						{
							SendErrorResponse(scopeState.PendingResponse, 1200, $"pending {scopeState.Name} (Variables) request cancelled");
							scopeState.PendingResponse = null;
						}

						scopeState.PendingResponse = response;

						if (state.PendingStackFrame != frameId)
						{
							VariablesScopeState otherScopeState = scope == SCOPE_LOCAL ? state.ClosureScope : state.LocalScope;
							if (otherScopeState.PendingResponse != null)
							{
								SendErrorResponse(otherScopeState.PendingResponse, 1200, $"pending {otherScopeState.Name} (Variables) request cancelled");
								otherScopeState.PendingResponse = null;
							}

							state.PendingStackFrame = frameId;

							if (frameId < state.Debugger.GetWatches(WatchType.CallStack).Count)
							{
								state.Debugger.QueueAction(new DebuggerAction { Action = DebuggerAction.ActionType.ViewFrame, StackFrame = frameId });
							}
							else
							{
								SendResponse(response, new VariablesResponseBody(new List<Variable>()));
							}
						}

						return;
					}

					SendScopeVariablesResponse(state, frameId, scope == SCOPE_LOCAL ? WatchType.Locals : WatchType.Closure, response);
					return;
				}

				if (scope == SCOPE_SELF)
				{
					var self = state.Debugger.Evaluate("self", state.CurrentStackFrame);
					SendResponse(response, new VariablesResponseBody(InspectVariableWithReferences(state, self)));
				}
				else if (scope == SCOPE_GLOBAL)
				{
					var global = state.Debugger.Evaluate("_G", state.CurrentStackFrame);
					SendResponse(response, new VariablesResponseBody(InspectVariableWithReferences(state, global)));
				}
				else
				{
					SendResponse(response, new VariablesResponseBody(new List<Variable>()));
				}
			}
		}

		public override void Evaluate(Response response, Table args)
		{
			string expression = getString(args, "expression");
			string context = getString(args, "context") ?? "hover";
			int frameId = getInt(args, "frameId", 0);

			if (context == "repl" && expression.StartsWith("!"))
			{
				ExecuteRepl(expression.Substring(1));
				SendResponse(response);
				return;
			}

			lock (m_Lock)
			{
				if (!TryGetThreadStateByFrame(frameId, out var state, out var stackFrame))
				{
					SendErrorResponse(response, 1, "No active threads");
					return;
				}

				try
				{
					DynValue result = state.Debugger.Evaluate(expression, stackFrame) ?? DynValue.Nil;
					int resultReference = CreateObjectReference(state, result);
					SendResponse(response, new EvaluateResponseBody(result.ToDebugPrintString(), resultReference)
					{
						type = result.Type.ToLuaDebuggerString()
					});
				}
				catch (Exception e)
				{
					SendErrorResponse(response, 1105, $"error while evaluating '{expression}' (exception: {e.Message})");
				}
			}
		}

		public override void ExceptionInfo(Response response, Table arguments)
		{
			ThreadState state;
			lock (m_Lock)
			{
				if (!TryGetThreadState(arguments, out state))
				{
					SendResponse(response);
					return;
				}
			}

			if (IsRuntimeExceptionCurrent(state))
			{
				SendResponse(response, new ExceptionInfoResponseBody("runtime", state.RuntimeException.Message, "always", ExceptionDetails(state.RuntimeException)));
			}
			else
			{
				SendResponse(response);
			}
		}

		public override void SetBreakpoints(Response response, Table args)
		{
			SourceCode source = null;
			ThreadState state = null;
			Table argsSource = args["source"] as Table;
			string path = null;

			if (argsSource != null)
			{
				int sourceReference = getInt(argsSource, "sourceReference", 0);
				if (sourceReference > 0)
				{
					DecodeSourceReference(sourceReference, out var threadId, out var sourceId);
					lock (m_Lock)
					{
						if (TryGetThreadState(threadId, out state))
						{
							source = state.Debugger.GetSource(sourceId);
						}
					}
				}

				string p = argsSource["path"].ToString();
				if (!string.IsNullOrWhiteSpace(p))
				{
					path = ConvertClientPathToDebugger(p);
				}
			}

			lock (m_Lock)
			{
				if (source == null && !string.IsNullOrEmpty(path))
				{
					foreach (var threadState in m_ThreadsById.Values)
					{
						SourceCode threadSource = threadState.Debugger.FindSourceByName(path);
						if (threadSource != null)
						{
							source = threadSource;
							state = threadState;
							break;
						}
					}
				}
			}

			if (source == null || state == null)
			{
				SendResponse(response, new SetBreakpointsResponseBody());
				return;
			}

			Table requestedBreakpoints = args.Get("breakpoints").Table ?? new Table(null);
			var pendingBreakpoints = new Dictionary<int, DynamicExpression>();
			var breakpointFailures = new Dictionary<int, Breakpoint>();

			foreach (var requestedBreakpoint in requestedBreakpoints.Values)
			{
				Table breakpointTable = requestedBreakpoint.ToObject<Table>();
				int line = breakpointTable.Get("line").ToObject<int>();
				DynValue condition = breakpointTable.Get("condition");

				try
				{
					DynamicExpression conditionExpression = condition.IsNil()
						? null
						: source.OwnerScript.CreateDynamicExpression(condition.ToObject<string>());
					pendingBreakpoints.Add(line, conditionExpression);
				}
				catch (Exception)
				{
					breakpointFailures[line] = new Breakpoint("Invalid breakpoint expression");
				}
			}

			Dictionary<int, DynamicExpression> confirmedBreakpoints = state.Debugger.DebugService.ResetBreakPoints(source, pendingBreakpoints);
			var breakpointResults = new List<Breakpoint>();

			foreach (var requestedBreakpoint in requestedBreakpoints.Values)
			{
				Table breakpointTable = requestedBreakpoint.ToObject<Table>();
				int line = breakpointTable.Get("line").ToObject<int>();

				if (confirmedBreakpoints.ContainsKey(line))
				{
					breakpointResults.Add(new Breakpoint(line));
				}
				else if (breakpointFailures.TryGetValue(line, out var failure))
				{
					breakpointResults.Add(failure);
				}
				else
				{
					breakpointResults.Add(new Breakpoint("Unable to set breakpoint at this location"));
				}
			}

			SendResponse(response, new SetBreakpointsResponseBody(breakpointResults));
		}

		public void Terminate(bool restart = false)
		{
			lock (m_Lock)
			{
				foreach (var state in m_ThreadsById.Values)
				{
					state.Debugger.Client = null;
					ClearVariableReferencesForThread(state);
				}
			}

			SendTerminateEvent(restart);
			Stop();
		}

		void OnSendStopEvent(int threadId)
		{
			lock (m_Lock)
			{
				if (!m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return;
				}

				m_SelectedThreadId = threadId;
				switch (state.StopReason)
				{
					case STOP_REASON_PAUSED:
						SendEvent(new StoppedEvent(threadId, "pause", "Paused by debugger"));
						break;
					case STOP_REASON_STEP:
						SendEvent(new StoppedEvent(threadId, "step", "Paused after stepping"));
						break;
					case STOP_REASON_BREAKPOINT:
						SendEvent(new StoppedEvent(threadId, "breakpoint", "Paused on breakpoint"));
						break;
					case STOP_REASON_EXCEPTION:
						SendEvent(new StoppedEvent(threadId, "exception", "Paused on exception", state.RuntimeException?.Message));
						break;
					default:
						SendEvent(new StoppedEvent(threadId, "unknown", "Paused for an unknown reason"));
						break;
				}
			}
		}

		void OnWatchesUpdated(int threadId, WatchType watchType, int stackFrameIndex)
		{
			lock (m_Lock)
			{
				if (!m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return;
				}

				if (stackFrameIndex >= 0 && state.CurrentStackFrame != stackFrameIndex)
				{
					state.CurrentStackFrame = stackFrameIndex;
				}

				if (watchType == WatchType.CallStack)
				{
					state.CurrentCallStack = state.Debugger.GetWatches(WatchType.CallStack);
					ClearVariableReferencesForThread(state);
				}

				if (watchType == WatchType.Locals || watchType == WatchType.Closure)
				{
					VariablesScopeState scopeState = watchType == WatchType.Locals ? state.LocalScope : state.ClosureScope;
					scopeState.CurrentStackFrame = state.CurrentStackFrame;

					if (state.CurrentStackFrame == state.PendingStackFrame && scopeState.PendingResponse != null)
					{
						SendScopeVariablesResponse(state, state.CurrentStackFrame, watchType, scopeState.PendingResponse);
						scopeState.PendingResponse = null;
					}
				}
			}
		}

		void OnSourceCodeChanged(int threadId, int sourceId)
		{
			lock (m_Lock)
			{
				if (!m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return;
				}

				if (state.Debugger.IsSourceOverride(sourceId))
				{
					SendText("[{0}] Loaded source '{1}' -> '{2}'", state.Debugger.Name, state.Debugger.GetSource(sourceId).Name, state.Debugger.GetSourceFile(sourceId));
				}
				else
				{
					SendText("[{0}] Loaded source '{1}'", state.Debugger.Name, state.Debugger.GetSource(sourceId).Name);
				}
			}
		}

		void OnExecutionEnded(int threadId)
		{
			lock (m_Lock)
			{
				if (m_NotifyExecutionEnd && m_ThreadsById.TryGetValue(threadId, out var state))
				{
					SendText("[{0}] Execution ended.", state.Debugger?.Name ?? state.Name);
				}
			}
		}

		void OnException(int threadId, ScriptRuntimeException ex)
		{
			lock (m_Lock)
			{
				if (!m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return;
				}

				if (state.Debugger.ErrorRegex.IsMatch(ex.Message))
				{
					state.StopReason = STOP_REASON_EXCEPTION;
					state.RuntimeException = ex;
				}
			}
		}

		void OnUnbind(int threadId)
		{
			lock (m_Lock)
			{
				if (!m_ThreadsById.TryGetValue(threadId, out var state))
				{
					return;
				}

				if (state.Debugger.Client != state.ClientProxy)
				{
					return;
				}

				state.Debugger.Client = null;
			}
		}

		void SendScopeVariablesResponse(ThreadState state, int frameId, WatchType watchType, Response response)
		{
			var variables = new List<Variable>();

			foreach (WatchItem watch in state.Debugger.GetWatches(watchType))
			{
				DynValue value = watch.Value ?? DynValue.Void;
				int reference = CreateObjectReference(state, value);
				variables.Add(new Variable(watch.Name, value.ToDebugPrintString(), value.Type.ToLuaDebuggerString(), reference));
			}

			SendResponse(response, new VariablesResponseBody(variables));
		}

		ExceptionDetails ExceptionDetails(ScriptRuntimeException ex)
		{
			return new ExceptionDetails(
				ex.Message,
				null,
				null,
				null,
				null,
				null
			);
		}

		bool IsRuntimeExceptionCurrent(ThreadState state)
		{
			if (state.RuntimeException == null)
			{
				return false;
			}

			IList<WatchItem> exceptionCallStack = state.RuntimeException.CallStack;
			IList<WatchItem> debuggerCallStack = state.Debugger.GetWatches(WatchType.CallStack);

			if (exceptionCallStack.Count != debuggerCallStack.Count)
			{
				return false;
			}

			for (int i = 0; i < exceptionCallStack.Count; i++)
			{
				WatchItem a = exceptionCallStack[i];
				WatchItem b = debuggerCallStack[i];
				if (a.Address != b.Address || a.BasePtr != b.BasePtr || a.RetAddress != b.RetAddress)
				{
					return false;
				}
			}

			return true;
		}

		void ExecuteRepl(string cmd)
		{
			bool showHelp = false;
			cmd = cmd.Trim();

			if (cmd == "help")
			{
				showHelp = true;
			}
			else if (cmd.StartsWith("execendnotify"))
			{
				string val = cmd.Substring("execendnotify".Length).Trim();
				if (val == "off")
				{
					m_NotifyExecutionEnd = false;
				}
				else if (val == "on")
				{
					m_NotifyExecutionEnd = true;
				}
				else if (val.Length > 0)
				{
					SendText("Error : expected 'on' or 'off'");
				}

				SendText("Notifications of execution end are : {0}", m_NotifyExecutionEnd ? "enabled" : "disabled");
			}
			else
			{
				SendText("Syntax error : {0}\n", cmd);
				showHelp = true;
			}

			if (showHelp)
			{
				SendText("Available commands : ");
				SendText("    !help - gets this help");
				SendText("    !execendnotify [on|off] - sets execution end notification on/off");
				SendText("    ... or type an expression to evaluate it on the fly.");
			}
		}

		void SendText(string msg, params object[] args)
		{
			SendEvent(new OutputEvent("console", string.Format(msg, args) + "\n"));
		}

		bool TryGetThreadState(Table args, out ThreadState state)
		{
			int threadId = getInt(args, "threadId", 0);
			if (threadId <= 0)
			{
				threadId = m_SelectedThreadId;
			}

			if (threadId <= 0)
			{
				threadId = m_ThreadsById.Keys.OrderBy(id => id).FirstOrDefault();
			}

			return TryGetThreadState(threadId, out state);
		}

		bool TryGetThreadState(int threadId, out ThreadState state)
		{
			if (threadId > 0 && m_ThreadsById.TryGetValue(threadId, out state))
			{
				m_SelectedThreadId = threadId;
				return true;
			}

			state = null;
			return false;
		}

		bool TryGetThreadStateByFrame(int frameId, out ThreadState state, out int stackFrame)
		{
			if (frameId > 0)
			{
				DecodeStackFrameId(frameId, out var threadId, out stackFrame);
				return TryGetThreadState(threadId, out state);
			}

			stackFrame = 0;
			return TryGetThreadState(m_SelectedThreadId, out state) || TryGetThreadState(m_ThreadsById.Keys.OrderBy(id => id).FirstOrDefault(), out state);
		}

		int CreateScopeReference(ThreadState state, int scope, int frameId)
		{
			return CreateVariableReference(state, VariableReferenceKind.ScopeRoot, frameId, scope, null);
		}

		int CreateObjectReference(ThreadState state, object value)
		{
			return CreateVariableReference(state, VariableReferenceKind.Object, 0, 0, value);
		}

		int CreateVariableReference(ThreadState state, VariableReferenceKind kind, int frameId, int scope, object value)
		{
			int id = m_NextVariableReferenceId++;
			var reference = new VariableReferenceState(state.ThreadId, state.VariableGeneration, kind, frameId, scope, value);
			m_VariableReferencesById[id] = reference;
			state.ActiveVariableReferenceIds.Add(id);
			return id;
		}

		void ClearVariableReferencesForThread(ThreadState state)
		{
			foreach (int id in state.ActiveVariableReferenceIds)
			{
				m_VariableReferencesById.Remove(id);
			}

			state.ActiveVariableReferenceIds.Clear();
			state.VariableGeneration++;
		}

		bool TryGetVariableReference(int id, out VariableReferenceState reference, out ThreadState state)
		{
			if (!m_VariableReferencesById.TryGetValue(id, out reference))
			{
				state = null;
				return false;
			}

			if (!m_ThreadsById.TryGetValue(reference.ThreadId, out state))
			{
				m_VariableReferencesById.Remove(id);
				reference = null;
				return false;
			}

			if (reference.Generation != state.VariableGeneration)
			{
				m_VariableReferencesById.Remove(id);
				state.ActiveVariableReferenceIds.Remove(id);
				reference = null;
				state = null;
				return false;
			}

			return true;
		}

		List<Variable> InspectVariableWithReferences(ThreadState state, object value)
		{
			var variables = new List<Variable>();
			var structuredVariables = new List<object>();

			VariableInspector.InspectVariable(value, variables, structuredVariables);

			if (variables.Count == 0)
			{
				return variables;
			}

			var rewrittenVariables = new List<Variable>(variables.Count);

			foreach (Variable variable in variables)
			{
				if (variable.variablesReference > 0 && variable.variablesReference <= structuredVariables.Count)
				{
					int objectReference = CreateObjectReference(state, structuredVariables[variable.variablesReference - 1]);
					rewrittenVariables.Add(new Variable(variable.name, variable.value, variable.type, objectReference));
				}
				else
				{
					rewrittenVariables.Add(new Variable(variable.name, variable.value, variable.type));
				}
			}

			return rewrittenVariables;
		}

		int EncodeSourceReference(int threadId, int sourceId)
		{
			return ((threadId & 0x7FFF) << SOURCE_REFERENCE_THREAD_OFFSET) | ((sourceId + 1) & SOURCE_REFERENCE_SOURCE_MASK);
		}

		void DecodeSourceReference(int sourceReference, out int threadId, out int sourceId)
		{
			threadId = (sourceReference & SOURCE_REFERENCE_THREAD_MASK) >> SOURCE_REFERENCE_THREAD_OFFSET;
			sourceId = (sourceReference & SOURCE_REFERENCE_SOURCE_MASK) - 1;
		}

		int EncodeStackFrameId(int threadId, int level)
		{
			return (threadId << STACK_FRAME_THREAD_OFFSET) | (level & STACK_FRAME_LEVEL_MASK);
		}

		void DecodeStackFrameId(int frameId, out int threadId, out int level)
		{
			threadId = (frameId & STACK_FRAME_THREAD_MASK) >> STACK_FRAME_THREAD_OFFSET;
			level = frameId & STACK_FRAME_LEVEL_MASK;
		}

		string getString(Table args, string property, string dflt = null)
		{
			string s = (string) args[property];

			if (s == null)
			{
				return dflt;
			}

			s = s.Trim();
			return s.Length == 0 ? dflt : s;
		}

		int getInt(Table args, string propName, int defaultValue)
		{
			DynValue value = args.Get(propName);
			return value.Type == DataType.Number ? value.ToObject<int>() : defaultValue;
		}

		readonly SourceRef DefaultSourceRef = new SourceRef(-1, 0, 0, 0, 0, false);

		enum VariableReferenceKind
		{
			ScopeRoot,
			Object
		}

		class VariableReferenceState
		{
			public int ThreadId { get; }
			public int Generation { get; }
			public VariableReferenceKind Kind { get; }
			public int FrameId { get; }
			public int Scope { get; }
			public object Value { get; }

			public VariableReferenceState(int threadId, int generation, VariableReferenceKind kind, int frameId, int scope, object value)
			{
				ThreadId = threadId;
				Generation = generation;
				Kind = kind;
				FrameId = frameId;
				Scope = scope;
				Value = value;
			}
		}

		/**
		 * Thread refers to Debug Adapter Protocol's concept of a thread. We map this concept to a Script / isolated VM.
		 */
		class ThreadState
		{
			public int ThreadId { get; }
			public AsyncDebugger Debugger { get; set; }
			public DebuggerClientProxy ClientProxy { get; }
			public string Name { get; set; }
			public List<WatchItem> CurrentCallStack { get; set; } = new List<WatchItem>();
			public int CurrentStackFrame { get; set; } = -1;
			public int PendingStackFrame { get; set; } = -1;
			public VariablesScopeState LocalScope { get; } = new VariablesScopeState("Local");
			public VariablesScopeState ClosureScope { get; } = new VariablesScopeState("Closure");
			public int StopReason { get; set; } = STOP_REASON_PAUSED;
			public ScriptRuntimeException RuntimeException { get; set; }
			public int VariableGeneration { get; set; } = 1;
			public HashSet<int> ActiveVariableReferenceIds { get; } = new HashSet<int>();

			public ThreadState(int threadId, AsyncDebugger debugger, DebuggerClientProxy clientProxy)
			{
				ThreadId = threadId;
				Debugger = debugger;
				ClientProxy = clientProxy;
				Name = debugger?.Name ?? $"Script {threadId}";
			}
		}

		class DebuggerClientProxy : IAsyncDebuggerClient
		{
			readonly ScriptDebugSession m_Session;
			readonly int m_ThreadId;

			public DebuggerClientProxy(ScriptDebugSession session, int threadId)
			{
				m_Session = session;
				m_ThreadId = threadId;
			}

			public void SendStopEvent()
			{
				m_Session.OnSendStopEvent(m_ThreadId);
			}

			public void OnWatchesUpdated(WatchType watchType, int stackFrameIndex)
			{
				m_Session.OnWatchesUpdated(m_ThreadId, watchType, stackFrameIndex);
			}

			public void OnSourceCodeChanged(int sourceID)
			{
				m_Session.OnSourceCodeChanged(m_ThreadId, sourceID);
			}

			public void OnExecutionEnded()
			{
				m_Session.OnExecutionEnded(m_ThreadId);
			}

			public void OnException(ScriptRuntimeException ex)
			{
				m_Session.OnException(m_ThreadId, ex);
			}

			public void Unbind()
			{
				m_Session.OnUnbind(m_ThreadId);
			}
		}
	}

	public class VariablesScopeState
	{
		public string Name { get; }

		public int CurrentStackFrame { get; set; } = -1;
		public Response PendingResponse { get; set; }

		public VariablesScopeState(string name)
		{
			Name = name;
		}

		public void Reset()
		{
			CurrentStackFrame = -1;
			PendingResponse = null;
		}
	}
}

#endif
