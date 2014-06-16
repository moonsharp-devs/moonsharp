using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class CallbackArguments
	{
		IList<RValue> m_Args;

		public CallbackArguments(IList<RValue> args)
		{
			m_Args = args;
		}

		public int Count
		{
			get { return m_Args.Count; }
		}

		public RValue this[int index]
		{
			get 
			{
				if (index < m_Args.Count)
					return m_Args[index];

				return RValue.Nil;
			}
		}

		public IList<RValue> List { get { return m_Args; } }

		public RValue[] ToArray()
		{
			return List.ToArray();
		}

		public void ThrowBadArgument(int argNum, string funcName, object expected, object got)
		{
			// bad argument #1 to 'next' (table expected, got number)
			throw new ScriptRuntimeException(null, "bad argument #{0} to '{1}' ({2} expected, got {3}",
				argNum + 1, funcName, expected, got);
		}

		public RValue AsType(int argNum, string funcName, DataType type)
		{
			if (this[argNum].Type != type)
				ThrowBadArgument(argNum, funcName, type, this[argNum].Type);

			return this[argNum];
		}



	}
}
