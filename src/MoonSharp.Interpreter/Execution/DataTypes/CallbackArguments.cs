using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class CallbackArguments
	{
		IList<DynValue> m_Args;

		public CallbackArguments(IList<DynValue> args)
		{
			m_Args = args;
		}

		public int Count
		{
			get { return m_Args.Count; }
		}

		public DynValue this[int index]
		{
			get 
			{
				if (index < m_Args.Count)
					return m_Args[index];

				return DynValue.Nil;
			}
		}

		public IList<DynValue> List { get { return m_Args; } }

		public DynValue[] ToArray()
		{
			return List.ToArray();
		}

		public void ThrowBadArgument(int argNum, string funcName, object expected, object got)
		{
			// bad argument #1 to 'next' (table expected, got number)
			throw new ScriptRuntimeException(null, "bad argument #{0} to '{1}' ({2} expected, got {3}",
				argNum + 1, funcName, expected, got);
		}

		public DynValue AsType(int argNum, string funcName, DataType type, bool allowNil = false)
		{
			if (allowNil && this[argNum].Type == DataType.Nil)
				return this[argNum];

			if (this[argNum].Type != type)
				ThrowBadArgument(argNum, funcName, type, this[argNum].Type);

			return this[argNum];
		}



	}
}
