using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using System;
using System.Reflection;
using System.Linq;

public class CustomTest1Behaviour : MonoBehaviour {

    public class MyClass
    {
        public string MyMethod()
        {
            return DateTime.Now.ToString("u");
        }
    }

    void RegisterNamespace(string ns)
    {
        //string @namespace = "System";

        //var q = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.Namespace == @namespace).ToList();

        //q.ForEach(t => UserData.RegisterType(t));
    }

	// Use this for initialization
	void Start () 
    {
       /* RegisterNamespace("System");


        UserData.RegisterType<MyClass>();


        Script S = new Script();
        S.Options.DebugPrint = s => Debug.Log(s);

        S.Globals["obj"] = new MyClass();

        S.DoString("print (obj.myMethod());");

        Debug.Log("CUSTOM TEST 1 - DONE"); */
	}
	
	// Update is called once per frame
	void Update () {
        string[] arr = (new string[] { "abc", "XY", "CDE", "ijk" }).OrderBy(s => s).ToArray();
	}

    void OnGUI()
    {
        string[] arr = (new string[] { "abc", "XY", "CDE", "ijk" }).OrderBy(s => s).ToArray();
        string text = string.Join(", ", arr);

        string banner = string.Format("MoonSharp Test Runner {0} [{1}]", Script.VERSION, Script.GlobalOptions.Platform.GetPlatformName());

        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), banner);
        GUI.TextArea(new Rect(0, 30, Screen.width, Screen.height - 30), text);
    }

}
