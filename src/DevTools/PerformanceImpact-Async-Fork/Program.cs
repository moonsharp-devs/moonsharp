using System;
using System.Threading;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Diagnostics;

namespace PerformanceImpact_Async
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = @"
			function move(n, src, dst, via)
				if n > 0 then
					move(n - 1, src, via, dst)
					--check(src, 'to', dst)
					move(n - 1, via, dst, src)
				end
			end
 
            function run_test()
			    for i = 1, 15000 do
				    move(4, 1, 2, 3)
			    end
            end
			";

            Script.WarmUp();

            var S = new Script();

            S.DoString(code);

            long totalTime = 0;
            long i = 0;

            var run_testFunc = S.Globals.Get("run_test");

#if FORK
            var ecToken = new ExecutionControlToken();
#endif

            while (true)
            {
                S.PerformanceStats.Enabled = true;

#if FORK
                S.CallAsync(ecToken, run_testFunc).Wait();
#else
                S.CallAsync(run_testFunc).Wait();
#endif

                // Get current average
                totalTime += S.PerformanceStats.GetPerformanceCounterResult(PerformanceCounter.Execution).Counter;

                ++i;

                Console.WriteLine("Current average: {0}", totalTime / i);

                //Thread.Sleep(20);

                S.PerformanceStats.Enabled = false;
            }
        }
    }
}
