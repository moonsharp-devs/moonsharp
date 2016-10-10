using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using System.IO;
using MoonSharp.Interpreter.Serialization;

public class UnityTests : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        UserData.RegisterType<UnityEngine.GUI>();

        Table dump = UserData.GetDescriptionOfRegisteredTypes(true);
        File.WriteAllText(@"/temp/unitydump.lua", dump.Serialize());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
