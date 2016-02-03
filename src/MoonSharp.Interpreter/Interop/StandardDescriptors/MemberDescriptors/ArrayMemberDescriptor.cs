using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	public class ArrayMemberDescriptor : ObjectCallbackMemberDescriptor, IWireableDescriptor 
	{
		bool m_IsSetter;

		public ArrayMemberDescriptor(string name, bool isSetter, ParameterDescriptor[] indexerParams)
			: base(
			name,
			isSetter ? (Func<object, ScriptExecutionContext, CallbackArguments, object>)ArrayIndexerSet : (Func<object, ScriptExecutionContext, CallbackArguments, object>)ArrayIndexerGet,
			indexerParams)
		{
			m_IsSetter = isSetter;
		}
		public ArrayMemberDescriptor(string name, bool isSetter)
			: base(
			name,
			isSetter ? (Func<object, ScriptExecutionContext, CallbackArguments, object>)ArrayIndexerSet : (Func<object, ScriptExecutionContext, CallbackArguments, object>)ArrayIndexerGet)
		{
			m_IsSetter = isSetter;
		}

		public void PrepareForWiring(Table t)
		{
			t.Set("class", DynValue.NewString(this.GetType().FullName));
			t.Set("name", DynValue.NewString(Name));
			t.Set("setter", DynValue.NewBoolean(m_IsSetter));

			if (this.Parameters != null)
			{
				var pars = DynValue.NewPrimeTable();

				t.Set("params", pars);

				int i = 0;

				foreach (var p in Parameters)
				{
					DynValue pt = DynValue.NewPrimeTable();
					pars.Table.Set(++i, pt);
					p.PrepareForWiring(pt.Table);
				}
			}
		}

		private static int[] BuildArrayIndices(CallbackArguments args, int count)
		{
			int[] indices = new int[count];

			for (int i = 0; i < count; i++)
				indices[i] = args.AsInt(i, "userdata_array_indexer");

			return indices;
		}

		private static object ArrayIndexerSet(object arrayObj, ScriptExecutionContext ctx, CallbackArguments args)
		{
			Array array = (Array)arrayObj;
			int[] indices = BuildArrayIndices(args, args.Count - 1);
			DynValue value = args[args.Count - 1];

			Type elemType = array.GetType().GetElementType();

			object objValue = ScriptToClrConversions.DynValueToObjectOfType(value, elemType, null, false);

			array.SetValue(objValue, indices);

			return DynValue.Void;
		}


		private static object ArrayIndexerGet(object arrayObj, ScriptExecutionContext ctx, CallbackArguments args)
		{
			Array array = (Array)arrayObj;
			int[] indices = BuildArrayIndices(args, args.Count);

			return array.GetValue(indices);
		}

	}
}
