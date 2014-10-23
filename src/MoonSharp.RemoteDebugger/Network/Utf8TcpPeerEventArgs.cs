using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.RemoteDebugger.Network
{
	public class Utf8TcpPeerEventArgs : EventArgs
	{
		public Utf8TcpPeerEventArgs(Utf8TcpPeer peer, string message = null)
		{
			Peer = peer;
			Message = message;
		}

		public Utf8TcpPeer Peer { get; private set; }
		public string Message { get; private set; }
	}
}
