using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SynchProjects
{
	class Program
	{
		static void CopyCompileFilesAsLinks(string platformName, string srcCsProj, string platformDest, string pathPrefix)
		{
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

		static void Main(string[] args)
		{
			const string INTERPRETER_PROJECT = @"C:\git\moonsharp\src\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj";
			const string INTERPRETER_SUBPROJECTS_PATHS = @"C:\git\moonsharp\src\MoonSharp.Interpreter\_Projects\MoonSharp.Interpreter.{0}\MoonSharp.Interpreter.{0}.csproj";
			const string INTERPRETER_PATH_PREFIX = @"..\..\";

			const string TESTS_PROJECT = @"C:\git\moonsharp\src\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35-client.csproj";
			const string TESTS_SUBPROJECTS_PATHS = @"C:\git\moonsharp\src\MoonSharp.Interpreter.Tests\_Projects\MoonSharp.Interpreter.Tests.{0}\MoonSharp.Interpreter.Tests.{0}.csproj";
			const string TESTS_PATH_PREFIX = @"..\..\";

			string[] PLATFORMS = new string[] { "net40-client", "portable40" };

			foreach (string platform in PLATFORMS)
				CopyCompileFilesAsLinks(platform, INTERPRETER_PROJECT, INTERPRETER_SUBPROJECTS_PATHS, INTERPRETER_PATH_PREFIX);

			foreach (string platform in PLATFORMS)
				CopyCompileFilesAsLinks(platform, TESTS_PROJECT, TESTS_SUBPROJECTS_PATHS, TESTS_PATH_PREFIX);


#if false
			//****************************************************************************
			//** UNIT TESTS
			//****************************************************************************

			// Tests - net40
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.net40\MoonSharp.Interpreter.Tests.net40.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - portable-net40
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.portable-net40\MoonSharp.Interpreter.Tests.portable-net40.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.net45\MoonSharp.Interpreter.Tests.net45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - EXTERNAL portable-net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.External.portable-net45\MoonSharp.Interpreter.Tests.External.portable-net45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - portable-net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.portable-net45\MoonSharp.Interpreter.Tests.portable-net45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - netcore45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.netcore45\MoonSharp.Interpreter.Tests.netcore45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			//

#endif 
			Console.ReadLine();
		}
	}
}
