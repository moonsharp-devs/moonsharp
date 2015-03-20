using UnityEngine;
using System.Collections;
using System.Threading;
using MoonSharp.Interpreter.Tests;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

public class TestRunner : MonoBehaviour
{
	Thread m_Thread;
	string m_Text = "";
	object m_Lock = new object();
	bool m_LastWasLine = true;

	// Use this for initialization
	void Start()
	{
		Script.WarmUp();

		m_Thread = new Thread(() => DoTests());
		m_Thread.Name = "Tests";
		m_Thread.IsBackground = true;
		m_Thread.Start();
	}


	// Update is called once per frame
	void Update()
	{

	}


	void DoTests()
	{
		MoonSharp.Interpreter.Tests.TestRunner tr = new MoonSharp.Interpreter.Tests.TestRunner(Log);
		tr.Test();
	}

	void Log(TestResult r)
	{
		if (r.Type == TestResultType.Fail)
		{
			Console_WriteLine("[FAIL] | {0} - {1} - {2}", r.TestName, r.Message, r.Exception);
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
			Console_WriteLine("{0}", r.Message);
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
