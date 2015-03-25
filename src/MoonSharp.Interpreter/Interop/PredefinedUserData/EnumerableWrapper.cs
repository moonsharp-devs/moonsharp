using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Wrappers for enumerables as return types
	/// </summary>
	internal class EnumerableWrapper
	{
		IEnumerator m_Enumerator;
		Script m_Script;
		bool m_HasTurnOnce = false;


		private EnumerableWrapper(Script script, IEnumerator enumerator)
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
				DynValue v = ClrToScriptConversions.ObjectToDynValue(m_Script, m_Enumerator.Current);

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
			EnumerableWrapper iterator = ud.Object as EnumerableWrapper;

			if (iterator == null)
			{
				throw ScriptRuntimeException.BadArgument(0, "clr_iterator_next",
					(ud.Object != null) ? ud.Object.GetType().FullName : "null",
					typeof(EnumerableWrapper).FullName,
					false);
			}

			return iterator.GetNext(prev);
		}

		internal static DynValue ConvertIterator(Script script, IEnumerator enumerator)
		{
			EnumerableWrapper ei = new EnumerableWrapper(script, enumerator);

			return DynValue.NewTuple(
				DynValue.NewCallback(clr_iterator_next),
				UserData.Create(ei),
				DynValue.Nil);
		}

		internal static DynValue ConvertTable(Table table)
		{
			return ConvertIterator(table.OwnerScript, table.Values.GetEnumerator());
		}

	}
}
