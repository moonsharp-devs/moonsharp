using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Tests;

namespace SilverlightTestRunner
{
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();
		}

		object m_Lock = new object();
		bool m_LastWasLine = true;
		SynchronizationContext m_Ctx;

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			m_Ctx = SynchronizationContext.Current;
		}

		void DoTests()
		{
			m_Ctx.Post(o => textView.Text = "", null);

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
				m_Ctx.Post(o => textView.Text += message, null);
				m_LastWasLine = false;
			}
		}

		private void Console_WriteLine(string message, params object[] args)
		{
			lock (m_Lock)
			{
				if (!m_LastWasLine)
				{
					m_Ctx.Post(o => textView.Text += "\n", null);
					m_LastWasLine = true;
				}

				string msg = (string.Format(message, args) + "\n");

				m_Ctx.Post(o => textView.Text += msg, null);
			}
		}


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DoTests();
		}

	}
}
