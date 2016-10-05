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

		/// <summary>
		/// Initializes a new instance of the <see cref="MoonSharpVsCodeDebugServer"/> class.
		/// </summary>
		/// <param name="script">The script object to debug.</param>
		/// <param name="port">The port on which the debugger listens. It's recommended to use 41912 unless you are going to keep more than one script object around.</param>
		public MoonSharpVsCodeDebugServer(Script script, int port)
		{
			m_Port = port;
			m_Debugger = new AsyncDebugger(script);
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

		/// <summary>
		/// Starts listening on the localhost for incoming connections.
		/// </summary>
		public void Start()
		{
			TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), m_Port);
			serverSocket.Start();

			new System.Threading.Thread(() => {
				while (true)
				{
					var clientSocket = serverSocket.AcceptSocket();
					if (clientSocket != null)
					{
						Console.Error.WriteLine(">> accepted connection from client");

						new System.Threading.Thread(() => {
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
						}).Start();
					}
				}
			}).Start();
		}
	}
}
