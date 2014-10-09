using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// This class is a container for arguments received by a CallbackFunction
	/// </summary>
	public class CallbackArguments
	{
		IList<DynValue> m_Args;

		/// <summary>
		/// Initializes a new instance of the <see cref="CallbackArguments" /> class.
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <param name="isMethodCall">if set to <c>true</c> [is method call].</param>
		public CallbackArguments(IList<DynValue> args, bool isMethodCall)
		{
			m_Args = args;
			IsMethodCall = isMethodCall;
		}

		/// <summary>
		/// Gets the count of arguments
		/// </summary>
		public int Count
		{
			get { return m_Args.Count; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this is a method call.
		/// </summary>
		public bool IsMethodCall { get; private set; }


		/// <summary>
		/// Gets the <see cref="DynValue"/> at the specified index, or Nil if not found (mimicing Lua behavior)
		/// </summary>
		public DynValue this[int index]
		{
			get
			{
				return RawGet(index) ?? DynValue.Nil;
			}
		}

		/// <summary>
		/// Gets the <see cref="DynValue"/> at the specified index, or null.
		/// </summary>
		public DynValue RawGet(int index)
		{
			if (index < m_Args.Count)
				return m_Args[index];

			return null;
		}


		/// <summary>
		/// Gets the list of arguments
		/// </summary>
		public IList<DynValue> List { get { return m_Args; } }

		/// <summary>
		/// Converts the arguments to an array
		/// </summary>
		public DynValue[] ToArray()
		{
			return List.ToArray();
		}

		/// <summary>
		/// Gets the specified argument as as an argument of the specified type. If not possible,
		/// an exception is raised.
		/// </summary>
		/// <param name="argNum">The argument number.</param>
		/// <param name="funcName">Name of the function.</param>
		/// <param name="type">The type desired.</param>
		/// <param name="allowNil">if set to <c>true</c> nil values are allowed.</param>
		/// <returns></returns>
		public DynValue AsType(int argNum, string funcName, DataType type, bool allowNil = false)
		{
			if (allowNil && this[argNum].Type == DataType.Nil)
				return this[argNum];

			if (argNum >= this.Count)
				throw ScriptRuntimeException.BadArgumentNoValue(argNum, funcName, type);

			if (this[argNum].Type != type)
				throw ScriptRuntimeException.BadArgument(argNum, funcName, type, this[argNum].Type, allowNil);

			return this[argNum];
		}


		public double AsDouble(int argNum, string funcName)
		{
			if (this[argNum].Type == DataType.Nil)
				throw ScriptRuntimeException.BadArgumentNoValue(argNum, funcName, DataType.Number);

			if (this[argNum].Type != DataType.Number)
			{
				double? val = this[argNum].CastToNumber();
				throw ScriptRuntimeException.BadArgument(argNum, funcName, DataType.Number, this[argNum].Type, false);
			}
			else
			{
				return this[argNum].Number;
			}
		}

		public int AsInt(int argNum, string funcName)
		{
			double d = AsDouble(argNum, funcName);
			return (int)d;
		}

		public string AsStringUsingMeta(ScriptExecutionContext executionContext, int i, string funcName)
		{
			if ((this[i].Type == DataType.Table) && (this[i].Table.MetaTable != null) &&
				(this[i].Table.MetaTable.RawGet("__tostring") != null))
			{
				var v = executionContext.GetScript().Call(this[i].Table.MetaTable.RawGet("__tostring"), this[i]);

				if (v.Type != DataType.String)
					throw new ScriptRuntimeException("'tostring' must return a string to '{0}'", funcName);

				return v.ToPrintString();
			}
			else
			{
				return (this[i].ToPrintString());
			}
		}

	}
}
