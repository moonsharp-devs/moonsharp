using System;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Exception thrown when an async script is requested to abort
    /// </summary>
#if !(PCL || ((!UNITY_EDITOR) && (ENABLE_DOTNET)) || NETFX_CORE)
	[Serializable]
#endif
    public class ScriptTerminationRequestedException : InterpreterException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTerminationRequestedException"/> class.
        /// </summary>
        internal ScriptTerminationRequestedException()
            : base("script has been requested to abort")
        {
            DecoratedMessage = Message;
        }
    }
}
