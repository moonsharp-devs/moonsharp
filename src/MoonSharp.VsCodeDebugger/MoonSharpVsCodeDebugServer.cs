#if (!UNITY_5) || UNITY_STANDALONE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.VsCodeDebugger.DebuggerLogic;

namespace MoonSharp.VsCodeDebugger
{
	/// <summary>
	/// Class implementing a debugger allowing attaching from a Visual Studio Code debugging session.
	/// </summary>
	public class MoonSharpVsCodeDebugServer : IDisposable
	{
		readonly int m_Port;
		readonly object m_Lock = new object();
		readonly List<AsyncDebugger> m_PendingDebuggerList = new List<AsyncDebugger>();

		TcpListener m_Listener;
		ScriptDebugSession m_Session;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoonSharpVsCodeDebugServer" /> class.
		/// </summary>
		/// <param name="port">The port on which the debugger listens. It's recommended to use 41912.</param>
		public MoonSharpVsCodeDebugServer(int port = 41912)
		{
			m_Port = port;
		}

		/// <summary>
		/// Attaches the specified script to the debug server.
		/// </summary>
		public void AttachToScript(Script script, string name, Func<SourceCode, string> sourceFinder = null)
		{
			if (script == null)
			{
				throw new ArgumentException("Cannot attach to null");
			}

			lock (m_Lock)
			{
				if (m_Session?.HasScript(script) == true || m_PendingDebuggerList.Any(d => d.Script == script))
				{
					throw new ArgumentException("Script already attached to this debug server.");
				}

				var debugger = new AsyncDebugger(script, sourceFinder ?? (s => s.Name), name);
				if (m_Session != null)
				{
					m_Session.AddDebugger(debugger);
				}
				else
				{
					m_PendingDebuggerList.Add(debugger);
				}
			}
		}

		/// <summary>
		/// Replaces a script attached to the debug server.
		/// </summary>
		public void ReplaceAttachedScript(Script previousScript, Script newScript, string name = null, Func<SourceCode, string> sourceFinder = null)
		{
			lock (m_Lock)
			{
				if (newScript == null)
				{
					Detach(previousScript);
					return;
				}

				if (m_Session != null)
				{
					if (!m_Session.HasScript(previousScript))
					{
						throw new ArgumentException($"Cannot replace script \"{name}\" that is not attached to this debug server.");
					}

					var selectedName = name ?? "Lua Script";
					Func<SourceCode, string> selectedSourceFinder = sourceFinder;

					if (selectedSourceFinder == null &&
						m_Session.TryGetThreadIdForScript(previousScript, out var previousThreadId) &&
						m_Session.TryGetDebugger(previousThreadId, out var previousDebugger))
					{
						selectedSourceFinder = previousDebugger.SourceFinder;
						selectedName = name ?? previousDebugger.Name;
					}

					var replacement = new AsyncDebugger(newScript, selectedSourceFinder ?? (s => s.Name), selectedName);
					m_Session.ReplaceDebugger(previousScript, replacement);
				}
				else
				{
					int index = m_PendingDebuggerList.FindIndex(d => d.Script == previousScript);
					if (index < 0)
					{
						throw new ArgumentException($"Cannot replace script \"{name}\" that is not attached to this pending debug server.");
					}

					AsyncDebugger previousDebugger = m_PendingDebuggerList[index];
					previousScript.DetachDebugger();
					var replacement = new AsyncDebugger(newScript, sourceFinder ?? previousDebugger.SourceFinder, name ?? previousDebugger.Name)
					{
						PauseRequested = true
					};
					m_PendingDebuggerList[index] = replacement;
				}
			}
		}

		/// <summary>
		/// Detaches the specified script.
		/// </summary>
		public void Detach(Script script)
		{
			if (script == null)
			{
				throw new ArgumentException("Cannot detach null.");
			}

			lock (m_Lock)
			{
				if (m_Session != null)
				{
					if (!m_Session.RemoveDebugger(script))
					{
						throw new ArgumentException("Cannot find script associated with debugger.");
					}
				}
				else
				{
					int removed = m_PendingDebuggerList.RemoveAll(d => d.Script == script);
					if (removed == 0)
					{
						throw new ArgumentException("Cannot find script associated with debugger.");
					}
				}
			}
		}

		/// <summary>
		/// Gets a list of the attached debuggers by id and name
		/// </summary>
		public IEnumerable<KeyValuePair<int, string>> GetAttachedDebuggersByIdAndName()
		{
			lock (m_Lock)
			{
				if (m_Session != null)
				{
					return m_Session.GetAttachedDebuggersByIdAndName().ToList();
				}

				int threadId = 1;
				return m_PendingDebuggerList.Select(d => new KeyValuePair<int, string>(threadId++, d.Name)).ToList();
			}
		}

		/// <summary>
		/// Gets a list of the listening sessions by port and debugger name
		/// </summary>
		public IEnumerable<KeyValuePair<int, string>> GetSessionsByPortAndName()
		{
			lock (m_Lock)
			{
				if (m_Listener == null)
				{
					return new List<KeyValuePair<int, string>>();
				}

				return new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>(((IPEndPoint)m_Listener.LocalEndpoint).Port, "MoonSharp")
				};
			}
		}

