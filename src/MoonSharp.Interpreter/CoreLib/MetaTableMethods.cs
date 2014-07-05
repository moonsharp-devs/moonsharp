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
		public static DynValue setmetatable(ScriptExecutionContext executionContext, CallbackArguments args)  
		{
			DynValue table = args.AsType(0, "setmetatable", DataType.Table);
			DynValue metatable = args.AsType(1, "setmetatable", DataType.Table, true);

			DynValue curmeta = executionContext.GetMetamethod(table, "__metatable");

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
		public static DynValue getmetatable(ScriptExecutionContext executionContext, CallbackArguments args)  
		{
			DynValue obj = args[0];

			if (obj.Type == DataType.Nil)
				return DynValue.Nil;

			DynValue curmeta = executionContext.GetMetamethod(obj, "__metatable");

			if (curmeta != null)
			{
				return curmeta;
			}

			return obj.Meta ?? DynValue.Nil;
		}

		// rawget (table, index)
		// -------------------------------------------------------------------------------------------------------------------
		// Gets the real value of table[index], without invoking any metamethod. table must be a table; index may be any value.
		[MoonSharpMethod]
		public static DynValue rawget(ScriptExecutionContext executionContext, CallbackArguments args)  
		{
			DynValue table = args.AsType(0, "rawget", DataType.Table);
			DynValue index = args[1];

			return table.Table[index];
		}

		// rawset (table, index, value)
		// -------------------------------------------------------------------------------------------------------------------
		// Sets the real value of table[index] to value, without invoking any metamethod. table must be a table, 
		// index any value different from nil and NaN, and value any Lua value.
		// This function returns table. 
		[MoonSharpMethod]
		public static DynValue rawset(ScriptExecutionContext executionContext, CallbackArguments args)  
		{
			DynValue table = args.AsType(0, "rawset", DataType.Table);
			DynValue index = args[1];

			table.Table[index] = args[2];

			return table;
		}

		// rawequal (v1, v2)
		// -------------------------------------------------------------------------------------------------------------------
		// Checks whether v1 is equal to v2, without invoking any metamethod. Returns a boolean. 
		[MoonSharpMethod]
		public static DynValue rawequal(ScriptExecutionContext executionContext, CallbackArguments args)  
		{
			DynValue v1 = args[0];
			DynValue v2 = args[1];

			return DynValue.NewBoolean(v1.Equals(v2)); 
		}

		//rawlen (v)
		// -------------------------------------------------------------------------------------------------------------------
		//Returns the length of the object v, which must be a table or a string, without invoking any metamethod. Returns an integer number.	
		[MoonSharpMethod]
		public static DynValue rawlen(ScriptExecutionContext executionContext, CallbackArguments args) 
		{
			return args[0].GetLength();
		}



	}
}
