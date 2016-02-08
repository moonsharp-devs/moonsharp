using UnityEngine;
using System.Collections;
using System.Threading;
using MoonSharp.Interpreter.Tests;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Interop;
using System;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;

public class TestRunner : MonoBehaviour
{

    public class HardwireAndLogPolicy : IRegistrationPolicy
    {
        public IUserDataDescriptor HandleRegistration(IUserDataDescriptor newDescriptor, IUserDataDescriptor oldDescriptor)
        {
            if (oldDescriptor == null && newDescriptor != null)
            {
                return newDescriptor;
            }

            return oldDescriptor;
        }

        public bool AllowTypeAutoRegistration(Type type)
        {
            return false;
        }

    }


	string m_Text = "";
	object m_Lock = new object();
	bool m_LastWasLine = true;

    Dictionary<string, string> ReadAllScripts()
    {
        Dictionary<string, string> scripts = new  Dictionary<string, string>();

        object[] result = Resources.LoadAll("MoonSharp/Scripts", typeof(TextAsset));

        foreach(TextAsset ta in result.OfType<TextAsset>())
        {
            scripts.Add(ta.name, ta.text);
        }

        return scripts;
    }

	// Use this for initialization
	void Start()
	{
        Script.DefaultOptions.ScriptLoader = new MoonSharp.Interpreter.Loaders.UnityAssetsScriptLoader(ReadAllScripts());

		Debug.Log("STARTED!");
		StartCoroutine(DoTests());
	}


	// Update is called once per frame
	void Update()
	{

	}
    // Tests skipped on all platforms
    static List<string> SKIPLIST = new List<string>()
    {
        "TestMore_308_io",  // avoid interactions with low level system
        "TestMore_309_os",  // avoid interactions with low level system
    };

    static List<string> HARDWIRE_SKIPLIST = new List<string>()
    {
        // events
        "Interop_Event_Simple",
        "Interop_Event_TwoObjects",
        "Interop_Event_Multi",
        "Interop_Event_MultiAndDetach",
        "Interop_Event_DetachAndDeregister",
        "Interop_SEvent_DetachAndDeregister",
        "Interop_SEvent_DetachAndReregister",

        // tests dependent on type dereg
        "Interop_ListMethod_None",
        "Interop_ListMethod_Lazy",
        "Interop_ListMethod_Precomputed",
        "VInterop_ListMethod_None",
        "VInterop_ListMethod_Lazy",
        "VInterop_ListMethod_Precomputed",

        // private members
        "Interop_NestedTypes_Private_Ref",
        "Interop_NestedTypes_Private_Val",

        // value type property setters
        "VInterop_IntPropertySetter_None",
        "VInterop_IntPropertySetter_Lazy",
        "VInterop_IntPropertySetter_Precomputed",
        "VInterop_NIntPropertySetter_None",
        "VInterop_NIntPropertySetter_Lazy",
        "VInterop_NIntPropertySetter_Precomputed",
        "VInterop_WoIntPropertySetter_None",
        "VInterop_WoIntPropertySetter_Lazy",
        "VInterop_WoIntPropertySetter_Precomputed",
        "VInterop_WoIntProperty2Setter_None",
        "VInterop_WoIntProperty2Setter_Lazy",
        "VInterop_WoIntProperty2Setter_Precomputed",
        "VInterop_IntPropertySetterWithSimplifiedSyntax",
        "VInterop_IntFieldSetter_None",
        "VInterop_IntFieldSetter_Lazy",
        "VInterop_IntFieldSetter_Precomputed",
        "VInterop_NIntFieldSetter_None",
        "VInterop_NIntFieldSetter_Lazy",
        "VInterop_NIntFieldSetter_Precomputed",
        "VInterop_IntFieldSetterWithSimplifiedSyntax",
    };


    // Tests skipped on AOT platforms - known not workings :(
    static List<string> AOT_SKIPLIST = new List<string>()
    {
        //"RegCollGen_List_ExtMeth_Last", 
        //"VInterop_NIntPropertySetter_None",   
        //"VInterop_NIntPropertySetter_Lazy",   
        //"VInterop_NIntPropertySetter_Precomputed",    
        //"VInterop_Overloads_NumDowncast", 
        //"VInterop_Overloads_NilSelectsNonOptional",   
        //"VInterop_Overloads_FullDecl",
        //"VInterop_Overloads_Static2",
        //"VInterop_Overloads_Cache1",
        //"VInterop_Overloads_Cache2",
        //"VInterop_ConcatMethod_None",
        //"VInterop_ConcatMethod_Lazy",
        //"VInterop_ConcatMethod_Precomputed",
        //"VInterop_ConcatMethodSemicolon_None",
        //"VInterop_ConcatMethodSemicolon_Lazy",
        //"VInterop_ConcatMethodSemicolon_Precomputed",
        //"VInterop_ConstructorAndConcatMethodSemicolon_None",
        //"VInterop_ConstructorAndConcatMethodSemicolon_Lazy",
        //"VInterop_ConstructorAndConcatMethodSemicolon_Precomputed",
    };



	IEnumerator DoTests()
	{
        MyNamespace.MyClass.Initialize();
        SKIPLIST.AddRange(HARDWIRE_SKIPLIST);
        UserData.RegistrationPolicy = new HardwireAndLogPolicy();



		MoonSharp.Interpreter.Tests.TestRunner tr = new MoonSharp.Interpreter.Tests.TestRunner(Log);

        foreach (var r in tr.IterateOnTests(null, SKIPLIST.ToArray()))
		{
			Log(r);
			yield return null;
		}
	}

	void Log(TestResult r)
	{
		if (r.Type == TestResultType.Fail)
		{
            Console_WriteLine("[FAIL] | {0} - {1} - {2}", r.TestName, r.Message, ""); // r.Exception);
		}
		else if (r.Type == TestResultType.Ok)
		{
			Console_Write(".");
		}
		else if (r.Type == TestResultType.Skipped)
		{
			Console_Write("s");
		}
		else
		{
			//Console_WriteLine("{0}", r.Message);
		}
	}


	private void Console_Write(string message)
	{
		lock (m_Lock)
		{
			m_Text = m_Text + message;
			m_LastWasLine = false;
		}
	}

	private void Console_WriteLine(string message, params object[] args)
	{
		lock (m_Lock)
		{
			if (!m_LastWasLine)
			{
				m_Text = m_Text + "\n";
				m_LastWasLine = true;
			}

			m_Text = m_Text + string.Format(message, args) + "\n";
		}
	}



	void OnGUI()
	{
		string text = "";
		lock (m_Lock)
			text = m_Text;

		GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "MoonSharp Test Runner");
		GUI.TextArea(new Rect(0, 30, Screen.width, Screen.height - 30), text);
	}





}
