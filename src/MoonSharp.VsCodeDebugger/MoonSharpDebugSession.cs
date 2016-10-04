using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.DebuggerKit;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.VsCodeDebugger.SDK;
using Newtonsoft.Json.Linq;

namespace MoonSharp.VsCodeDebugger
{
	internal class MoonSharpDebugSession : DebugSession, IAsyncDebuggerClient
	{
		AsyncDebugger m_Debug;

		internal MoonSharpDebugSession(AsyncDebugger debugger)
			: base(true, false)
		{
			m_Debug = debugger;
		}

		public override void Initialize(Response response, JObject args)
		{
			m_Debug.Client = this;

			SendResponse(response, new Capabilities()
			{
				// This debug adapter does not need the configurationDoneRequest.
				supportsConfigurationDoneRequest = false,

				// This debug adapter does not support function breakpoints.
				supportsFunctionBreakpoints = false,

				// This debug adapter doesn't support conditional breakpoints.
				supportsConditionalBreakpoints = false,

				// This debug adapter does not support a side effect free evaluate request for data hovers.
				supportsEvaluateForHovers = false,

				// This debug adapter does not support exception breakpoint filters
				exceptionBreakpointFilters = new object[0]
			});

			// Debugger is ready to accept breakpoints immediately
			SendEvent(new InitializedEvent());
		}

		public override void Attach(Response response, JObject arguments)
		{
			LogDone("Attach");
			SendResponse(response);
		}

		private void LogTodo(string v)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(v);
		}

