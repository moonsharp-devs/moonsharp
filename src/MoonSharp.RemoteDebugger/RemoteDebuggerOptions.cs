using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.RemoteDebugger.Network;

namespace MoonSharp.RemoteDebugger
{
	public struct RemoteDebuggerOptions
	{
		public Utf8TcpServerOptions NetworkOptions;

		public bool SingleScriptMode;

		public int? HttpPort;
		public int RpcPortBase;

		public static RemoteDebuggerOptions Default
		{
			get
			{
				return new RemoteDebuggerOptions()
				{
					NetworkOptions = Utf8TcpServerOptions.LocalHostOnly | Utf8TcpServerOptions.SingleClientOnly,
					SingleScriptMode = false,
					HttpPort = 2705,
					RpcPortBase = 2006,
				};
			}
		}

	}
}
