using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Interop.LuaStateInterop;

namespace MoonSharp.Interpreter.Execution
{
	/// <summary>
	/// Class giving access to details of the environment where the script is executing
	/// </summary>
	public class ScriptExecutionContext
	{
		Processor m_Processor;
		CallbackFunction m_Callback;

		internal ScriptExecutionContext(Processor p, CallbackFunction callBackFunction)
		{
			m_Processor = p;
			m_Callback = callBackFunction;
		}

		/// <summary>
		/// Gets or sets the additional data associated to this CLR function call.
		/// </summary>
		public object AdditionalData 
		{
			get { return m_Callback.AdditionalData; }
			set { m_Callback.AdditionalData = value; } 
		}


		/// <summary>
		/// Gets the metatable associated with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public Table GetMetatable(DynValue value)
		{
			return m_Processor.GetMetatable(value);
		}


		/// <summary>
		/// Gets the specified metamethod associated with the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="metamethod">The metamethod name.</param>
		/// <returns></returns>
		public DynValue GetMetamethod(DynValue value, string metamethod)
		{
			return m_Processor.GetMetamethod(value, metamethod);
		}

		/// <summary>
		/// prepares a tail call request for the specified metamethod, or null if no metamethod is found.
		/// </summary>
		public DynValue GetMetamethodTailCall(DynValue value, string metamethod, params DynValue[] args)
		{
			DynValue meta = this.GetMetamethod(value, metamethod);
			if (meta == null) return null;
			return DynValue.NewTailCallReq(meta, args);
		}

		/// <summary>
		/// Gets the metamethod to be used for a binary operation using op1 and op2.
		/// </summary>
		public DynValue GetBinaryMetamethod(DynValue op1, DynValue op2, string eventName)
		{
			return m_Processor.GetBinaryMetamethod(op1, op2, eventName);
		}

		/// <summary>
		/// Gets the script object associated with this request
		/// </summary>
		/// <returns></returns>
		public Script GetScript()
		{
			return m_Processor.GetScript();
		}

		/// <summary>
		/// Gets the coroutine which is performing the call
		/// </summary>
		public Coroutine GetCallingCoroutine()
		{
			return m_Processor.AssociatedCoroutine;
		}

		/// <summary>
		/// Calls a callback function implemented in "classic way". 
		/// Useful to port C code from Lua, or C# code from UniLua and KopiLua.
		/// Lua : http://www.lua.org/
		/// UniLua : http://github.com/xebecnan/UniLua
		/// KopiLua : http://github.com/NLua/KopiLua
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="functionName">Name of the function - for error messages.</param>
		/// <param name="callback">The callback.</param>
		/// <returns></returns>
		public DynValue EmulateClassicCall(CallbackArguments args, string functionName, Func<LuaState, int> callback)
		{
			LuaState L = new LuaState(this, args, functionName);
			int retvals = callback(L);
			return L.GetReturnValue(retvals);
		}
	}
}