		private void LogTemp(string v)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(v);
		}

		private void LogDone(string v)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(v);
		}

		public override void Continue(Response response, JObject arguments)
		{
			LogTodo("Continue");
			m_Debug.QueueAction(new DebuggerAction() { Action = DebuggerAction.ActionType.Run });
			SendResponse(response);
		}

		public override void Disconnect(Response response, JObject arguments)
		{
			m_Debug.Client = null;

			LogTemp("Disconnect");
			SendResponse(response);
		}

		public override void Evaluate(Response response, JObject arguments)
		{
			LogTodo("Evaluate");
			SendResponse(response);
		}


		public override void Launch(Response response, JObject arguments)
		{
			LogTodo("Launch");
			SendResponse(response);
		}

		public override void Next(Response response, JObject arguments)
		{
			LogDone("Next");
			m_Debug.QueueAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepOver });
			SendResponse(response);
			//SendEvent(CreateStoppedEvent("step"));
		}

		private StoppedEvent CreateStoppedEvent(string reason, string text = null)
		{
			return new StoppedEvent(0, reason, text);
		}

		public override void Pause(Response response, JObject arguments)
		{
			//SendEvent(CreateStoppedEvent("step"));

			LogTemp("Pause");
			m_Debug.PauseRequested = true;
			SendResponse(response);
			SendText("Pause pending -- will pause at first script statement.");
		}

		public override void Scopes(Response response, JObject arguments)
		{
			LogTemp("Scopes");

			var scopes = new List<Scope>();
			SendResponse(response, new ScopesResponseBody(scopes));
		}
		 
		public override void SetBreakpoints(Response response, JObject args)
		{
			LogTodo("SetBreakpoints");
			string path = null;

			JObject args_source = args["source"] as JObject;

			if (args_source != null)
			{
				string p = args_source["path"].ToString();
				if (p != null && p.Trim().Length > 0)
					path = p;
			}

			if (path == null)
			{
				SendErrorResponse(response, 3010, "setBreakpoints: property 'source' is empty or misformed", null, false, true);
				return;
			}

			path = ConvertClientPathToDebugger(path);

			SourceCode src = m_Debug.FindSourceByName(path);

			if (src == null)
			{
				// we only support breakpoints in files mono can handle
				SendResponse(response, new SetBreakpointsResponseBody());
				return;
			}

			JArray clientLines = args["lines"] as JArray;

			var lin = new HashSet<int>(clientLines.Select(jt => ConvertClientLineToDebugger(jt.ToObject<int>())).ToArray());

			var lin2 = m_Debug.DebugService.ResetBreakPoints(src, lin);

			var breakpoints = new List<Breakpoint>();
			foreach (var l in lin)
			{
				breakpoints.Add(new Breakpoint(lin2.Contains(l), l));
			}

			response.SetBody(new SetBreakpointsResponseBody(breakpoints)); SendResponse(response);
		}

		public override void StackTrace(Response response, JObject args)
		{
			LogDone("StackTrace");
			int maxLevels = getInt(args, "levels", 10);
			int threadReference = getInt(args, "threadId", 0);

			var stackFrames = new List<StackFrame>();

			var stack = m_Debug.GetWatches(WatchType.CallStack);

			for (int i = 0; i < Math.Min(maxLevels - 1, stack.Count); i++)
			{
				WatchItem frame = stack[i];

				string name = frame.Name;
				SourceRef sourceRef = frame.Location ?? DefaultSourceRef;
				int sourceIdx = sourceRef.SourceIdx;
				SourceCode sourceCode = m_Debug.GetSource(sourceIdx);
				string path = sourceIdx <= 0 ? "(native)" : (sourceCode == null ? "???" : sourceCode.Name);
				string sourceName = Path.GetFileName(path);

				var source = new Source(sourceName, path); // ConvertDebuggerPathToClient(path));

				stackFrames.Add(new StackFrame(i, name, source,
					ConvertDebuggerLineToClient(sourceRef.FromLine), sourceRef.FromChar,
					ConvertDebuggerLineToClient(sourceRef.ToLine), sourceRef.ToChar));
			}

			stackFrames.Add(new StackFrame(Math.Min(maxLevels - 1, stack.Count), "(native)", null, 0));
			
			SendResponse(response, new StackTraceResponseBody(stackFrames));
		}

		readonly SourceRef DefaultSourceRef = new SourceRef(-1, 0, 0, 0, 0, false);

		private int getInt(JObject args, string propName, int defaultValue)
		{
			var jo = args[propName];

			if (jo == null)
				return defaultValue;
			else
				return jo.ToObject<int>();
		}


		public override void StepIn(Response response, JObject arguments)
		{
			LogTodo("StepIn");
			m_Debug.QueueAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepIn });
			SendResponse(response);
			//SendEvent(CreateStoppedEvent("step"));
		}

		public override void StepOut(Response response, JObject arguments)
		{
			LogTodo("StepOut");
			m_Debug.QueueAction(new DebuggerAction() { Action = DebuggerAction.ActionType.StepOut });
			SendResponse(response);
			//SendEvent(CreateStoppedEvent("step"));
		}

		public override void Threads(Response response, JObject arguments)
		{
			LogTemp("Threads");

			var threads = new List<Thread>() { new Thread(0, "Main Thread") };

			SendResponse(response, new ThreadsResponseBody(threads));
		}

		public override void Variables(Response response, JObject arguments)
		{
			LogTodo("Variables");
			SendResponse(response);
		}

		void IAsyncDebuggerClient.SendHostReady(bool hostReady)
		{
		}

		void IAsyncDebuggerClient.SendSourceRef(SourceRef sourceref)
		{
		}

		void IAsyncDebuggerClient.OnWatchesUpdated(WatchType watchType)
		{
			if (watchType == WatchType.CallStack)
				SendEvent(CreateStoppedEvent("step"));

			//Console.ForegroundColor = ConsoleColor.Cyan;
			//Console.WriteLine("::{0}", watchType);
		}

		void IAsyncDebuggerClient.OnSourceCodeChanged(int sourceID)
		{
			SendText("Loaded source '{0}'", m_Debug.GetSource(sourceID).Name);
		}

		public void OnExecutionEnded()
		{
			SendText("Execution ended.");
		}

		private void SendText(string msg, params object[] args)
		{
			msg = string.Format(msg, args);
			SendEvent(new OutputEvent("console", DateTime.Now.ToString("u") + ": " + msg + "\n"));
		}
	}
}
