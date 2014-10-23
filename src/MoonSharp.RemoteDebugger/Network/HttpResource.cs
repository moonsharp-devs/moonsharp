using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.RemoteDebugger.Network
{
	public class HttpResource
	{
		public HttpResourceType Type { get; private set; }
		public byte[] Data { get; private set; }
		public Func<Dictionary<string, string>, HttpResource> Callback { get; private set; }

		private HttpResource() { }

		public static HttpResource CreateBinary(HttpResourceType type, byte[] data)
		{
			return new HttpResource()
			{
				Type = type,
				Data = data
			};
		}

		public static HttpResource CreateBinary(HttpResourceType type, string base64data)
		{
			return new HttpResource()
			{
				Type = type,
				Data = Convert.FromBase64String(base64data)
			};
		}

		public static HttpResource CreateText(HttpResourceType type, string data)
		{
			return new HttpResource()
			{
				Type = type,
				Data = Encoding.UTF8.GetBytes(data)
			};
		}

		public static HttpResource CreateCallback(Func<Dictionary<string, string>, HttpResource> callback)
		{
			return new HttpResource()
			{
				Type = HttpResourceType.Callback,
				Callback = callback
			};
		}



		public string GetContentTypeString()
		{
			switch (Type)
			{
				case HttpResourceType.PlainText:
					return "text/plain";
				case HttpResourceType.Html:
					return "text/html";
				case HttpResourceType.Json:
					return "application/json";
				case HttpResourceType.Xml:
					return "application/xml";
				case HttpResourceType.Jpeg:
					return "image/jpeg";
				case HttpResourceType.Png:
					return "image/png";
				case HttpResourceType.Binary:
					return "application/octet-stream";
				case HttpResourceType.Callback:
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
