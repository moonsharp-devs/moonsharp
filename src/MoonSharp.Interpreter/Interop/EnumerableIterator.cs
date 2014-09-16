using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public class EnumerableIterator
	{
		IEnumerator m_Enumerator;
		Script m_Script;
		bool m_HasTurnOnce = false;

		private EnumerableIterator(Script script, IEnumerator enumerator)
		{
			m_Script = script;
			m_Enumerator = enumerator;
		}

		private DynValue GetNext(DynValue prev)
		{
			if (prev.IsNil())
				Reset();

			while (m_Enumerator.MoveNext())
			{
				DynValue v = ConversionHelper.ClrObjectToComplexMoonSharpValue(m_Script, m_Enumerator.Current);

				if (!v.IsNil())
					return v;
			}

			return DynValue.Nil;
		}

		private void Reset()
		{
			if (m_HasTurnOnce)
				m_Enumerator.Reset();

			m_HasTurnOnce = true;
		}

		private static DynValue clr_iterator_next(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue userdata = args.AsType(0, "clr_iterator_next", DataType.UserData);
			DynValue prev = args[1];

			UserData ud = userdata.UserData;
			EnumerableIterator iterator = ud.Object as EnumerableIterator;

			if (iterator == null)
			{
				throw ScriptRuntimeException.BadArgument(0, "clr_iterator_next",
					(ud.Object != null) ? ud.Object.GetType().FullName : "null",
					typeof(EnumerableIterator).FullName,
					false);
			}

			return iterator.GetNext(prev);
		}

		public static DynValue ConvertIterator(Script script, IEnumerator enumerator)
		{
			EnumerableIterator ei = new EnumerableIterator(script, enumerator);

			return DynValue.NewTuple(
				DynValue.NewCallback(clr_iterator_next),
				script.UserDataRepository.CreateUserData(ei),
				DynValue.Nil);
		}

	}
}
