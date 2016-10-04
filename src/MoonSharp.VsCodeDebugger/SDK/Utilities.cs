/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace MoonSharp.VsCodeDebugger.SDK
{
	public class Utilities
	{
		private const string OSASCRIPT = "/usr/bin/osascript";  // osascript is the AppleScript interpreter on OS X
		private const string LINUX_TERM = "/usr/bin/gnome-terminal";    //private const string LINUX_TERM = "/usr/bin/x-terminal-emulator";
		private const string OSX_BIN_DIR = "/usr/local/bin";

		private static readonly Regex VARIABLE = new Regex(@"\{(\w+)\}");

		/*
		 * Is this Windows?
		 */
		public static bool IsWindows()
		{
			PlatformID pid = Environment.OSVersion.Platform;
			return !(pid == PlatformID.Unix || pid == PlatformID.MacOSX);
		}

		/*
		 * Is this OS X?
		 */
		public static bool IsOSX()
		{
			return File.Exists(OSASCRIPT);  // mono has no better way to determine whether this is OS X
		}

		/*
		 * Is this Linux?
		 */
		public static bool IsLinux()
		{
			return File.Exists(LINUX_TERM);  // mono has no better way to determine whether this is Linux
		}

		/*
		 * On OS X make sure that /usr/local/bin is on the PATH
		 */
		public static void FixPathOnOSX()
		{
			if (Utilities.IsOSX())
			{
				var path = System.Environment.GetEnvironmentVariable("PATH");
				if (!path.Split(':').Contains(OSX_BIN_DIR))
				{
					path += ":" + OSX_BIN_DIR;
					System.Environment.SetEnvironmentVariable("PATH", path);
				}
			}
		}

		/*
		 * Resolve hostname, dotted-quad notation for IPv4, or colon-hexadecimal notation for IPv6 to IPAddress.
		 * Returns null on failure.
		 */
		public static IPAddress ResolveIPAddress(string addressString)
		{
			try
			{
				IPAddress ipaddress = null;
				if (IPAddress.TryParse(addressString, out ipaddress))
				{
					return ipaddress;
				}

#if DNXCORE50
				IPHostEntry entry = Dns.GetHostEntryAsync(addressString).Result;
#else
				IPHostEntry entry = Dns.GetHostEntry(addressString);
#endif
				if (entry != null && entry.AddressList != null && entry.AddressList.Length > 0)
				{
					if (entry.AddressList.Length == 1)
					{
						return entry.AddressList[0];
					}
					foreach (IPAddress address in entry.AddressList)
					{
						if (address.AddressFamily == AddressFamily.InterNetwork)
						{
							return address;
						}
					}
				}
			}
			catch (Exception)
			{
				// fall through
			}

			return null;
		}

		/*
		 * Find a free socket port.
		 */
		public static int FindFreePort(int fallback)
		{
			TcpListener l = null;
			try
			{
				l = new TcpListener(IPAddress.Loopback, 0);
				l.Start();
				return ((IPEndPoint)l.LocalEndpoint).Port;
			}
			catch (Exception)
			{
				// ignore
			}
			finally
			{
				l.Stop();
			}
			return fallback;
		}

		public static string ExpandVariables(string format, object variables, bool underscoredOnly = true)
		{
			if (variables == null)
			{
				variables = new { };
			}
			Type type = variables.GetType();
			return VARIABLE.Replace(format, match => {
				string name = match.Groups[1].Value;
				if (!underscoredOnly || name.StartsWith("_"))
				{

					PropertyInfo property = type.GetProperty(name);
					if (property != null)
					{
						object value = property.GetValue(variables, null);
						return value.ToString();
					}
					return '{' + name + ": not found}";
				}
				return match.Groups[0].Value;
			});
		}

		/**
		 * converts the given absPath into a path that is relative to the given dirPath.
		 */
		public static string MakeRelativePath(string dirPath, string absPath)
		{
			if (!dirPath.EndsWith("/"))
			{
				dirPath += "/";
			}
			if (absPath.StartsWith(dirPath))
			{
				return absPath.Replace(dirPath, "");
			}
			return absPath;
			/*
			Uri uri1 = new Uri(path);
			Uri uri2 = new Uri(dir_path);
			return uri2.MakeRelativeUri(uri1).ToString();
			*/
		}
	}
}