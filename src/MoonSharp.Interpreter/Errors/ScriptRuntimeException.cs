using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class ScriptRuntimeException : InterpreterException
	{
		public ScriptRuntimeException(Exception ex)
			: base(ex)
		{
		}

		public ScriptRuntimeException(string format, params object[] args)
			: base(format, args)
		{

		}

		public static ScriptRuntimeException ArithmeticOnNonNumber(DynValue l, DynValue r = null)
		{
			if (l.Type != DataType.Number && l.Type != DataType.String)
				return new ScriptRuntimeException("attempt to perform arithmetic on a {0} value", l.Type.ToLuaTypeString());
			else if (r != null && r.Type != DataType.Number && r.Type != DataType.String)
				return new ScriptRuntimeException("attempt to perform arithmetic on a {0} value", r.Type.ToLuaTypeString());
			else if (l.Type == DataType.String || (r != null && r.Type == DataType.String))
				return new ScriptRuntimeException("attempt to perform arithmetic on a string value");
			else
				throw new InternalErrorException("ArithmeticOnNonNumber - both are numbers/strings");
		}


		public static ScriptRuntimeException ConcatOnNonString(DynValue l, DynValue r)
		{
			if (l.Type != DataType.Number && l.Type != DataType.String)
				return new ScriptRuntimeException("attempt to concatenate a {0} value", l.Type.ToLuaTypeString());
			else if (r != null && r.Type != DataType.Number && r.Type != DataType.String)
				return new ScriptRuntimeException("attempt to concatenate a {0} value", r.Type.ToLuaTypeString());
			else
				throw new InternalErrorException("ConcatOnNonString - both are numbers/strings");
		}

		public static ScriptRuntimeException LenOnInvalidType(DynValue r)
		{
			return new ScriptRuntimeException("attempt to get length of a {0} value", r.Type.ToLuaTypeString());
		}

		public static ScriptRuntimeException CompareInvalidType(DynValue l, DynValue r)
		{
			if (l.Type.ToLuaTypeString() == r.Type.ToLuaTypeString())
				return new ScriptRuntimeException("attempt to compare two {0} values", l.Type.ToLuaTypeString());
			else
				return new ScriptRuntimeException("attempt to compare {0} with {1}", l.Type.ToLuaTypeString(), r.Type.ToLuaTypeString());
		}

		public static ScriptRuntimeException BadArgument(int argNum, string funcName, DataType expected, DataType got, bool allowNil)
		{
			return BadArgument(argNum, funcName, expected.ToString().ToLowerInvariant(), got.ToString().ToLowerInvariant(), allowNil);
		}

		public static ScriptRuntimeException BadArgument(int argNum, string funcName, string expected, string got, bool allowNil)
		{
			return new ScriptRuntimeException("bad argument #{0} to '{1}' ({2}{3} expected, got {4})",
				argNum + 1, funcName, allowNil ? "nil or " : "", expected, got);
		}


		public static ScriptRuntimeException IndexType(DynValue obj)
		{
			return new ScriptRuntimeException("attempt to index a {0} value", obj.Type.ToLuaTypeString());
		}

		public static ScriptRuntimeException LoopInIndex()
		{
			return new ScriptRuntimeException("loop in gettable");
		}

		public static ScriptRuntimeException LoopInNewIndex()
		{
			return new ScriptRuntimeException("loop in settable");
		}

		public static ScriptRuntimeException TableIndexIsNil()
		{
			return new ScriptRuntimeException("table index is nil");
		}

		public static ScriptRuntimeException TableIndexIsNaN()
		{
			return new ScriptRuntimeException("table index is NaN");
		}

		public static ScriptRuntimeException ConvertToNumberFailed(int stage)
		{
			switch (stage)
			{
				case 1:
					return new ScriptRuntimeException("'for' initial value must be a number");
				case 2:
					return new ScriptRuntimeException("'for' step must be a number");
				case 3:
					return new ScriptRuntimeException("'for' limit must be a number");
				default:
					return new ScriptRuntimeException("value must be a number");
			}
		}

		public static ScriptRuntimeException ConvertObjectFailed(object obj)
		{
			return new ScriptRuntimeException("cannot convert clr type {0}", obj.GetType());
		}

		public static ScriptRuntimeException ConvertObjectFailed(DataType t)
		{
			return new ScriptRuntimeException("cannot convert a {0} to a clr type", t.ToString().ToLowerInvariant());
		}

		public static ScriptRuntimeException UserDataArgumentTypeMismatch(DataType t, Type clrType)
		{
			return new ScriptRuntimeException("cannot find a conversion from a moon# {0} to a clr {1}", t.ToString().ToLowerInvariant(), clrType.FullName);
		}

		public static ScriptRuntimeException UserDataMissingField(string typename, string fieldname)
		{
			return new ScriptRuntimeException("cannot access field {0} of userdata<{1}>", fieldname, typename);
		}
	}
}
