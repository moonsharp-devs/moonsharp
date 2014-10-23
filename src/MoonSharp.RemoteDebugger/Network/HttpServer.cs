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
	/// This is a very very simplified and light http server. It exists to run on platforms where 
	/// more standard methods offered by .NET BCL are not available and/or they require special priviledges
	/// on the machine. 
	/// This supports only GET method and basic (or no) authentication.
	/// </summary>
	public class HttpServer
	{
		Utf8TcpServer m_Server;
		Dictionary<string, List<string>> m_HttpData = new Dictionary<string, List<string>>();
		Dictionary<string, HttpResource> m_Resources = new Dictionary<string, HttpResource>();

		object m_Lock = new object();

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
						peer.Send("HTTP/1.0 401 Not Authorized");
						peer.Send("Server: moonsharp-remote-debugger/{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
						peer.Send("Content-Type: text; charset=utf-8");
						peer.Send("Content-Length: 0");
						peer.Send("Connection: close");
						peer.Send("WWW-Authenticate: Basic realm=\"moonsharp-remote-debugger\"");
						peer.Send("Cache-Control: max-age=0, no-cache");
						peer.Send("");
						return;
					}
				}

				HttpResource res = GetResourceFromPath(httpdata[0]);

				if (res == null)
				{
					peer.Send("HTTP/1.0 404 Not Found");
					peer.Send("Server: moonsharp-remote-debugger/{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
					peer.Send("Content-Type: text; charset=utf-8");
					peer.Send("Content-Length: 0");
					peer.Send("Connection: close");
					peer.Send("Cache-Control: max-age=0, no-cache");
					peer.Send("");
				}
				else
				{
					peer.Send("HTTP/1.0 200 OK");
					peer.Send("Server: moonsharp-remote-debugger/{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
					peer.Send("Content-Type: {0}", res.GetContentTypeString());
					peer.Send("Content-Length: {0}", res.Data.Length);
					peer.Send("Connection: close");
					peer.Send("Cache-Control: max-age=0, no-cache");
					peer.Send("");
					peer.SendBinary(res.Data);
				}
			}
			catch (Exception ex)
			{
				m_Server.Logger(ex.Message);

				try
				{
					peer.Send("HTTP/1.0 500 Internal Server Error");
					peer.Send("Server: moonsharp-remote-debugger/{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString());
					peer.Send("Content-Type: text; charset=utf-8");
					peer.Send("Content-Length: 0");
					peer.Send("Connection: close");
					peer.Send("Cache-Control: max-age=0, no-cache");
					peer.Send("");
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



	}
}
