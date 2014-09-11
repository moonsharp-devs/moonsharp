using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
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

		public DynValue AsType(int argNum, string funcName, DataType type, bool allowNil = false)
		{
			if (allowNil && this[argNum].Type == DataType.Nil)
				return this[argNum];

			if (this[argNum].Type != type)
				throw ScriptRuntimeException.BadArgument(argNum, funcName, type, this[argNum].Type, allowNil);

			return this[argNum];
		}



	}
}
