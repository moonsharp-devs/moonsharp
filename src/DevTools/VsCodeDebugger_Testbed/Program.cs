using System;
using System.Linq;
using System.Threading;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;

namespace VsCodeDebugger_Testbed
{
	class Program
	{
		const string Script1Code = @"
function run(n)
	if n <= 1 then
		return 1
	end
	return n * run(n - 1)
end
";

		const string Script2Code = @"
function run(n)
	local s = 0
	for i = 1, n do
		s = s + i
	end
	return s
end
";

		static int GetIntArg(string[] args, string name, int defaultValue)
		{
			string prefix = name + "=";
			string value = args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
			if (value == null)
			{
				return defaultValue;
			}

			int parsed;
			return int.TryParse(value.Substring(prefix.Length), out parsed) ? parsed : defaultValue;
		}

		static bool HasArg(string[] args, string name)
		{
			return args.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
		}

		public static void Main(string[] argv)
		{
			bool headless = HasArg(argv, "--headless");
			bool interactive = !headless || HasArg(argv, "--interactive");
			int iterations = Math.Max(1, GetIntArg(argv, "--iterations", 8));
			int sleepMs = Math.Max(0, GetIntArg(argv, "--sleep-ms", 25));
			int detachAt = Math.Max(1, GetIntArg(argv, "--detach-at", Math.Max(1, iterations / 2)));

			using (var server = new MoonSharpVsCodeDebugServer())
			{
				server.Logger = s => Console.WriteLine("[dap] " + s);
				server.Start();

				Script script1 = new Script();
				script1.DoString(Script1Code, null, "fact.lua");
				server.AttachToScript(script1, "Script #1");
				Closure func1 = script1.Globals.Get("run").Function;

				Script script2 = new Script();
				script2.DoString(Script2Code, null, "fact2.lua");
				server.AttachToScript(script2, "Script #2");
				Closure func2 = script2.Globals.Get("run").Function;

				Console.WriteLine("READY.");
				bool script2Attached = true;

				if (interactive)
				{
					Console.WriteLine("Interactive mode.");
					Console.WriteLine("Enter an integer n to evaluate scripts. Commands: d=detach Script #2, q=quit.");

					while (true)
					{
						Console.Write("> ");
						string line = Console.ReadLine();
						if (line == null)
						{
							break;
						}

						line = line.Trim();
						if (line.Length == 0)
						{
							continue;
						}

						if (string.Equals(line, "q", StringComparison.OrdinalIgnoreCase))
						{
							break;
						}

						if (string.Equals(line, "d", StringComparison.OrdinalIgnoreCase))
						{
							if (script2Attached)
							{
								server.Detach(script2);
								script2Attached = false;
								Console.WriteLine("Detached Script #2");
							}
							else
							{
								Console.WriteLine("Script #2 already detached.");
							}

							continue;
						}

						int n;
						if (!int.TryParse(line, out n))
						{
							Console.WriteLine("Invalid input. Enter an integer, 'd', or 'q'.");
							continue;
						}

						var fact = func1.Call(n);
						Console.WriteLine("fact({0}) = {1}", n, fact.Number);

						if (script2Attached)
						{
							var sum = func2.Call(n);
							Console.WriteLine("sum1toN({0}) = {1}", n, sum.Number);
						}
					}
				}
				else
				{
					Console.WriteLine("Running {0} iterations, detach at {1}.", iterations, detachAt);

					for (int i = 1; i <= iterations; i++)
					{
						if (script2Attached && i == detachAt)
						{
							server.Detach(script2);
							script2Attached = false;
							Console.WriteLine("Detached Script #2");
						}

						Closure func = (script2Attached && (i % 2 == 0)) ? func2 : func1;

						try
						{
							DynValue val = func.Call(5);
							Console.ForegroundColor = ConsoleColor.Magenta;
							Console.WriteLine("iter {0}: {1}", i, val.Number);
							Console.ResetColor();
						}
						catch (InterpreterException ex)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write(ex.DecoratedMessage);
							Console.ResetColor();
							throw;
						}

						if (sleepMs > 0)
						{
							Thread.Sleep(sleepMs);
						}
					}
				}

				Console.WriteLine("DONE.");
			}
		}
	}
}
