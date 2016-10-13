using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SynchProjects
{
	class Program
	{
		static string BASEPATH;

		static void CopyCompileFilesAsLinks(string platformName, string srcCsProj, string platformDest, string pathPrefix)
		{
			platformName = AdjustBasePath(platformName);
			srcCsProj = AdjustBasePath(srcCsProj);
			platformDest = AdjustBasePath(platformDest);
			pathPrefix = AdjustBasePath(pathPrefix);

			string dstCsProj = string.Format(platformDest, platformName);
			try
			{
				int warningCount = 0;
				const string XMLNS = "http://schemas.microsoft.com/developer/msbuild/2003";
				HashSet<string> linksDone = new HashSet<string>();

				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine("Synch vsproj compiles {0} ...", Path.GetFileNameWithoutExtension(dstCsProj));

				XmlDocument xsrc = new XmlDocument();
				XmlDocument xdst = new XmlDocument();

				xsrc.Load(srcCsProj);
				xdst.Load(dstCsProj);

				XmlNamespaceManager sxns = new XmlNamespaceManager(xsrc.NameTable);
				XmlNamespaceManager dxns = new XmlNamespaceManager(xdst.NameTable);

				sxns.AddNamespace("ms", XMLNS);
				dxns.AddNamespace("ms", XMLNS);

				XmlElement srccont = xsrc.SelectSingleNode("/ms:Project/ms:ItemGroup[count(ms:Compile) != 0]", sxns) as XmlElement;
				XmlElement dstcont = xdst.SelectSingleNode("/ms:Project/ms:ItemGroup[count(ms:Compile) != 0]", dxns) as XmlElement;

				// dirty hack
				dstcont.InnerXml = srccont.InnerXml;

				List<XmlElement> toRemove = new List<XmlElement>();

				foreach (XmlElement xe in dstcont.ChildNodes.OfType<XmlElement>())
				{
					string file = xe.GetAttribute("Include");
					string link = Path.GetFileName(file);

					if (link.Contains(".g4"))
					{
						toRemove.Add(xe);
						continue;
					}

					if (!linksDone.Add(link))
					{
						++warningCount;
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("\t[WARNING] - Duplicate file: {0}", link);
					}

					file = pathPrefix + file;

					xe.SetAttribute("Include", file);

					XmlElement xlink = xe.OwnerDocument.CreateElement("Link", XMLNS);
					xlink.InnerText = link;
					xe.AppendChild(xlink);
				}

				foreach (XmlElement xe in toRemove)
					xe.ParentNode.RemoveChild(xe);

				xdst.Save(dstCsProj);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\t[DONE] ({0} warnings)", warningCount);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("\t[ERROR] - {0}", ex.Message);
			}

			Console.WriteLine("\n");
		}

		private static string AdjustBasePath(string str)
		{
			return str.Replace("{BASEPATH}", BASEPATH);
		}

		static void Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("********************************************************");
			Console.WriteLine("* !! REMEMBER TO RSYNC UNITY AND .NET CORE PROJECTS !! *");
			Console.WriteLine("********************************************************");


			const string INTERPRETER_PROJECT = @"{BASEPATH}\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj";
			const string INTERPRETER_SUBPROJECTS_PATHS = @"{BASEPATH}\MoonSharp.Interpreter\_Projects\MoonSharp.Interpreter.{0}\MoonSharp.Interpreter.{0}.csproj";
			const string INTERPRETER_PATH_PREFIX = @"..\..\";

			const string DEBUGGER_PROJECT = @"{BASEPATH}\MoonSharp.RemoteDebugger\MoonSharp.RemoteDebugger.net35-client.csproj";
			const string DEBUGGER_SUBPROJECTS_PATHS = @"{BASEPATH}\MoonSharp.RemoteDebugger\_Projects\MoonSharp.RemoteDebugger.{0}\MoonSharp.RemoteDebugger.{0}.csproj";
			const string DEBUGGER_PATH_PREFIX = @"..\..\";

			const string VSCODEDEBUGGER_PROJECT = @"{BASEPATH}\MoonSharp.VsCodeDebugger\MoonSharp.VsCodeDebugger.net35-client.csproj";
			const string VSCODEDEBUGGER_SUBPROJECTS_PATHS = @"{BASEPATH}\MoonSharp.VsCodeDebugger\_Projects\MoonSharp.VsCodeDebugger.{0}\MoonSharp.VsCodeDebugger.{0}.csproj";
			const string VSCODEDEBUGGER_PATH_PREFIX = @"..\..\";

			const string TESTS_PROJECT = @"{BASEPATH}\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35-client.csproj";
			const string TESTS_SUBPROJECTS_PATHS = @"{BASEPATH}\MoonSharp.Interpreter.Tests\_Projects\MoonSharp.Interpreter.Tests.{0}\MoonSharp.Interpreter.Tests.{0}.csproj";
			const string TESTS_PATH_PREFIX = @"..\..\";

			string[] INTERPRETER_PLATFORMS = new string[] { "net40-client", "portable40" };
			string[] DEBUGGER_PLATFORMS = new string[] { "net40-client" };
			string[] VSCODEDEBUGGER_PLATFORMS = new string[] { "net40-client" };
			string[] TESTS_PLATFORMS = new string[] { "net40-client", "portable40", "Embeddable.portable40" };

			CalcBasePath();

			foreach (string platform in INTERPRETER_PLATFORMS)
				CopyCompileFilesAsLinks(platform, INTERPRETER_PROJECT, INTERPRETER_SUBPROJECTS_PATHS, INTERPRETER_PATH_PREFIX);

			foreach (string platform in DEBUGGER_PLATFORMS)
				CopyCompileFilesAsLinks(platform, DEBUGGER_PROJECT, DEBUGGER_SUBPROJECTS_PATHS, DEBUGGER_PATH_PREFIX);

			foreach (string platform in VSCODEDEBUGGER_PLATFORMS)
				CopyCompileFilesAsLinks(platform, VSCODEDEBUGGER_PROJECT, VSCODEDEBUGGER_SUBPROJECTS_PATHS, VSCODEDEBUGGER_PATH_PREFIX);

			foreach (string platform in TESTS_PLATFORMS)
				CopyCompileFilesAsLinks(platform, TESTS_PROJECT, TESTS_SUBPROJECTS_PATHS, TESTS_PATH_PREFIX);


			Console.ReadLine();
		}

		private static void CalcBasePath()
		{
			string path = "";
			string[] dir = AppDomain.CurrentDomain.BaseDirectory.Split('\\');

			for (int i = 0; i < dir.Length; i++)
			{
				if (dir[i].ToLower() == "devtools")
					break;

				if (path.Length > 0)
					path = path + "\\" + dir[i];
				else
					path = dir[i];
			}

			BASEPATH = path;
		}
	}
}
