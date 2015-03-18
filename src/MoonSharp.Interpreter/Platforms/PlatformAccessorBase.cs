using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	public abstract class PlatformAccessorBase : IPlatformAccessor
	{
		private string[] m_EnvironmentPaths = null;
		private bool? m_IsAOT = null;

		public PlatformAccessorBase()
		{ }

		public PlatformAccessorBase(string[] modulePaths)
		{
			m_EnvironmentPaths = modulePaths;
		}



		protected virtual string ResolveModuleName(string modname, string[] paths)
		{
			modname = modname.Replace('.', '/');

			foreach (string path in paths)
			{
				string file = path.Replace("?", modname);

				if (FileExists(file))
					return file;
			}

			return null;
		}

		public string ResolveModuleName(Script script, string modname, Table globalContext)
		{
			DynValue s = (globalContext ?? script.Globals).RawGet("LUA_PATH");

			if (s != null && s.Type == DataType.String)
				return ResolveModuleName(modname, ScriptOptions.UnpackStringPaths(s.String));

			return ResolveModuleName(modname, script.Options.ModulesPaths);
		}


		public virtual CoreModules FilterSupportedCoreModules(CoreModules module)
		{
			return module;
		}

		public virtual string GetEnvironmentVariable(string envvarname)
		{
			return null;
		}

		public virtual bool IsRunningOnAOT()
		{
			if (m_IsAOT.HasValue) 
				return m_IsAOT.Value;

			try
			{
				System.Linq.Expressions.Expression e = System.Linq.Expressions.Expression.Constant(5, typeof(int));
				var lambda = System.Linq.Expressions.Expression.Lambda<Func<int>>(e);
				lambda.Compile();
				m_IsAOT = false;
			}
			catch (Exception)
			{
				m_IsAOT = true;
			}

			return m_IsAOT.Value;
		}


		public abstract Stream OpenFileForIO(Script script, string filename, Encoding encoding, string mode);
		public abstract bool FileExists(string name);
		public abstract object LoadFile(Script script, string file, Table globalContext);
		public abstract Stream GetStandardStream(StandardFileType type);
		public abstract void DefaultPrint(string content);
		public abstract string DefaultInput();


		public string GetPlatformName()
		{
			return "";
		}




	}
}
