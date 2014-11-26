using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

class UnityLoader : ClassicLuaScriptLoader
{
	private Dictionary<string,string> m_Resources;

	public UnityLoader()
	{
		m_Resources = Resources.LoadAll("Scripts/TestMore", typeof(TextAsset))
			.OfType<TextAsset>()
			.ToDictionary(r => r.name, r => r.text);

		//foreach (string k in m_Resources.Keys)
		//	Debug.Log("I have : " + k);

		//Debug.Log("Total Resources : " + m_Resources.Count);
	}

	private string GetFileName(string filename)
	{
		int b = Math.Max(filename.LastIndexOf('\\'), filename.LastIndexOf('/'));

		if (b > 0)
			filename = filename.Substring(b + 1);

		filename = filename.Replace(".t", "");
		filename = filename.Replace(".lua", "");

		return filename;
	}

	public override string LoadFile(string file, Table globalContext)
	{
		file = GetFileName(file);

		if (m_Resources.ContainsKey(file))
			return m_Resources[file];
		else
		{
			Debug.LogError("UnityLoader.LoadFile : Cannot load " + file);
			throw new Exception("UnityLoader.LoadFile : Cannot load " + file);
		}
	}

	protected override bool FileExists(string file)
	{
		file = GetFileName(file);
		return m_Resources.ContainsKey(file);
	}

	public override string ResolveFileName(string filename, Table globalContext)
	{
		return base.ResolveFileName(filename, globalContext);
	}
}
