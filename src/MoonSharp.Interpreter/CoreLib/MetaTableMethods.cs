using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class MetaTableMethods
	{
		// setmetatable (table, metatable)
		// -------------------------------------------------------------------------------------------------------------------
		// Sets the metatable for the given table. (You cannot change the metatable of other 
		// types from Lua, only from C.) If metatable is nil, removes the metatable of the given table. 
		// If the original metatable has a "__metatable" field, raises an error ("cannot change a protected metatable").
		// This function returns table. 
		[MoonSharpMethod]
		public static RValue setmetatable(IExecutionContext executionContext, CallbackArguments args)  
		{
			RValue table = args.AsType(0, "setmetatable", DataType.Table);
			RValue metatable = args.AsType(1, "setmetatable", DataType.Table, true);

			RValue curmeta = executionContext.GetMetamethod(table, "__metatable");

			if (curmeta != null)
			{
				throw new ScriptRuntimeException(null, "cannot change a protected metatable");
			}

			table.Meta = metatable;
			return table;
		}

		// getmetatable (object)
		// -------------------------------------------------------------------------------------------------------------------
		// If object does not have a metatable, returns nil. Otherwise, if the object's metatable 
		// has a "__metatable" field, returns the associated value. Otherwise, returns the metatable of the given object. 
		[MoonSharpMethod]
		public static RValue getmetatable(IExecutionContext executionContext, CallbackArguments args)  
		{
			RValue obj = args[0];

			if (obj.Type == DataType.Nil)
				return RValue.Nil;

			RValue curmeta = executionContext.GetMetamethod(obj, "__metatable");

			if (curmeta != null)
			{
				return curmeta;
			}

			return obj.Meta ?? RValue.Nil;
		}

		// rawget (table, index)
		// -------------------------------------------------------------------------------------------------------------------
		// Gets the real value of table[index], without invoking any metamethod. table must be a table; index may be any value.
		[MoonSharpMethod]
		public static RValue rawget(IExecutionContext executionContext, CallbackArguments args)  
		{
			RValue table = args.AsType(0, "rawget", DataType.Table);
			RValue index = args[1];

			return table.Table[index];
		}

		// rawset (table, index, value)
		// -------------------------------------------------------------------------------------------------------------------
		// Sets the real value of table[index] to value, without invoking any metamethod. table must be a table, 
		// index any value different from nil and NaN, and value any Lua value.
		// This function returns table. 
		[MoonSharpMethod]
		public static RValue rawset(IExecutionContext executionContext, CallbackArguments args)  
		{
			RValue table = args.AsType(0, "rawset", DataType.Table);
			RValue index = args[1];

			table.Table[index] = args[2];

			return table;
		}

		// rawequal (v1, v2)
		// -------------------------------------------------------------------------------------------------------------------
		// Checks whether v1 is equal to v2, without invoking any metamethod. Returns a boolean. 
		[MoonSharpMethod]
		public static RValue rawequal(IExecutionContext executionContext, CallbackArguments args)  
		{
			RValue v1 = args[0];
			RValue v2 = args[1];

			return new RValue(v1.Equals(v2)); 
		}

		//rawlen (v)
		// -------------------------------------------------------------------------------------------------------------------
		//Returns the length of the object v, which must be a table or a string, without invoking any metamethod. Returns an integer number.	
		[MoonSharpMethod]
		public static RValue rawlen(IExecutionContext executionContext, CallbackArguments args) 
		{
			return new RValue(args[0].GetLength());
		}



	}
}
