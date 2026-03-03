using System;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Marks a method as a clr enumerator based coroutine
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class MoonSharpClrCoroutineAttribute : Attribute
	{
        public MoonSharpClrCoroutineAttribute()
		{
        }
    }
}