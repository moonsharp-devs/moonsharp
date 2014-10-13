#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ptrdiff_t = System.Int32;
using lua_Integer = System.Int32;
using LUA_INTFRM_T = System.Int64;
using UNSIGNED_LUA_INTFRM_T = System.UInt64;
using LuaLBuffer = System.Text.StringBuilder;

namespace MoonSharp.Interpreter.Interop.LuaStateInterop
{
	public class LuaBase
	{
		protected const int LUA_TNONE = -1;
		protected const int LUA_TNIL = 0;
		protected const int LUA_TBOOLEAN = 1;
		protected const int LUA_TLIGHTUSERDATA = 2;
		protected const int LUA_TNUMBER = 3;
		protected const int LUA_TSTRING = 4;
		protected const int LUA_TTABLE = 5;
		protected const int LUA_TFUNCTION = 6;
		protected const int LUA_TUSERDATA = 7;
		protected const int LUA_TTHREAD = 8;


		protected static lua_Integer LuaType(LuaState L, lua_Integer p)
		{
			if (p > L.Args.Count || p <= 0)
				return LUA_TNONE;

			switch (L.Args[p].Type)
			{
				case DataType.Nil:
					return LUA_TNIL;
				case DataType.Boolean:
					return LUA_TNIL;
				case DataType.Number:
					return LUA_TNUMBER;
				case DataType.String:
					return LUA_TSTRING;
				case DataType.Function:
					return LUA_TFUNCTION;
				case DataType.Table:
					return LUA_TTABLE;
				case DataType.UserData:
					return LUA_TUSERDATA;
				case DataType.Thread:
					return LUA_TTHREAD;
				case DataType.ClrFunction:
					return LUA_TFUNCTION;
				case DataType.TailCallRequest:
				case DataType.YieldRequest:
				case DataType.Tuple:
				default:
					throw new ScriptRuntimeException("Can't call LuaType on any type");
			}
		}

		protected static string LuaLCheckLString(LuaState L, lua_Integer argNum, out uint l)
		{
			string str = L.Args.AsString(L.ExecutionContext, argNum, L.FunctionName);
			l = (uint)str.Length;
			return str;
		}

		protected static void LuaPushInteger(LuaState L, lua_Integer val)
		{
			L.Stack.Push(DynValue.NewNumber(val));
		}

		protected static lua_Integer LuaToBoolean(LuaState L, lua_Integer p)
		{
			return L.Args[p].CastToBool() ? 1 : 0;
		}

		protected static string LuaToLString(LuaState luaState, lua_Integer p, out uint l)
		{
			return LuaLCheckLString(luaState, p, out l);
		}

		protected static string LuaToString(LuaState luaState, lua_Integer p)
		{
			uint l;
			return LuaLCheckLString(luaState, p, out l);
		}

		protected static void LuaLAddValue(LuaLBuffer b)
		{
			b.StringBuilder.Append(b.LuaState.Stack.Pop().ToPrintString());
		}

		protected static void LuaLAddLString(LuaLBuffer b, CharPtr s, uint p)
		{
			b.StringBuilder.Append(s.ToString((int)p));
		}

		protected static lua_Integer LuaLOptInteger(LuaState L, lua_Integer pos, lua_Integer def)
		{
			return L.Args.AsInt(pos, L.FunctionName, true) ?? def;
		}

		protected static lua_Integer LuaLCheckInteger(LuaState L, lua_Integer pos)
		{
			return L.Args.AsInt(pos, L.FunctionName).Value;
		}

		protected static void LuaLArgCheck(LuaState L, bool condition, lua_Integer argNum, string message)
		{
			if (!condition)
				throw new ScriptRuntimeException(message);
		}

		protected static lua_Integer LuaLCheckInt(LuaState L, lua_Integer argNum)
		{
			return LuaLCheckInteger(L, argNum);
		}

		protected static lua_Integer LuaGetTop(LuaState L)
		{
			return L.Args.Count;
		}

		protected static lua_Integer LuaLError(LuaState luaState, string message, params object[] args)
		{
			throw new ScriptRuntimeException(message, args);
		}

		protected static void LuaLAddChar(LuaLBuffer b, char p)
		{
			b.StringBuilder.Append(p);
		}

		protected static void LuaLBuffInit(LuaState L, LuaLBuffer b)
		{
		}

		protected static void LuaPushLiteral(LuaState L, string literalString)
		{
			L.Stack.Push(DynValue.NewString(literalString));
		}

		protected static void LuaLPushResult(LuaLBuffer b)
		{
			LuaPushLiteral(b.LuaState, b.StringBuilder.ToString());
		}

		protected static void LuaPushLString(LuaState L, CharPtr s, uint len)
		{
			throw new NotImplementedException();
		}

		protected static void LuaLCheckStack(LuaState L, lua_Integer n, string message)
		{
			//throw new NotImplementedException();
		}


		protected static string LUA_QL(string p)
		{
			return "'" + p + "'";
		}


		protected static lua_Integer memcmp(CharPtr ptr1, CharPtr ptr2, uint size)
		{
			return memcmp(ptr1, ptr2, (int)size);
		}

		protected static int memcmp(CharPtr ptr1, CharPtr ptr2, int size)
		{
			for (int i = 0; i < size; i++)
				if (ptr1[i] != ptr2[i])
				{
					if (ptr1[i] < ptr2[i])
						return -1;
					else
						return 1;
				}
			return 0;
		}

		protected static CharPtr memchr(CharPtr ptr, char c, uint count)
		{
			for (uint i = 0; i < count; i++)
				if (ptr[i] == c)
					return new CharPtr(ptr.chars, (int)(ptr.index + i));
			return null;
		}

		protected static CharPtr strpbrk(CharPtr str, CharPtr charset)
		{
			for (int i = 0; str[i] != '\0'; i++)
				for (int j = 0; charset[j] != '\0'; j++)
					if (str[i] == charset[j])
						return new CharPtr(str.chars, str.index + i);
			return null;
		}

		protected static void LuaPushNil(LuaState L)
		{
			L.Stack.Push(DynValue.Nil);
		}

		protected static void LuaAssert(bool p)
		{
			if (!p)
				throw new InternalErrorException("LuaAssert failed!");
		}




	}
}

#endif
