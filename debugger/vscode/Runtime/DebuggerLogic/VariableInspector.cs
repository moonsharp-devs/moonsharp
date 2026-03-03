#if (!UNITY_5) || UNITY_STANDALONE

using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger.SDK;

namespace MoonSharp.VsCodeDebugger.DebuggerLogic
{
	internal static class VariableInspector
	{
		private static void InspectDynValue(DynValue v, List<Variable> variables, List<object> structuredVariables)
		{
			if (v.Type != DataType.Table)
			{
				variables.Add(new Variable("(value)", v.ToPrintString(), v.Type.ToLuaDebuggerString()));
				variables.Add(new Variable("(val #id)", v.ReferenceID.ToString(), null));
			}

			switch (v.Type)
			{
				case DataType.Tuple:
					for (int i = 0; i < v.Tuple.Length; i++)
					{
						DynValue value = v.Tuple[i] ?? DynValue.Void;
						variables.Add(new Variable("[i]", value.ToDebugPrintString(), value.Type.ToLuaDebuggerString()));
					}
					break;
				case DataType.Function:
					variables.Add(new Variable("(address)", v.Function.EntryPointByteCodeLocation.ToString("X8"), null));
					variables.Add(new Variable("(upvalues)", v.Function.GetUpvaluesCount().ToString(), null));
					variables.Add(new Variable("(upvalues type)", v.Function.GetUpvaluesType().ToString(), null));
					break;
				case DataType.Table:
					InspectTable(v.Table, variables, structuredVariables);
					break;
				case DataType.UserData:
					if (v.UserData.Descriptor != null)
					{
						variables.Add(new Variable("(descriptor)", v.UserData.Descriptor.Name, null));
					}
					else
					{
						variables.Add(new Variable("(descriptor)", "null!", DataType.Nil.ToLuaDebuggerString()));
					}

					variables.Add(new Variable("(native object)", v.UserData.Object != null ? v.UserData.Object.GetType().Name : "(null)", null));
					break;
				case DataType.Thread:
					variables.Add(new Variable("(coroutine state)", v.Coroutine.State.ToString(), null));
					variables.Add(new Variable("(coroutine type)", v.Coroutine.Type.ToString(), null));
					variables.Add(new Variable("(auto-yield counter)", v.Coroutine.AutoYieldCounter.ToString(), null));
					break;
				case DataType.ClrFunction:
					variables.Add(new Variable("(name)", v.Callback.Name ?? "(unnamed)", null));
					break;
				case DataType.TailCallRequest:
				case DataType.YieldRequest:
				case DataType.Nil:
				case DataType.Void:
				case DataType.Boolean:
				case DataType.Number:
				case DataType.String:
				default:
					break;
			}
		}

		private static void InspectTable(Table table, List<Variable> variables, List<object> structuredVariables)
		{
			if (table.MetaTable != null)
			{
				structuredVariables.Add(table.MetaTable);
				variables.Add(new Variable("(metatable)", "CLR Table", "Table", structuredVariables.Count));
			}

			foreach (TablePair p in table.Pairs)
			{
				var index = p.Value.Type == DataType.Table ? structuredVariables.Count + 1 : 0;

				if (index > 0)
				{
					structuredVariables.Add(p.Value);
				}

				var key = p.Key.Type == DataType.String ? p.Key.ToDebugPrintString() : "[" + p.Key.ToDebugPrintString() + "]";
				variables.Add(new Variable(key, p.Value.ToDebugPrintString(), p.Value.Type.ToLuaDebuggerString(), index));
			}
		}

		internal static void InspectVariable(object v, List<Variable> variables, List<object> structuredVariables)
		{
			if (v is DynValue dynValue)
			{
				InspectDynValue(dynValue, variables, structuredVariables);
			}
			else if (v is Table table)
			{
				InspectTable(table, variables, structuredVariables);
			}
		}
	}
}

#endif
