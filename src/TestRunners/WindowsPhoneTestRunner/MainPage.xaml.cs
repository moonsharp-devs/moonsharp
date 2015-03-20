using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Tests;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WindowsPhoneTestRunner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
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
