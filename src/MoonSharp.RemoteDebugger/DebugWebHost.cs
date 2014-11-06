using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.RemoteDebugger.Network;

namespace MoonSharp.RemoteDebugger
{
	public class DebugWebHost : HttpServer
	{
		public DebugWebHost(int port, Utf8TcpServerOptions options)
			: base(port, options)
		{
			RegisterEmbeddedResource("Main.html", HttpResourceType.Html, "Debugger");
			RegisterEmbeddedResource("Main.swf", HttpResourceType.Binary);
			RegisterEmbeddedResource("playerProductInstall.swf", HttpResourceType.Binary);
			RegisterEmbeddedResource("swfobject.js", HttpResourceType.PlainText);
			
			RegisterEmbeddedResource("bootstrap.min.css", HttpResourceType.Css);
			RegisterEmbeddedResource("theme.css", HttpResourceType.Css);
			RegisterEmbeddedResource("moonsharpdbg.png", HttpResourceType.Png);
			RegisterEmbeddedResource("bootstrap.min.js", HttpResourceType.Javascript);
			RegisterEmbeddedResource("jquery.min.js", HttpResourceType.Javascript);
		}

		private HttpResource RegisterEmbeddedResource(string resourceName, HttpResourceType type, string urlName = null)
		{
			urlName = urlName ?? resourceName;

			byte[] data = GetResourceData(resourceName);

			HttpResource r = HttpResource.CreateBinary(type, data);
			RegisterResource("/" + urlName, r);
			RegisterResource(urlName, r);
			return r;
		}

		private byte[] GetResourceData(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream("MoonSharp.RemoteDebugger.Resources." + resourceName))
			{
				byte[] data = new byte[stream.Length];
				stream.Read(data, 0, data.Length);
				return data;
			}
		}

		public string GetJumpPageText()
		{
			byte[] data = GetResourceData("JumpPage.html");
			return Encoding.UTF8.GetString(data);
		}




	}
}
