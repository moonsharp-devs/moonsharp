using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MoonSharp.DebuggerKit;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.VsCodeDebugger.SDK;

namespace MoonSharp.VsCodeDebugger
{
	/// <summary>
	/// Class implementing a debugger allowing attaching from a Visual Studio Code debugging session.
	/// </summary>
	public class MoonSharpVsCodeDebugServer
	{
		int m_Port;
		AsyncDebugger m_Debugger;
		Func<SourceCode, string> m_SourceFinder;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoonSharpVsCodeDebugServer" /> class.
		/// </summary>
		/// <param name="script">The script object to debug.</param>
		/// <param name="port">The port on which the debugger listens. It's recommended to use 41912 unless you are going to keep more than one script object around.</param>
		/// <param name="sourceFinder">A function which gets in input a source code and returns the path to
		/// source file to use. It can return null and in that case (or if the file cannot be found)
		/// a temporary file will be generated on the fly.</param>
		public MoonSharpVsCodeDebugServer(Script script, int port, Func<SourceCode, string> sourceFinder = null)
		{
			m_Port = port;
			m_SourceFinder = sourceFinder ?? (s => s.Name);
			m_Debugger = new AsyncDebugger(script, m_SourceFinder);
		}


		/// <summary>
		/// Gets the debugger object (to register it).
		/// </summary>
		public IDebugger GetDebugger()
		{
			return m_Debugger;
		}

		private void RunSession(Stream inputStream, Stream outputStream)
		{
			MoonSharpDebugSession debugSession = new MoonSharpDebugSession(m_Debugger);
			//debugSession.TRACE = trace_requests;
			//debugSession.TRACE_RESPONSE = trace_responses;
			debugSession.ProcessLoop(inputStream, outputStream);
		}

		public void Rebind(Script script, Func<SourceCode, string> sourceFinder = null)
		{
			m_SourceFinder = sourceFinder ?? m_SourceFinder;
			m_Debugger.Unbind();
			m_Debugger = new AsyncDebugger(script, m_SourceFinder);
			script.AttachDebugger(m_Debugger);
		}

		/// <summary>
		/// Starts listening on the localhost for incoming connections.
		/// </summary>
		public void Start()
		{
			TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), m_Port);
			serverSocket.Start();

			SpawnThread("VsCodeDebugServer", () =>
			{
				while (true)
				{
					var clientSocket = serverSocket.AcceptSocket();
					if (clientSocket != null)
					{
						Console.Error.WriteLine(">> accepted connection from client");

						SpawnThread("VsCodeDebugSession", () =>
						{
							using (var networkStream = new NetworkStream(clientSocket))
							{
								try
								{
									RunSession(networkStream, networkStream);
								}
								catch (Exception e)
								{
									Console.Error.WriteLine("Exception: " + e);
								}
							}
							clientSocket.Close();
							Console.Error.WriteLine(">> client connection closed");
						});
					}
				}
			});
		}

		private void SpawnThread(string name, Action threadProc)
		{
			new System.Threading.Thread(() => threadProc())
			{
				IsBackground = true,
				Name = name
			}
			.Start();
		}
	}
}
