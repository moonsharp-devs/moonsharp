using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MoonSharp.Interpreter;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NugetTests_wsa
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		string EXPECTEDVERSION = VERSION.NUMB;
		string EXPECTEDPLATF = "limited.dotnet.portable.clr4";

		string BASICSCRIPT = @"
function dodo(x, y, z)
	return tostring((x + y) * z);
end

return dodo;
";

        public MainPage()
        {
            this.InitializeComponent();
        }

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			CheckString(lblVersion, EXPECTEDVERSION, Script.VERSION);
			CheckString(lblPlatform, EXPECTEDPLATF, Script.GlobalOptions.Platform.GetPlatformName());

			Script S = new Script();
			DynValue fn = S.DoString(BASICSCRIPT);
			string res = fn.Function.Call(2, 3, 4).String;

			CheckString(lblTestResult, "20", res);
		}

		private void CheckString(TextBlock label, string expected, string actual)
		{
			label.Text = actual;

			if (actual != expected)
				label.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
			else
				label.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 200, 0));
		}
    }
}
