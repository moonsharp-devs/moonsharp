//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using MoonSharp.Interpreter.RuntimeAbstraction;

//namespace MoonSharp.Interpreter.Loaders
//{
//	public class ClassicLuaScriptLoader : IFileSystemAccessor
//	{
//		string[] m_EnvironmentPaths = null;
		
//		public ClassicLuaScriptLoader()
//		{
//			string env = Platform.Current.GetEnvironmentVariable("MOONSHARP_PATH");
//			if (!string.IsNullOrEmpty(env)) m_EnvironmentPaths = UnpackStringPaths(env);

//			if (m_EnvironmentPaths == null)
//			{
//				env = Platform.Current.GetEnvironmentVariable("LUA_PATH");
//				if (!string.IsNullOrEmpty(env)) m_EnvironmentPaths = UnpackStringPaths(env);
//			}

//			if (m_EnvironmentPaths == null)
//			{
//				m_EnvironmentPaths = UnpackStringPaths("?;?.lua");
//			}
//		}

//		public virtual object LoadFile(string file, Table globalContext)
//		{
//			var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
//			return stream;
//		}

//		public virtual string ResolveFileName(string filename, Table globalContext)
//		{
//			return filename;
//		}

//		public string[] ModulePaths { get; set; }

//		public virtual string ResolveModuleName(string modname, Table globalContext)
//		{
//			if (ModulePaths != null)
//				return ResolveModuleName(modname, ModulePaths);

//			DynValue s = globalContext.RawGet("LUA_PATH");

//			if (s != null && s.Type == DataType.String)
//				return ResolveModuleName(modname, UnpackStringPaths(s.String));

//			return ResolveModuleName(modname, m_EnvironmentPaths);
//		}

//		public virtual string[] UnpackStringPaths(string str)
//		{
//			return str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
//				.Select(s => s.Trim())
//				.Where(s => !string.IsNullOrEmpty(s))
//				.ToArray();
//		}

//		protected virtual string ResolveModuleName(string modname, string[] paths)
//		{
//			modname = modname.Replace('.', '/');

//			foreach (string path in paths)
//			{
//				string file = path.Replace("?", modname);
//				if (FileExists(file))
//					return file;
//			}

//			return null;
//		}

//		protected virtual bool FileExists(string file)
//		{
//			return File.Exists(file);
//		}

//	}
//}
