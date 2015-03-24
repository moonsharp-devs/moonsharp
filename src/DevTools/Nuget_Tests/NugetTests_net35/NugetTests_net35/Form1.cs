using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MoonSharp.Interpreter;
using MoonSharp.RemoteDebugger;

namespace NugetTests_net35
{
	public partial class Form1 : Form
	{
		string EXPECTEDVERSION = VERSION.NUMB;
		string EXPECTEDPLATF = "std.dotnet.clr2";

		string BASICSCRIPT = @"
function dodo(x, y, z)
	return tostring((x + y) * z);
end

return dodo;
";

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			CheckString(lblVersion, EXPECTEDVERSION, Script.VERSION);
			CheckString(lblPlatform, EXPECTEDPLATF, Script.GlobalOptions.Platform.GetPlatformName());

			Script S = new Script();
			DynValue fn = S.DoString(BASICSCRIPT);
			string res = fn.Function.Call(2, 3, 4).String;

			CheckString(lblTestResult, "20", res);
		}

		private void CheckString(Label label, string expected, string actual)
		{
			label.Text = actual;

			if (actual != expected)
				label.ForeColor = Color.Red;
			else
				label.ForeColor = Color.Green;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Script S = new Script();
			DynValue fn = S.DoString(BASICSCRIPT);

			ActivateRemoteDebugger(S);

			string res = fn.Function.Call(2, 3, 4).String;

			CheckString(lblTestResult, "20", res);

		}

		RemoteDebuggerService remoteDebugger;

		private void ActivateRemoteDebugger(Script script)
		{
			if (remoteDebugger == null)
			{
				remoteDebugger = new RemoteDebuggerService();

				// the last boolean is to specify if the script is free to run 
				// after attachment, defaults to false
				remoteDebugger.Attach(script, "Description of the script", false);
			}

			// start the web-browser at the correct url. Replace this or just
			// pass the url to the user in some way.
			Process.Start(remoteDebugger.HttpUrlStringLocalHost);
		}
	}
}
