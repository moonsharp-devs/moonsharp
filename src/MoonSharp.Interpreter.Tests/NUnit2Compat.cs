#if !(UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_EDITOR || UNITY_STANDALONE)

using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework
{
	/// <summary>
    /// Tests were originally written for NUnit2 and have since been migrated to NUnit3. NUnit3 removed this attribute,
    /// honestly, for good reason, but for now we're providing our own implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ExpectedExceptionAttribute : NUnitAttribute, IWrapTestMethod
    {
        public Type ExpectedException { get; }

        public ExpectedExceptionAttribute(Type type)
        {
            ExpectedException = type;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ExpectedExceptionCommand(command, ExpectedException);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand
        {
            private readonly Type _expectedException;

            public ExpectedExceptionCommand(TestCommand innerCommand, Type expectedException) : base(innerCommand)
            {
                _expectedException = expectedException;
            }

            #if UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_EDITOR || UNITY_STANDALONE
            public override TestResult Execute(ITestExecutionContext context)
            #else
            public override TestResult Execute(TestExecutionContext context)
            #endif
            {
                Type caughtType = null;

                try
                {
                    innerCommand.Execute(context);
                }
                catch (Exception e)
                {
                    if (e is NUnitException)
                    {
                        e = e.InnerException;
                    }

                    caughtType = e.GetType();
                }

                if (caughtType == _expectedException)
                {
                    context.CurrentResult.SetResult(ResultState.Success);
                }
                else if (caughtType != null)
                {
                    context.CurrentResult.SetResult(ResultState.Failure, $"Expected {_expectedException.Name} but got {caughtType.Name}");
                }
                else
                {
                    context.CurrentResult.SetResult(ResultState.Failure, $"Expected {_expectedException.Name} but no exception was thrown");
                }

                return context.CurrentResult;
            }
        }
    }
}

#endif
