using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.RemoteDebugger.Network;
using MoonSharp.RemoteDebugger.Threading;

namespace MoonSharp.RemoteDebugger
{
	public class DebugServer : IDebugger
	{
		List<string> m_Watches = new List<string>();
		Utf8TcpServer m_Server;


		public DebugServer(int port, bool localOnly)
		{
			m_Server = new Utf8TcpServer(port, 1 << 20, '\0', localOnly ? Utf8TcpServerOptions.LocalHostOnly : Utf8TcpServerOptions.Default);
			m_Server.Start();
			m_Server.DataReceived += m_Server_DataReceived;
		}

		#region Writes


		public void SetSourceCode(SourceCode sourceCode)
		{
			Send(xw =>
			{
				using (xw.Element("source-code"))
				{
					xw.Attribute("id", sourceCode.SourceID)
						.Attribute("name", sourceCode.Name);

					foreach (string line in sourceCode.Lines)
						xw.Element("l", line);
				}
			});
		}


		private void Send(Action<XmlWriter> a)
		{
			XmlWriterSettings xs = new XmlWriterSettings()
			{
				CheckCharacters = true,
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Fragment,
				Encoding = Encoding.UTF8,
				Indent = false,
			};

			StringBuilder sb = new StringBuilder();
			XmlWriter xw = XmlWriter.Create(sb, xs);

			a(xw);

			string xml = sb.ToString();
			m_Server.BroadcastMessage(xml);
			Console.WriteLine(xml);
		}


		public void Update(WatchType watchType, List<WatchItem> items)
		{
			Send(xw =>
			{
				using (xw.Element("watches"))
				{
					xw.Attribute("type", watchType);

					foreach (WatchItem wi in items)
					{
						using (xw.Element("watch"))
						{
							xw.Attribute("name", wi.Name);
							xw.Attribute("value", wi.Value);
							xw.Attribute("address", wi.Address);
							xw.Attribute("baseptr", wi.BasePtr);
							xw.Attribute("lvalue", wi.LValue);
							xw.Attribute("retaddress", wi.RetAddress);
						}
					}
				}
			});
		}

		public void SetByteCode(string[] byteCode)
		{
			Send(xw =>
				{
					using (xw.Element("bytecode"))
					{
						foreach (string line in byteCode)
							xw.Element("l", line);
					}
				});
		}

		#endregion

		BlockingQueue<DebuggerAction> m_QueuedActions = new BlockingQueue<DebuggerAction>();
		SourceRef m_LastSentSourceRef = null;

		public void QueueAction(DebuggerAction action)
		{
			m_QueuedActions.Enqueue(action);
		}

		public DebuggerAction GetAction(int ip, SourceRef sourceref)
		{
			if (sourceref != m_LastSentSourceRef)
			{
				Send(xw =>
					{
						using (xw.Element("source-loc"))
						{
							xw.Attribute("srcid", sourceref.SourceIdx)
								.Attribute("cf", sourceref.FromChar)
								.Attribute("ct", sourceref.ToChar)
								.Attribute("lf", sourceref.FromLine)
								.Attribute("lt", sourceref.ToLine);
						}
					});
			}

			return m_QueuedActions.Dequeue();
		}

		void m_Server_DataReceived(object sender, Utf8TcpPeerEventArgs e)
		{
			throw new NotImplementedException();
		}


		public List<string> GetWatchItems()
		{
			return m_Watches;
		}


		public bool IsPauseRequested()
		{
			return true;
		}
	}
}
