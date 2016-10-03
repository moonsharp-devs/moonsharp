using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace MoonSharp.RemoteDebugger.Network
{
	/// <summary>
	/// This is a very very (very!) simplified and light http server. It exists to run on platforms where 
	/// more standard methods offered by .NET BCL are not available and/or if priviledges cannot be 
	/// excalated. This just uses a TcpListener and a Socket.
	/// This supports only GET method and basic (or no) authentication.
	/// </summary>
	public class HttpServer : IDisposable
	{
		Utf8TcpServer m_Server;
		Dictionary<string, List<string>> m_HttpData = new Dictionary<string, List<string>>();
		Dictionary<string, HttpResource> m_Resources = new Dictionary<string, HttpResource>();
		object m_Lock = new object();

		const string ERROR_TEMPLATE = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\"><html><head><title>{0}</title></head><body><h1>{0}</h1>{1}<hr><address>MoonSharp Remote Debugger / {2}</address></body></html><!-- This padding is added to bring the error message over 512 bytes to avoid some browsers custom errors. This padding is added to bring the error message over 512 bytes to avoid some browsers custom errors. This padding is added to bring the error message over 512 bytes to avoid some browsers custom errors. This padding is added to bring the error message over 512 bytes to avoid some browsers custom errors. This padding is added to bring the error message over 512 bytes to avoid some browsers custom errors. -->";

		static readonly string VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		readonly string ERROR_401 = string.Format(ERROR_TEMPLATE, "401 Unauthorized", "Please login.", VERSION);
		readonly string ERROR_404 = string.Format(ERROR_TEMPLATE, "404 Not Found", "The specified resource cannot be found.", VERSION);
		readonly string ERROR_500 = string.Format(ERROR_TEMPLATE, "500 Internal Server Error", "An internal server error occurred.", VERSION);

		public HttpServer(int port, Utf8TcpServerOptions options)
		{
			m_Server = new Utf8TcpServer(port, 100 << 10, '\n', options);
			m_Server.DataReceived += OnDataReceivedAny;
			m_Server.ClientDisconnected += OnClientDisconnected;
		}

		public Func<string, string, bool> Authenticator { get; set; }

		public void Start()
		{
			m_Server.Start();
		}

		void OnDataReceivedAny(object sender, Utf8TcpPeerEventArgs e)
		{
			lock (m_Lock)
			{
				List<string> httpdata;

				string msg = e.Message.Replace("\n", "").Replace("\r", "");

				if (!m_HttpData.TryGetValue(e.Peer.Id, out httpdata))
				{
					httpdata = new List<string>();
					m_HttpData.Add(e.Peer.Id, httpdata);
				}

				if (msg.Length == 0)
				{
					ExecHttpRequest(e.Peer, httpdata);
					e.Peer.Disconnect();
				}
				else
				{
					httpdata.Add(msg);
				}
			}
		}

		private void SendHttp(Utf8TcpPeer peer, string responseCode, string contentType, string data, params string[] extraHeaders)
		{
			SendHttp(peer, responseCode, contentType, Encoding.UTF8.GetBytes(data), extraHeaders);
		}


		private void SendHttp(Utf8TcpPeer peer, string responseCode, string contentType, byte[] data, params string[] extraHeaders)
		{
			peer.Send("HTTP/1.0 {0}", responseCode);
			peer.Send("Server: moonsharp-remote-debugger/{0}", VERSION);
			peer.Send("Content-Type: {0}", contentType);
			peer.Send("Content-Length: {0}", data.Length);
			peer.Send("Connection: close");
			peer.Send("Cache-Control: max-age=0, no-cache");

			foreach (string h in extraHeaders)
				peer.Send(h);

			peer.Send("");
			peer.SendBinary(data);
		}


		private void ExecHttpRequest(Utf8TcpPeer peer, List<string> httpdata)
		{
			try
			{
				if (Authenticator != null)
				{
					string authstr = httpdata.FirstOrDefault(s => s.StartsWith("Authorization:"));
					bool authorized = false;

					if (authstr != null)
					{
						string user, password;
						ParseAuthenticationString(authstr, out user, out password);
						authorized = Authenticator(user, password);
					}

					if (!authorized)
					{
						SendHttp(peer, "401 Not Authorized", "text/html", ERROR_401, "WWW-Authenticate: Basic realm=\"moonsharp-remote-debugger\"");
						return;
					}
				}

				HttpResource res = GetResourceFromPath(httpdata[0]);

				if (res == null)
				{
					SendHttp(peer, "404 Not Found", "text/html", ERROR_404);
				}
				else
				{
					SendHttp(peer, "200 OK", res.GetContentTypeString(), res.Data);
				}
			}
			catch (Exception ex)
			{
				m_Server.Logger(ex.Message);

				try
				{
					SendHttp(peer, "500 Internal Server Error", "text/html", ERROR_500);
				}
				catch (Exception ex2)
				{
					m_Server.Logger(ex2.Message);
				}
			}
		}

		private void ParseAuthenticationString(string authstr, out string user, out string password)
		{
			// example: Authorization: Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
			user = null; password = null;

			string[] parts = authstr.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length < 3)
				return;

			if (parts[1] != "Basic")
				return;

			byte[] credentialBytes = Convert.FromBase64String(parts[2]);
			string credentialString = Encoding.UTF8.GetString(credentialBytes);
			string[] credentials = credentialString.Split(new char[] { ':' }, 2);

			if (credentials.Length != 2)
				return;

			user = credentials[0];
			password = credentials[1];
		}


		private HttpResource GetResourceFromPath(string path)
		{
			string[] parts = path.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length < 2)
				return null;

			if (parts[0] != "GET")
				return null;

			string uri = parts[1];

			if (!uri.Contains('?'))
			{
				return GetResourceFromUri(uri, null);
			}
			else
			{
				string[] macroparts = uri.Split(new char[] { '?' }, 2);
				uri = macroparts[0];
				string[] tuples = macroparts[1].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

				Dictionary<string, string> args = new Dictionary<string, string>();
				foreach (string t in tuples)
				{
					ParseArgument(t, args);
				}

				return GetResourceFromUri(uri, args);
			}
		}

		private void ParseArgument(string t, Dictionary<string, string> args)
		{
			string[] parts = t.Split(new char[] { '=' }, 2);

			if (parts.Length == 2)
				args.Add(parts[0], parts[1]);
			else
				args.Add(t, null);
		}

		private HttpResource GetResourceFromUri(string uri, Dictionary<string, string> args)
		{
			if (uri != "/")
				uri = uri.TrimEnd('/');

			HttpResource ret;
			if (m_Resources.TryGetValue(uri, out ret))
			{
				if (ret.Type == HttpResourceType.Callback)
				{
					if (args == null) args = new Dictionary<string, string>();
					args.Add("?", uri);
					return ret.Callback(args);
				}
				else
				{
					return ret;
				}
			}

			return null;
		}




		void OnClientDisconnected(object sender, Utf8TcpPeerEventArgs e)
		{
			lock (m_Lock)
			{
				if (m_HttpData.ContainsKey(e.Peer.Id))
					m_HttpData.Remove(e.Peer.Id);
			}
		}

		/// <summary>
		/// Registers the resource.
		/// </summary>
		/// <param name="path">The path, including a starting '/'.</param>
		/// <param name="resource">The resource.</param>
		public void RegisterResource(string path, HttpResource resource)
		{
			m_Resources.Add(path, resource);
		}

        public void Dispose()
        {
            m_Server.Dispose();
        }
    }
}
