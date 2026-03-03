#if HASDYNAMIC
using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
    [TestFixture]
    public class AsyncTests
    {
        [Test]
        public void ThreadPausedIfRunAsync()
        {
            Script S = new Script();

            S.Globals.Get("os").Table["sleep"] = (Action<ScriptExecutionContext, CallbackArguments>)((ctx, args) => 
            {
                ctx.PauseExecution(TimeSpan.FromSeconds(2));
            });

            string code = @"
                local timeStarted = os.clock()
                os.sleep()
                local timeEnded = os.clock()

                return timeEnded - timeStarted > 2";

            var ecToken = new ExecutionControlToken();
            Assert.IsTrue(S.DoStringAsync(ecToken, code).Result.CastToBool());
        }

        [Test]
        public void ThreadPausedIfRunSync()
        {
            Script S = new Script();

            S.Globals.Get("os").Table["sleep"] = (Action<ScriptExecutionContext, CallbackArguments>)((ctx, args) =>
            {
                ctx.PauseExecution(TimeSpan.FromSeconds(2));
            });

            string code = @"
                local timeStarted = os.clock()
                os.sleep()
                local timeEnded = os.clock()

                return timeEnded - timeStarted > 2";

            Assert.IsTrue(S.DoString(code).CastToBool());
        }

        [Test]
        public void ClrFunctionExecutionCanBeCancelledIfPausedWhenRunAsync()
        {
            Script S = new Script();

            S.Globals.Set("pause", DynValue.NewCallback((ctx, args) =>
            {
                ctx.PauseExecution(TimeSpan.FromSeconds(20));
                return DynValue.NewNil();
            }));

            var ecToken = new ExecutionControlToken();

            Task<DynValue> t = S.CallAsync(ecToken, S.Globals.Get("pause"));

            t.ContinueWith(_ => Assert.Pass(), TaskContinuationOptions.OnlyOnCanceled);
            t.ContinueWith(t_ => { if (t_.Exception.InnerExceptions.Count != 1 || !(t_.Exception.InnerException.InnerException is ScriptTerminationRequestedException)) { Assert.Fail("task faulted"); } }, TaskContinuationOptions.OnlyOnFaulted);
            t.ContinueWith(_ => Assert.Fail("task didn't abort"), TaskContinuationOptions.OnlyOnRanToCompletion);

            Thread.Sleep(500);
            ecToken.Terminate();

            while (t.Status != TaskStatus.Canceled && t.Status != TaskStatus.Faulted && t.Status != TaskStatus.RanToCompletion) { }
        }

        [Test]
        public void LuaCodeExecutionCanBeCancelledIfPausedWhenRunAsync()
        {
            Script S = new Script();

            S.Globals.Set("pause", DynValue.NewCallback((ctx, args) =>
            {
                ctx.PauseExecution(TimeSpan.FromSeconds(20));
                return DynValue.NewNil();
            }));

            var ecToken = new ExecutionControlToken();

            Task<DynValue> t = S.DoStringAsync(ecToken, "pause()");

            t.ContinueWith(_ => Assert.Pass(), TaskContinuationOptions.OnlyOnCanceled);
            t.ContinueWith(t_ => { if (t_.Exception.InnerExceptions.Count != 1 || !(t_.Exception.InnerException.InnerException is ScriptTerminationRequestedException)) { Assert.Fail("task faulted"); } }, TaskContinuationOptions.OnlyOnFaulted);
            t.ContinueWith(_ => Assert.Fail("task didn't abort"), TaskContinuationOptions.OnlyOnRanToCompletion);

            Thread.Sleep(500);
            ecToken.Terminate();

            while (t.Status != TaskStatus.Canceled && t.Status != TaskStatus.Faulted && t.Status != TaskStatus.RanToCompletion) { }
        }

        [Test]
        public void ExecutionControlTokenCanBeAssociatedWithMultipleScriptsSimultaneously()
        {
            var callback = DynValue.NewCallback((ctx, args) =>
            {
                ctx.PauseExecution(TimeSpan.FromSeconds(20));
                return DynValue.NewNil();
            });

            Script S1 = new Script();

            S1.Globals.Set("pause", callback);

            Script S2 = new Script();

            S2.Globals.Set("pause", callback);

            var ecToken = new ExecutionControlToken();

            Task<DynValue> t1 = S1.DoStringAsync(ecToken, "pause()");
            Task<DynValue> t2 = S2.DoStringAsync(ecToken, "pause()");

            t1.ContinueWith(t => { foreach (var e in t.Exception.InnerExceptions) { Console.WriteLine(e.InnerException.Message); } }, TaskContinuationOptions.NotOnRanToCompletion);
            t1.ContinueWith(t => Assert.IsTrue(t1.IsCanceled, "t1 is canceled"), TaskContinuationOptions.OnlyOnCanceled);
            t1.ContinueWith(t => Assert.Fail(), TaskContinuationOptions.OnlyOnRanToCompletion);

            t2.ContinueWith(t => { foreach (var e in t.Exception.InnerExceptions) { Console.WriteLine(e.InnerException.Message); } }, TaskContinuationOptions.NotOnRanToCompletion);
            t2.ContinueWith(t => Assert.IsTrue(t2.IsCanceled, "t2 is canceled"), TaskContinuationOptions.OnlyOnCanceled);
            t2.ContinueWith(t => Assert.Fail(), TaskContinuationOptions.OnlyOnRanToCompletion);

            Thread.Sleep(500);
            ecToken.Terminate();

            while ((t1.Status != TaskStatus.Canceled && t1.Status != TaskStatus.Faulted && t1.Status != TaskStatus.RanToCompletion) ||
                   (t2.Status != TaskStatus.Canceled && t2.Status != TaskStatus.Faulted && t2.Status != TaskStatus.RanToCompletion)) { }
        }
    }
}
#endif