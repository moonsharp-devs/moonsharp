using System;
using MoonSharp.Interpreter.Loaders;
using Android.Content.Res;
using System.IO;

namespace XamarinTestBed_Android
{
	public class XamarinLoader : ClassicLuaScriptLoader
	{
		AssetManager m_Assets;

		public XamarinLoader (AssetManager assets)
		{
			m_Assets = assets;
		}

		protected override bool FileExists (string file)
		{
			try
			{
				file = Path.GetFileName(file);

				Stream input = m_Assets.Open (file);
				input.Close();
				return true;
			}
			catch 
			{
				return false;
			}
		}

		public override string LoadFile (string file, MoonSharp.Interpreter.Table globalContext)
		{
			file = Path.GetFileName(file.Replace('\\', '/'));

			System.Diagnostics.Debug.WriteLine ("Attempting to load " + file);

			using(Stream input = m_Assets.Open (file))
				using(StreamReader reader = new StreamReader (input))
					return reader.ReadToEnd();
		}



	}
}

