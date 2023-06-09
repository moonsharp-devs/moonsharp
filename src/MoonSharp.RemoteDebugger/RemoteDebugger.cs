using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.RemoteDebugger.Network;

namespace MoonSharp.RemoteDebugger
{
	public class RemoteDebuggerService : IDisposable
	{
		RemoteDebuggerOptions m_Options;
		DebugWebHost m_HttpServer;
		string m_JumpPage;
		int m_RpcPortMax;
		List<DebugServer> m_DebugServers = new List<DebugServer>();

		object m_Lock = new object();


		public RemoteDebuggerService()
			: this(RemoteDebuggerOptions.Default)
		{ }

		public RemoteDebuggerService(RemoteDebuggerOptions options)
		{
			m_Options = options;

			if (options.HttpPort.HasValue)
			{
				Utf8TcpServerOptions httpopts = options.NetworkOptions & (~Network.Utf8TcpServerOptions.SingleClientOnly);
				m_HttpServer = new DebugWebHost(options.HttpPort.Value, httpopts);

				if (options.SingleScriptMode)
				{
					m_HttpServer.RegisterResource("/", HttpResource.CreateText(HttpResourceType.Html,
						string.Format("<html><body><iframe height='100%' width='100%' src='Debugger?port={0}'>Please follow <a href='{0}'>link</a>.</iframe></body></html>", options.RpcPortBase)));
				}
				else
				{
					m_JumpPage = m_HttpServer.GetJumpPageText();

					m_HttpServer.RegisterResource("/", HttpResource.CreateCallback(GetJumpPageData));
				}

				m_HttpServer.Start();
			}

			m_RpcPortMax = options.RpcPortBase;
		}

		private HttpResource GetJumpPageData(Dictionary<string, string> arg)
		{
			lock (m_Lock)
			{
				return HttpResource.CreateText(HttpResourceType.Html,
					string.Format(m_JumpPage, GetJumpHtmlFragment()));
			}
		}

		public void Attach(Script S, string scriptName, bool freeRunAfterAttach = false)
		{
			lock (m_Lock)
			{
				DebugServer d = new DebugServer(scriptName, S, m_RpcPortMax, m_Options.NetworkOptions, freeRunAfterAttach);
				S.AttachDebugger(d);
				m_DebugServers.Add(d);
			}
		}

		public string GetJumpHtmlFragment()
		{
			StringBuilder sb = new StringBuilder();
			lock (m_Lock)
			{
				foreach(DebugServer d in m_DebugServers)
				{
					sb.AppendFormat("<tr><td><a href=\"Debugger?port={0}\">{1}</a></td><td>{2}</td><td>{3}</td><td>{0}</td></tr>\n",
						d.Port, d.AppName, d.GetState(), d.ConnectedClients());
				}
			}
			return sb.ToString();
		}

        public void Dispose()
        {
            m_HttpServer.Dispose();
            m_DebugServers.ForEach(s => s.Dispose());
        }

        public string HttpUrlStringLocalHost
		{
			get
			{
				if (m_HttpServer != null)
				{
					return string.Format("http://127.0.0.1:{0}/", m_Options.HttpPort.Value);
				}
				return null;
			}
		}
	}
}
