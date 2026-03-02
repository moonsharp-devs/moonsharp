#if (!UNITY_5) || UNITY_STANDALONE

/*---------------------------------------------------------------------------------------------
Copyright (c) Microsoft Corporation

All rights reserved.

MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace MoonSharp.VsCodeDebugger.SDK
{
	// ---- Types -------------------------------------------------------------------------

	public class Message
	{
		public int id { get; }
		public string format { get; }
		public object variables { get; }
		public object showUser { get; }
		public object sendTelemetry { get; }

		public Message(int id, string format, object variables = null, bool user = true, bool telemetry = false)
		{
			this.id = id;
			this.format = format;
			this.variables = variables;
			this.showUser = user;
			this.sendTelemetry = telemetry;
		}
	}

	public class StackFrame
	{
		public const string HINT_NORMAL = "normal";
		public const string HINT_LABEL = "label";
		public const string HINT_SUBTLE = "subtle";

		public int id { get; }
		public Source source { get; }
		public int line { get; }
		public int column { get; }
		public string name { get; }

		public int? endLine { get; }

		public int? endColumn { get; }

		public string presentationHint { get; }

		public StackFrame(int id, string name, Source source, int line, int column = 0, int? endLine = null, int? endColumn = null, string hint = HINT_LABEL)
		{
			this.id = id;
			this.name = name;
			this.source = source;

			// These should NEVER be negative
			this.line = Math.Max(0, line);
			this.column = Math.Max(0, column);

			this.endLine = endLine;
			this.endColumn = endColumn;

			this.presentationHint = hint;
		}
	}

	public class Scope
	{
		public string name { get; }
		public int variablesReference { get; }
		public bool expensive { get; }

		public Scope(string name, int variablesReference, bool expensive = false)
		{
			this.name = name;
			this.variablesReference = variablesReference;
			this.expensive = expensive;
		}
	}

	public class Variable
	{
		public string name { get; }
		public string value { get; }
		public string type { get; }
		public int variablesReference { get; }

		public Variable(string name, string value, string type, int variablesReference = 0)
		{
			this.name = name;
			this.value = value;
			this.type = type;
			this.variablesReference = variablesReference;
		}
	}

	public class Thread
	{
		public int id { get; }
		public string name { get; }

		public Thread(int id, string name)
		{
			this.id = id;
			if (name == null || name.Length == 0)
			{
				this.name = string.Format("Thread #{0}", id);
			}
			else
			{
				this.name = name;
			}
		}
	}

	public class Source
	{
		public const string HINT_NORMAL = "normal";
		public const string HINT_EMPHASIZE = "emphasize";
		public const string HINT_DEEMPHASIZE = "deemphasize";

		public string name { get; }
		public string path { get; }
		public int sourceReference { get; }
		public string presentationHint { get; }

		public Source(string name, string path, int sourceReference, string hint = HINT_NORMAL)
		{
			this.name = name;
			this.path = path;
			this.sourceReference = sourceReference;
			this.presentationHint = hint;
		}
	}

	public class Breakpoint
	{
		public bool verified { get; }
		public int line { get; }
		public string message { get; }
		public string reason { get; }

		public Breakpoint(int line)
		{
			this.verified = true;
			this.line = line;
		}

		public Breakpoint(string failureMessage)
		{
			this.verified = false;
			this.line = line;
			this.message = failureMessage;
			this.reason = "failed";
		}
	}

	// ---- Events -------------------------------------------------------------------------

	public class InitializedEvent : Event
	{
		public InitializedEvent()
			: base("initialized")
		{
		}
	}

	public class ContinuedEvent : Event
	{
		public ContinuedEvent(int threadId)
			: base("continued", new {
				threadId
			})
		{
		}
	}

	public class StoppedEvent : Event
	{
		public StoppedEvent(int threadId, string reason, string description, string text = null)
			: base("stopped", new
			{
				threadId,
				reason,
				description,
				text
			})
		{
		}
	}

	public class ExitedEvent : Event
	{
		public ExitedEvent(int exCode)
			: base("exited", new { exitCode = exCode })
		{
		}
	}

	public class TerminatedEvent : Event
	{
		public TerminatedEvent(object restart = null)
			: base("terminated", new {
				restart
			})
		{
		}
	}

	public class ThreadEvent : Event
	{
		public ThreadEvent(string reasn, int tid)
			: base("thread", new
			{
				reason = reasn,
				threadId = tid
			})
		{
		}
	}

	public class OutputEvent : Event
	{
		public OutputEvent(string cat, string outpt)
			: base("output", new
			{
				category = cat,
				output = outpt
			})
		{
		}
	}

	// ---- Response -------------------------------------------------------------------------

	public class Capabilities : ResponseBody
	{
		public bool supportsConfigurationDoneRequest { get; }
		public bool supportsFunctionBreakpoints { get; }
		public bool supportsConditionalBreakpoints { get; }
		public bool supportsEvaluateForHovers { get; }
		public object[] exceptionBreakpointFilters { get; }
		public bool supportsExceptionInfoRequest { get; }
		public bool supportsDelayedStackTraceLoading { get; }
		public bool supportsSourceRequest { get; }

		public Capabilities(bool supportsConfigurationDoneRequest, bool supportsFunctionBreakpoints, bool supportsConditionalBreakpoints, bool supportsEvaluateForHovers, object[] exceptionBreakpointFilters, bool supportsExceptionInfoRequest, bool supportsDelayedStackTraceLoading, bool supportsSourceRequest = false)
		{
			this.supportsConfigurationDoneRequest = supportsConfigurationDoneRequest;
			this.supportsFunctionBreakpoints = supportsFunctionBreakpoints;
			this.supportsConditionalBreakpoints = supportsConditionalBreakpoints;
			this.supportsEvaluateForHovers = supportsEvaluateForHovers;
			this.exceptionBreakpointFilters = exceptionBreakpointFilters;
			this.supportsExceptionInfoRequest = supportsExceptionInfoRequest;
			this.supportsDelayedStackTraceLoading = supportsDelayedStackTraceLoading;
			this.supportsSourceRequest = supportsSourceRequest;
		}
	}

	public class ErrorResponseBody : ResponseBody
	{
		public Message error { get; }

		public ErrorResponseBody(Message error)
		{
			this.error = error;
		}
	}

	public class StackTraceResponseBody : ResponseBody
	{
		public StackFrame[] stackFrames { get; }
		public int totalFrames { get; }

		public StackTraceResponseBody(List<StackFrame> frames, int total)
		{
			stackFrames = frames.ToArray<StackFrame>();
			totalFrames = total;
		}
	}

	public class ScopesResponseBody : ResponseBody
	{
		public Scope[] scopes { get; }

		public ScopesResponseBody(List<Scope> scps)
		{
			scopes = scps.ToArray<Scope>();
		}
	}

	public class VariablesResponseBody : ResponseBody
	{
		public Variable[] variables { get; }

		public VariablesResponseBody(List<Variable> vars)
		{
			variables = vars.ToArray<Variable>();
		}
	}

	public class ThreadsResponseBody : ResponseBody
	{
		public Thread[] threads { get; }

		public ThreadsResponseBody(List<Thread> ths)
		{
			threads = ths.ToArray<Thread>();
		}
	}

	public class EvaluateResponseBody : ResponseBody
	{
		public string result { get; }
		public string type { get; set;  }
		public int variablesReference { get; }

		public EvaluateResponseBody(string value, int reff = 0)
		{
			result = value;
			variablesReference = reff;
		}
	}

	public class SourceResponseBody : ResponseBody
	{
		public string content { get; }
		public string mimeType { get; }

		public SourceResponseBody(string content, string mimeType = null)
		{
			this.content = content;
			this.mimeType = mimeType;
		}
	}

	public class ExceptionDetails
	{
		public string message { get; }
		public string typeName { get; }
		public string fullTypeName { get; }
		public string evaluateName { get; }
		public string stackTrace { get; }
		public ExceptionDetails innerException { get; }

		public ExceptionDetails(string message, string typeName, string fullTypeName, string evaluateName, string stackTrace, ExceptionDetails innerException)
		{
			this.message = message;
			this.typeName = typeName;
			this.fullTypeName = fullTypeName;
			this.evaluateName = evaluateName;
			this.stackTrace = stackTrace;
			this.innerException = innerException;
		}
	}

	public class ExceptionInfoResponseBody : ResponseBody
	{
		public string exceptionId { get; }
		public string description { get; }
		public string breakMode { get; }
		public ExceptionDetails details { get; }

		public ExceptionInfoResponseBody(string exceptionId, string description, string breakMode, ExceptionDetails details)
		{
			this.exceptionId = exceptionId;
			this.description = description;
			this.breakMode = breakMode;
			this.details = details;
		}
	}

	public class SetBreakpointsResponseBody : ResponseBody
	{
		public Breakpoint[] breakpoints { get; }

		public SetBreakpointsResponseBody(List<Breakpoint> bpts = null)
		{
			if (bpts == null)
				breakpoints = new Breakpoint[0];
			else
				breakpoints = bpts.ToArray<Breakpoint>();
		}
	}

	// ---- The Session --------------------------------------------------------

	public abstract class DebugSession : ProtocolServer
	{
		private bool _clientLinesStartAt1 = true;
		private bool _clientPathsAreURI = false;

		private bool _initialized = false;

		public DebugSession()
		{
		}

		public void SendResponse(Response response, ResponseBody body = null)
		{
			if (!_initialized)
			{
				if (response.command != "initialize")
				{
					return;
				}

				_initialized = true;
			}

			if (body != null)
			{
				response.SetBody(body);
			}

			SendMessage(response);
		}

		public override void SendEvent(Event e)
		{
			if (_initialized)
			{
				base.SendEvent(e);
			}
		}

		public void SendErrorResponse(Response response, int id, string format, object arguments = null, bool user = true, bool telemetry = false)
		{
			var msg = new Message(id, format, arguments, user, telemetry);
			var message = Utilities.ExpandVariables(msg.format, msg.variables);
			response.SetErrorBody(message, new ErrorResponseBody(msg));
			SendMessage(response);
		}

		protected override void DispatchRequest(string command, Table args, Response response)
		{
			if (args == null)
			{
				args = new Table(null);
			}

			try
			{
				switch (command)
				{
					case "initialize":
						if (args["linesStartAt1"] != null)
						{
							_clientLinesStartAt1 = args.Get("linesStartAt1").ToObject<bool>();
						}

						var pathFormat = args.Get("pathFormat").ToObject<string>();
						if (pathFormat != null)
						{
							switch (pathFormat)
							{
								case "uri":
									_clientPathsAreURI = true;
									break;
								case "path":
									_clientPathsAreURI = false;
									break;
								default:
									SendErrorResponse(response, 1015, "initialize: bad value '{_format}' for pathFormat", new { _format = pathFormat });
									return;
							}
						}

						Initialize(response, args);
						break;

					case "launch":
						Launch(response, args);
						break;

					case "attach":
						Attach(response, args);
						break;

					case "disconnect":
						Disconnect(response, args);
						break;

					case "configurationDone":
						ConfigurationDone(response, args);
						break;

					case "next":
						Next(response, args);
						break;

					case "continue":
						Continue(response, args);
						break;

					case "stepIn":
						StepIn(response, args);
						break;

					case "stepOut":
						StepOut(response, args);
						break;

					case "pause":
						Pause(response, args);
						break;

					case "stackTrace":
						StackTrace(response, args);
						break;

					case "scopes":
						Scopes(response, args);
						break;

					case "variables":
						Variables(response, args);
						break;

					case "source":
						Source(response, args);
						break;

					case "threads":
						Threads(response, args);
						break;

					case "setBreakpoints":
						SetBreakpoints(response, args);
						break;

					case "setFunctionBreakpoints":
						SetFunctionBreakpoints(response, args);
						break;

					case "setExceptionBreakpoints":
						SetExceptionBreakpoints(response, args);
						break;

					case "evaluate":
						Evaluate(response, args);
						break;

					case "exceptionInfo":
						ExceptionInfo(response, args);
						break;

					default:
						SendErrorResponse(response, 1014, "unrecognized request: {_request}", new { _request = command });
						break;
				}
			}
			catch (Exception e)
			{
				SendErrorResponse(response, 1104, "error while processing request '{_request}' (exception: {_exception})", new { _request = command, _exception = e.Message });
			}

			if (command == "disconnect")
			{
				Stop();
			}
		}

		public abstract void Initialize(Response response, Table args);

		public abstract void Launch(Response response, Table arguments);

		public abstract void Attach(Response response, Table arguments);

		public abstract void Disconnect(Response response, Table arguments);

		public abstract void ConfigurationDone(Response response, Table arguments);

		public virtual void SetFunctionBreakpoints(Response response, Table arguments)
		{
			SendResponse(response);
		}

		public virtual void SetExceptionBreakpoints(Response response, Table arguments)
		{
			SendResponse(response);
		}

		public abstract void SetBreakpoints(Response response, Table arguments);

		public abstract void Continue(Response response, Table arguments);

		public abstract void Next(Response response, Table arguments);

		public abstract void StepIn(Response response, Table arguments);

		public abstract void StepOut(Response response, Table arguments);

		public abstract void Pause(Response response, Table arguments);

		public abstract void StackTrace(Response response, Table arguments);

		public abstract void Scopes(Response response, Table arguments);

		public abstract void Variables(Response response, Table arguments);

		public abstract void Source(Response response, Table arguments);

		public abstract void Threads(Response response, Table arguments);

		public abstract void Evaluate(Response response, Table arguments);

		public abstract void ExceptionInfo(Response response, Table arguments);

		// protected

		protected int ConvertDebuggerLineToClient(int line)
		{
			return _clientLinesStartAt1 ? line : line - 1;
		}

		protected int ConvertClientLineToDebugger(int line)
		{
			return _clientLinesStartAt1 ? line : line + 1;
		}

		protected string ConvertDebuggerPathToClient(string path)
		{
			if (path == null || !_clientPathsAreURI)
			{
				return path;
			}

			try
			{
				var uri = new Uri(path);
				return uri.AbsoluteUri;
			}
			catch
			{
				return null;
			}
		}

		protected string ConvertClientPathToDebugger(string clientPath)
		{
			if (clientPath != null && _clientPathsAreURI)
			{
				if (Uri.IsWellFormedUriString(clientPath, UriKind.Absolute))
				{
					Uri uri = new Uri(clientPath);
					return uri.LocalPath;
				}

				Console.Error.WriteLine("path not well formed: '{0}'", clientPath);
				return null;
			}

			return clientPath;
		}
	}
}
#endif
