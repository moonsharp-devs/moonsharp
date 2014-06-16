using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.DataTypes;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class TableIterators
	{
		[MoonSharpMethod]
		public static RValue ipairs(IExecutionContext executionContext, CallbackArguments args) 
		{
			RValue table = args[0];

			RValue meta = executionContext.GetMetamethodTailCall(table, "__ipairs", args.ToArray());

			return meta ?? new RValue(new RValue[] { new RValue(new CallbackFunction(__next_i)), table, new RValue(0) });
		}

		// pairs (t)
		// -------------------------------------------------------------------------------------------------------------------
		// If t has a metamethod __pairs, calls it with t as argument and returns the first three results from the call.
		// Otherwise, returns three values: the next function, the table t, and nil, so that the construction
		//     for k,v in pairs(t) do body end
		// will iterate over all key–value pairs of table t.
		// See function next for the caveats of modifying the table during its traversal. 
		[MoonSharpMethod]
		public static RValue pairs(IExecutionContext executionContext, CallbackArguments args) 
		{
			RValue table = args[0];

			RValue meta = executionContext.GetMetamethodTailCall(table, "__pairs", args.ToArray());

			return meta ?? new RValue(new RValue[] { new RValue(new CallbackFunction(next)), table });
		}

		// next (table [, index])
		// -------------------------------------------------------------------------------------------------------------------
		// Allows a program to traverse all fields of a table. Its first argument is a table and its second argument is an 
		// index in this table. next returns the next index of the table and its associated value. 
		// When called with nil as its second argument, next returns an initial index and its associated value. 
		// When called with the last index, or with nil in an empty table, next returns nil. If the second argument is absent, 
		// then it is interpreted as nil. In particular, you can use next(t) to check whether a table is empty.
		// The order in which the indices are enumerated is not specified, even for numeric indices. 
		// (To traverse a table in numeric order, use a numerical for.)
		// The behavior of next is undefined if, during the traversal, you assign any value to a non-existent field in the table. 
		// You may however modify existing fields. In particular, you may clear existing fields. 
		[MoonSharpMethod]
		public static RValue next(IExecutionContext executionContext, CallbackArguments args) 
		{
			RValue table = args.AsType(0, "next", DataType.Table);
			RValue index = args[1];

			TablePair pair = table.Table.NextKey(index);

			return new RValue(new RValue[] { pair.Key, pair.Value });
		}

		// __next_i (table [, index])
		// -------------------------------------------------------------------------------------------------------------------
		// Allows a program to traverse all fields of an array. index is an integer number
		private static RValue __next_i(IExecutionContext executionContext, CallbackArguments args) 
		{
			RValue table = args.AsType(0, "__next_i", DataType.Table);
			RValue index = args.AsType(1, "__next_i", DataType.Number);

			int idx = ((int)index.Number) + 1;
			RValue val = table.Table[idx];
			
			if (val.Type != DataType.Nil)
			{
				return new RValue(new RValue[] { new RValue(idx), val });
			}
			else
			{
				return RValue.Nil;
			}
		}
	}
}
