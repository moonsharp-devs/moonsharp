using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.REPL;

namespace MoonSharpSL5ReplDemo
{
	public partial class MainPage : UserControl
	{
		Script script;
		ReplHistoryInterpreter interpreter;

		public MainPage()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Console_WriteLine("MoonSharp REPL {0} [{1}]", Script.VERSION, Script.GlobalOptions.Platform.GetPlatformName());
			Console_WriteLine("Copyright (C) 2014-2015 Marco Mastropaolo");
			Console_WriteLine("http://www.moonsharp.org");
			Console_WriteLine();

			Console_WriteLine("Type Lua code in the text box below to execute it.");
			Console_WriteLine("The 'io', 'file' and parts of the 'os' modules are not available due to Silverlight restrictions.");
			Console_WriteLine("Type list() or list(<table>) to see which globals are available.");
			Console_WriteLine();
			Console_WriteLine("Welcome.");
			Console_WriteLine();

			script = new Script(CoreModules.Preset_Complete);

			script.DoString(@"
local function pad(str, len)
	str = str .. ' ' .. string.rep('.', len);
	str = string.sub(str, 1, len);
	return str;
end

function list(lib)
	if (lib == nil) then lib = _G; end

	if (type(lib) ~= 'table') then
		print('A table was expected to list command.');
		return
	end

	for k, v in pairs(lib) do
		print(pad(type(v), 12) .. ' ' .. k)
	end
end");

			script.Options.DebugPrint = s => Console_WriteLine(s);

			interpreter = new ReplHistoryInterpreter(script, 100)
			{
				HandleDynamicExprs = true,
				HandleClassicExprsSyntax = true
			};


			DoPrompt();
		}

		private void Console_WriteLine(string str = null)
		{
			txtOutput.Text += (str ?? "") + "\n";
			scroller.ScrollToVerticalOffset(scroller.ScrollableHeight);
			scroller.UpdateLayout();
		}

		private void Console_WriteLine(string format, params object[] args)
		{
			string str = string.Format(format, args);
			Console_WriteLine(str);
		}

		private void DoPrompt()
		{
			lblPrompt.Text = interpreter.ClassicPrompt;
			txtInput.Text = "";
			txtInput.Focus();
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Console_WriteLine(lblPrompt.Text + " " + txtInput.Text);

				try
				{
					DynValue dv = interpreter.Evaluate(txtInput.Text);

					if (dv != null)
					{
						if (dv.Type == DataType.Void)
							Console_WriteLine("ok");
						else
							Console_WriteLine("{0}", dv);
					}
				}
				catch (InterpreterException ex)
				{
					Console_WriteLine("{0}", ex.DecoratedMessage ?? ex.Message);
				}
				catch (Exception ex)
				{
					Console_WriteLine("Unexpected error: {0}", ex.Message);
				}

				DoPrompt();
			}
			else if (e.Key == Key.Up)
			{
				string v = interpreter.HistoryPrev();
				if (v != null)
				{
					txtInput.Text = v;
					txtInput.Select(txtInput.Text.Length, 0);
				}
			}
			else if (e.Key == Key.Down)
			{
				string v = interpreter.HistoryNext();
				if (v != null)
				{
					txtInput.Text = v;
					txtInput.Select(txtInput.Text.Length, 0);
				}
			}

		}




	}
}