		/// <summary>
		/// Gets a list of the attached listeners by port and debugger summary
		/// </summary>
		public IEnumerable<KeyValuePair<int, string>> GetListenersByPortAndDebuggerName()
		{
			lock (m_Lock)
			{
				if (m_Listener == null)
				{
					return new List<KeyValuePair<int, string>>();
				}

				int count = m_Session?.GetAttachedDebuggersByIdAndName().Count() ?? m_PendingDebuggerList.Count;

				return new List<KeyValuePair<int, string>>
				{
					new KeyValuePair<int, string>(((IPEndPoint)m_Listener.LocalEndpoint).Port, $"{count} script(s)")
				};
			}
		}

		/// <summary>
		/// Gets or sets a delegate which will be called when logging messages are generated
		/// </summary>
		public Action<string> Logger { get; set; }

		/// <summary>
		/// Stops listening.
		/// </summary>
		public void Dispose()
		{
			lock (m_Lock)
			{
				m_Session?.Terminate();
				m_Session = null;
				m_Listener?.Stop();
				m_Listener = null;
				m_PendingDebuggerList.Clear();
			}
		}

		/// <summary>
		/// Starts listening on localhost for incoming connections.
		/// </summary>
		public MoonSharpVsCodeDebugServer Start()
		{
			lock (m_Lock)
			{
				if (m_Session != null)
				{
					throw new InvalidOperationException("Cannot start; server has already been started.");
				}

				m_Listener = new TcpListener(IPAddress.Loopback, m_Port);
				m_Listener.Start();
				int port = ((IPEndPoint)m_Listener.LocalEndpoint).Port;
				m_Session = new ScriptDebugSession(port, this);

				foreach (AsyncDebugger debugger in m_PendingDebuggerList)
				{
					m_Session.AddDebugger(debugger);
				}

				m_PendingDebuggerList.Clear();

				SpawnThread("VsCodeDebugServer_" + port, ListenThread);
				return this;
			}
		}

		void ListenThread()
		{
			TcpListener listener;
			ScriptDebugSession session;

			lock (m_Lock)
			{
				listener = m_Listener;
				session = m_Session;
			}

			if (listener == null || session == null)
			{
				return;
			}

			int port = ((IPEndPoint)listener.LocalEndpoint).Port;
			string sessionIdentifier = port.ToString();

			try
			{
				while (true)
				{
					Socket clientSocket = listener.AcceptSocket();
					Log("[{0}] : Accepted connection from client {1}", sessionIdentifier, clientSocket.RemoteEndPoint);

					if (session.ClientConnected)
					{
						Log("[{0}] : Rejecting connection because a debug client is already connected", sessionIdentifier);
						try
						{
							clientSocket.Shutdown(SocketShutdown.Both);
						}
						catch (SocketException)
						{
							// ignore
						}
						finally
						{
							clientSocket.Close();
						}
						continue;
					}

					SpawnThread("VsCodeDebugSession_" + sessionIdentifier, () =>
					{
						using (var networkStream = new NetworkStream(clientSocket))
						{
							try
							{
								session.ProcessLoop(networkStream, networkStream);
							}
							catch (Exception ex)
							{
								Log("[{0}] : Error : {1}", sessionIdentifier, ex.Message);
							}
						}

						try
						{
							clientSocket.Shutdown(SocketShutdown.Both);
						}
						catch (SocketException)
						{
							// ignore
						}
						finally
						{
							clientSocket.Close();
						}

						Log("[{0}] : Client connection closed", sessionIdentifier);
					});
				}
			}
			catch (SocketException)
			{
				// expected when stopping listener
			}
			catch (Exception ex)
			{
				Log("Fatal error in listening thread : {0}", ex.Message);
			}
		}

		void Log(string format, params object[] args)
		{
			Action<string> logger = Logger;
			if (logger != null)
			{
				logger(string.Format(format, args));
			}
		}

		static void SpawnThread(string name, Action threadProc)
		{
			new System.Threading.Thread(() => threadProc())
			{
				IsBackground = true,
				Name = name
			}.Start();
		}
	}
}

#else
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.VsCodeDebugger
{
	public class MoonSharpVsCodeDebugServer : IDisposable
	{
		public MoonSharpVsCodeDebugServer(int port = 41912)
		{
		}

		public void AttachToScript(Script script, string name, Func<SourceCode, string> sourceFinder = null)
		{
		}

		public void ReplaceAttachedScript(Script previousScript, Script newScript, string name = null, Func<SourceCode, string> sourceFinder = null)
		{
		}

		public IEnumerable<KeyValuePair<int, string>> GetAttachedDebuggersByIdAndName()
		{
			yield break;
		}

		public IEnumerable<KeyValuePair<int, string>> GetListenersByPortAndDebuggerName()
		{
			yield break;
		}

		public IEnumerable<KeyValuePair<int, string>> GetSessionsByPortAndName()
		{
			yield break;
		}

		/// <summary>
		/// Detaches the specified script. The debugger attached to that script will get disconnected.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <exception cref="ArgumentException">Thrown if the script cannot be found.</exception>
		public void Detach(Script script)
		{
		}

		public Action<string> Logger { get; set; }

		public void Dispose()
		{
		}

		public MoonSharpVsCodeDebugServer Start()
		{
			return this;
		}
	}
}
#endif
