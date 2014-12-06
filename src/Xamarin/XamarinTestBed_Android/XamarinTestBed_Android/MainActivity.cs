using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Tests;

namespace XamarinTestBed_Android
{
	[Activity (Label = "MoonSharp Xamarin Tests", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		Thread m_Thread;
		object m_Lock = new object();
		bool m_LastWasLine = true;
		TextView textView;

		// Use this for initialization
		void Start()
		{
			Script.DefaultOptions.ScriptLoader = new XamarinLoader (Assets);
			m_Thread = new Thread(() => DoTests());
			m_Thread.Name = "Tests";
			m_Thread.IsBackground = false;
			m_Thread.Start();
		}

		void DoTests()
		{
			textView.Post (() => textView.Text = "");

			MoonSharp.Interpreter.Tests.TestRunner tr = new MoonSharp.Interpreter.Tests.TestRunner(Log);
			tr.Test();
		}

		void Log(TestResult r)
		{
			if (r.Type == TestResultType.Fail)
			{
				string message = (r.Exception is ScriptRuntimeException) ? ((ScriptRuntimeException)r.Exception).DecoratedMessage : r.Exception.Message;

				// Console_WriteLine("[FAIL] | {0} - {1} - {2}", r.TestName, message, r.Exception);
				Console_WriteLine("[FAIL] | {0} - {1} ", r.TestName, message);
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
				textView.Post (() => textView.Text += message);
				m_LastWasLine = false;
			}
		}

		private void Console_WriteLine(string message, params object[] args)
		{
			lock (m_Lock)
			{
				if (!m_LastWasLine)
				{
					textView.Post (() => textView.Text += "\n");
					m_LastWasLine = true;
				}

				string msg = (string.Format (message, args) + "\n");

				textView.Post (() => textView.Text += msg);
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			bool onXamarin = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Any(t => t.FullName.StartsWith("Android.App."));


			System.Diagnostics.Debug.WriteLine (onXamarin ? "FOUND!" : "NOT FOUND! :(");



			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			textView = FindViewById<TextView> (Resource.Id.textView1);
			
			button.Click += delegate {
				Start();
			};
		}
	}
}


