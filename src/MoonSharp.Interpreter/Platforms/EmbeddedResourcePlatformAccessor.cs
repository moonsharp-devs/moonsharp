using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	/// <summary>
	/// A Platform Accessor giving no support for 'io' and 'os' modules, and loading scripts from 
	/// an assembly resources.
	/// </summary>
	public class EmbeddedResourcePlatformAccessor : LimitedPlatformAccessorBase
	{
		Assembly m_ResourceAssembly;
		HashSet<string> m_ResourceNames;
		string m_Namespace;

		/// <summary>
		/// Initializes a new instance of the <see cref="EmbeddedResourcePlatformAccessor"/> class.
		/// </summary>
		/// <param name="resourceAssembly">The assembly containing the scripts as embedded resources.</param>
		public EmbeddedResourcePlatformAccessor(Assembly resourceAssembly)
		{
			m_ResourceAssembly = resourceAssembly;
			m_Namespace = m_ResourceAssembly.FullName.Split(',').First();
			m_ResourceNames = new HashSet<string>(m_ResourceAssembly.GetManifestResourceNames());
		}

		private string FileNameToResource(string file)
		{
			file = file.Replace('/', '.');
			file = file.Replace('\\', '.');
			return m_Namespace + "." + file;
		}

		/// <summary>
		/// Checks if a script file exists.
		/// </summary>
		/// <param name="name">The script filename.</param>
		/// <returns></returns>
		public override bool ScriptFileExists(string name)
		{
			name = FileNameToResource(name);
			return m_ResourceNames.Contains(name);
		}

		/// <summary>
		/// Opens a file for reading the script code.
		/// It can return either a string, a byte[] or a Stream.
		/// If a byte[] is returned, the content is assumed to be a serialized (dumped) bytecode. If it's a string, it's
		/// assumed to be either a script or the output of a string.dump call. If a Stream, autodetection takes place.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="file">The file.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns>
		/// A string, a byte[] or a Stream.
		/// </returns>
		public override object OpenScriptFile(Script script, string file, Table globalContext)
		{
			file = FileNameToResource(file);
			return m_ResourceAssembly.GetManifestResourceStream(file);
		}

		/// <summary>
		/// Gets the platform name prefix
		/// </summary>
		/// <returns></returns>
		public override string GetPlatformNamePrefix()
		{
			return "resource";
		}

		/// <summary>
		/// Default handler for 'print' calls. Can be customized in ScriptOptions
		/// </summary>
		/// <param name="content">The content.</param>
		public override void DefaultPrint(string content)
		{
			System.Diagnostics.Debug.WriteLine(content);
		}
	}
}
