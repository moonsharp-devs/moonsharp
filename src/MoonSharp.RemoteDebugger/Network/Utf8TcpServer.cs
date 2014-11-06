using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace MoonSharp.RemoteDebugger.Network
{
	public class Utf8TcpServer : IDisposable 
	{
		int m_PortNumber = 1912;
		IPAddress m_IPAddress;
		TcpListener m_Listener = null;
		Action<string> m_Logger;
		List<Utf8TcpPeer> m_PeerList = new List<Utf8TcpPeer>();
		object m_PeerListLock = new object();
		public char PacketSeparator { get; private set; }

		public Utf8TcpServerOptions Options { get; private set; }

		public event EventHandler<Utf8TcpPeerEventArgs> ClientConnected;
		public event EventHandler<Utf8TcpPeerEventArgs> DataReceived;
		public event EventHandler<Utf8TcpPeerEventArgs> ClientDisconnected;

		public int PortNumber
		{
			get { return m_PortNumber; }
		}


		public Utf8TcpServer(int port, int bufferSize, char packetSeparator, Utf8TcpServerOptions options)
        {
			m_IPAddress = ((options & Utf8TcpServerOptions.LocalHostOnly) != 0) ? IPAddress.Loopback : IPAddress.Any;
            m_PortNumber = port;
			m_Logger = s => System.Diagnostics.Debug.WriteLine(s);
			PacketSeparator = packetSeparator;
			BufferSize = bufferSize;
			Options = options;
        }

		public Action<string> Logger
		{
			get { return m_Logger; }
			set { m_Logger = value ?? (s => Console.WriteLine(s)); }
		}

        public void Start()
        {
			m_Listener = new TcpListener(m_IPAddress, m_PortNumber);
			m_Listener.Start();
			m_Listener.BeginAcceptSocket(OnAcceptSocket, null);
        }

		public int BufferSize { get; private set; }


		private void OnAcceptSocket(IAsyncResult ar)
		{
			try
			{
				Socket s = m_Listener.EndAcceptSocket(ar);
				AddNewClient(s);
				m_Listener.BeginAcceptSocket(OnAcceptSocket, null);
			}
			catch (SocketException ex)
			{
				Logger("OnAcceptSocket : " + ex.Message);
			}
			catch (ObjectDisposedException ex)
			{
				Logger("OnAcceptSocket : " + ex.Message);
			}
		}

		public int GetConnectedClients()
		{
			lock (m_PeerListLock) 
				return m_PeerList.Count;
		}


		private void AddNewClient(Socket socket)
		{
			if ((Options & Utf8TcpServerOptions.SingleClientOnly) != 0)
			{
				lock (m_PeerListLock)
				{
					foreach (var pp in m_PeerList)
					{
						pp.Disconnect();
					}
				}
			}

			Utf8TcpPeer peer = new Utf8TcpPeer(this, socket);

			lock (m_PeerListLock)
			{
				m_PeerList.Add(peer);
				peer.ConnectionClosed += OnPeerDisconnected;
				peer.DataReceived += OnPeerDataReceived;
			}


			if (ClientConnected != null)
			{
				Utf8TcpPeerEventArgs args = new Utf8TcpPeerEventArgs(peer);
				ClientConnected(this, args);
			} 
			
			peer.Start();
		}

		private void OnPeerDataReceived(object sender, Utf8TcpPeerEventArgs e)
		{
			if (DataReceived != null)
			{
				DataReceived(this, e);
			}
		}

		void OnPeerDisconnected(object sender, Utf8TcpPeerEventArgs e)
        {
            try
            {
				if (ClientDisconnected != null)
				{
					ClientDisconnected(this, e);
				}

                lock (m_PeerListLock)
                {
                    m_PeerList.Remove(e.Peer);
					e.Peer.ConnectionClosed -= OnPeerDisconnected;
					e.Peer.DataReceived -= OnPeerDataReceived;
				}
            }
            catch
            {
            }
        }

        public void BroadcastMessage(string message)
        {
			List<Utf8TcpPeer> peers;

            lock (m_PeerListLock)
            {
				peers = m_PeerList.ToList();
			}

			message = CompleteMessage(message);

			if (message == null)
				return;

			foreach (Utf8TcpPeer peer in peers)
            {
                try
                {
					peer.SendTerminated(message);
                }
                catch { }
            }
        }

		public string CompleteMessage(string message)
		{
			if (string.IsNullOrEmpty(message))
				return PacketSeparator.ToString();

			if (message[message.Length - 1] != PacketSeparator)
				message = message + PacketSeparator;

			return message;
		}

        public void Stop()
        {
			m_Listener.Stop();
        }


		public void Dispose()
		{
			Stop();
		}
	}
}
