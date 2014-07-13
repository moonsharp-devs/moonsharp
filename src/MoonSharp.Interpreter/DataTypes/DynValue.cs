using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// A class representing a value in a Lua/Moon# script.
	/// </summary>
	public sealed class DynValue
	{
		static int s_RefIDCounter = 0;
		private int m_RefID = ++s_RefIDCounter;
		private int m_HashCode = -1;


		/// <summary>
		/// Gets a unique reference identifier. This is guaranteed to be unique only in a single Script object as it's not thread-safe.
		/// </summary>
		public int ReferenceID { get { return m_RefID; } }

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public DataType Type { get; private set; }
		/// <summary>
		/// Gets the function (valid only if the <seealso cref="Type"/> is <seealso cref="DataType.Function"/>)
		/// </summary>
		public Closure Function { get; private set; }
		/// <summary>
		/// Gets the numeric value (valid only if the <seealso cref="Type"/> is <seealso cref="DataType.Number"/>)
		/// </summary>
		public double Number { get; private set; }
		/// <summary>
		/// Gets the values in the tuple (valid only if the <seealso cref="Type"/> is Tuple).
		/// This field is currently also used to hold arguments in values whose <seealso cref="Type"/> is <seealso cref="DataType.TailCallRequest"/>.
		/// </summary>
		public DynValue[] Tuple { get; private set; }
		/// <summary>
		/// Gets the table (valid only if the <seealso cref="Type"/> is <seealso cref="DataType.Table"/>)
		/// </summary>
		public Table Table { get; private set; }
		/// <summary>
		/// Gets the boolean value (valid only if the <seealso cref="Type"/> is <seealso cref="DataType.Boolean"/>)
		/// </summary>
		public bool Boolean { get; private set; }
		/// <summary>
		/// Gets the string value (valid only if the <seealso cref="Type"/> is <seealso cref="DataType.String"/>)
		/// </summary>
		public string String { get; private set; }
		/// <summary>
		/// Gets the CLR callback (valid only if the <seealso cref="Type"/> is <seealso cref="DataType.Callback"/>)
		/// </summary>
		public CallbackFunction Callback { get; set; }
		/// <summary>
		/// Gets the meta-table associated with this instance.
		/// </summary>
		public Table MetaTable { get; set; }
		/// <summary>
		/// Gets or sets the user object, if this value is userdata
		/// </summary>
		public object UserObject { get; set; }

		/// <summary>
		/// Returns true if this instance is write protected.
		/// </summary>
		public bool ReadOnly { get; internal set; }


		/// <summary>
		/// Creates a new writable value initialized to Nil.
		/// </summary>
		public static DynValue NewNil()
		{
			return new DynValue();
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified boolean.
		/// </summary>
		public static DynValue NewBoolean(bool v)
		{
			return new DynValue()
			{
				Boolean = v,
				Type = DataType.Boolean,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified number.
		/// </summary>
		public static DynValue NewNumber(double num)
		{
			return new DynValue()
			{
				Number = num,
				Type = DataType.Number,
				m_HashCode = -1,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified string.
		/// </summary>
		public static DynValue NewString(string str)
		{
			return new DynValue()
			{
				String = str,
				Type = DataType.String,
			};
		}


		/// <summary>
		/// Creates a new writable value initialized to the specified closure (function).
		/// </summary>
		public static DynValue NewClosure(Closure function)
		{
			return new DynValue()
			{
				Function = function,
				Type = DataType.Function,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified CLR callback.
		/// </summary>
		public static DynValue NewCallback(Func<ScriptExecutionContext, CallbackArguments, DynValue> callBack)
		{
			return new DynValue()
			{
				Callback = new CallbackFunction(callBack),
				Type = DataType.ClrFunction,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified CLR callback.
		/// </summary>
		public static DynValue NewCallback(CallbackFunction function)
		{
			return new DynValue()
			{
				Callback = function,
				Type = DataType.ClrFunction,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to the specified table.
		/// </summary>
		public static DynValue NewTable(Table table)
		{
			return new DynValue()
			{
				Table = table,
				Type = DataType.Table,
			};
		}

		/// <summary>
		/// Creates a new writable value initialized to an empty table.
		/// </summary>
		public static DynValue NewTable(Script script)
		{
			return NewTable(new Table(script));
		}

		/// <summary>
		/// Creates a new request for a tail call. This is the preferred way to execute Lua/Moon# code from a callback,
		/// although it's not always possible to use it. When a function (callback or script closure) returns a
		/// TailCallRequest, the bytecode processor immediately executes the function contained in the request.
		/// By executing script in this way, a callback function ensures it's not on the stack anymore and thus a number
		/// of functionality (state savings, coroutines, etc) keeps working at full power.
		/// </summary>
		/// <param name="tailFn">The function to be called.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		public static DynValue NewTailCallReq(DynValue tailFn, params DynValue[] args)
		{
			return new DynValue()
			{
				UserObject = new TailCallData()
				{
					Args = args,
					Function = tailFn,
					Mode = CallMode.Normal 
				},
				Type = DataType.TailCallRequest,
			};
		}

		/// <summary>
		/// Creates a new request for a tail call. This is the preferred way to execute Lua/Moon# code from a callback,
		/// although it's not always possible to use it. When a function (callback or script closure) returns a
		/// TailCallRequest, the bytecode processor immediately executes the function contained in the request.
		/// By executing script in this way, a callback function ensures it's not on the stack anymore and thus a number
		/// of functionality (state savings, coroutines, etc) keeps working at full power.
		/// </summary>
		/// <param name="tailFn">The data for the tail call.</param>
		/// <returns></returns>
		public static DynValue NewTailCallReq(TailCallData tailCallData)
		{
			return new DynValue()
			{
				UserObject = tailCallData,
				Type = DataType.TailCallRequest,
			};
		}

		/// <summary>
		/// Creates a new tuple initialized to the specified values.
		/// </summary>
		public static DynValue NewTuple(params DynValue[] values)
		{
			if (values.Length == 0)
				return DynValue.NewNil();

			if (values.Length == 1)
				return values[0];

			return new DynValue()
			{
				Tuple = values,
				Type = DataType.Tuple,
			};
		}

		/// <summary>
		/// Creates a new tuple initialized to the specified values - which can be potentially other tuples
		/// </summary>
		public static DynValue NewTupleNested(params DynValue[] values)
		{
			if (!values.Any(v => v.Type == DataType.Tuple))
				return NewTuple(values);

			if (values.Length == 1)
				return values[0];

			List<DynValue> vals = new List<DynValue>();

			foreach (var v in values)
			{
				if (v.Type == DataType.Tuple)
					vals.AddRange(v.Tuple);
				else
					vals.Add(v);
			}

			return new DynValue()
			{
				Tuple = vals.ToArray(),
				Type = DataType.Tuple,
			};
		}

		/// <summary>
		/// Creates a new value initialized to the specified coroutine index.
		/// </summary>
		internal static DynValue NewCoroutine(int coroutineIdx)
		{
			return new DynValue()
			{
				Number = coroutineIdx,
				Type = DataType.Thread
			};
		}


		/// <summary>
		///  Creates a new value initialized to the specified CLR object
		/// </summary>
		/// <param name="obj">The CLR object.</param>
		/// <param name="metatable">Optional - the metatable.</param>
		public static DynValue NewObject(object obj, Table metatable = null)
		{
			return new DynValue()
			{
				UserObject = obj,
				Type = DataType.UserData,
				MetaTable = metatable
			};
		}



		/// <summary>
		/// Returns this value as readonly - eventually cloning it in the process if it isn't readonly to start with.
		/// </summary>
		public DynValue AsReadOnly()
		{
			if (ReadOnly)
				return this;
			else
			{
				DynValue v = Clone();
				v.ReadOnly = true;
				return v;
			}
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <exception cref="System.ArgumentException">Can't clone Symbol values</exception>
		public DynValue Clone()
		{
			DynValue v = new DynValue();
			v.Boolean = this.Boolean;
			v.Callback = this.Callback;
			v.Function = this.Function;
			v.Number = this.Number;
			v.ReadOnly = this.ReadOnly;
			v.String = this.String;
			v.Table = this.Table;
			v.Tuple = this.Tuple;
			v.Type = this.Type;
			v.MetaTable = this.MetaTable;
			v.m_HashCode = this.m_HashCode;
			return v;
		}

		/// <summary>
		/// Clones this instance, returning a writable copy.
		/// </summary>
		/// <exception cref="System.ArgumentException">Can't clone Symbol values</exception>
		public DynValue CloneAsWritable()
		{
			DynValue v = new DynValue();
			v.Boolean = this.Boolean;
			v.Function = this.Function;
			v.Callback = this.Callback;
			v.Number = this.Number;
			v.ReadOnly = false;
			v.String = this.String;
			v.Table = this.Table;
			v.Tuple = this.Tuple;
			v.Type = this.Type;
			v.MetaTable = this.MetaTable;
			v.m_HashCode = this.m_HashCode;
			return v;
		}


		/// <summary>
		/// A preinitialized, readonly instance, equaling Nil
		/// </summary>
		public static DynValue Nil { get; private set; }
		/// <summary>
		/// A preinitialized, readonly instance, equaling True
		/// </summary>
		public static DynValue True { get; private set; }
		/// <summary>
		/// A preinitialized, readonly instance, equaling False
		/// </summary>
		public static DynValue False { get; private set; }


		static DynValue()
		{
			Nil = new DynValue().AsReadOnly();
			True = DynValue.NewBoolean(true).AsReadOnly();
			False = DynValue.NewBoolean(false).AsReadOnly();
		}


		/// <summary>
		/// Returns a string which is what it's expected to be output by the print function applied to this value.
		/// </summary>
		public string ToPrintString()
		{
			switch (Type)
			{
				case DataType.String:
					return String;
				case DataType.Table:
					return "(Table)";
				case DataType.Tuple:
					return string.Join("\t", Tuple.Select(t => t.ToPrintString()).ToArray());
				case DataType.TailCallRequest:
					return "(TailCallRequest -- INTERNAL!)";
				case DataType.UserData:
					return "(UserData)";
				case DataType.Thread:
					return "(Thread)";
				default:
					return ToString();
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			switch (Type)
			{
				case DataType.Nil:
					return "nil";
				case DataType.Boolean:
					return Boolean.ToString().ToLower();
				case DataType.Number:
					return Number.ToString();
				case DataType.String:
					return "\"" + String + "\"";
				case DataType.Function:
					return string.Format("(Function {0:X8})", Function.ByteCodeLocation);
				case DataType.ClrFunction:
					return string.Format("(Function CLR)", Function);
				case DataType.Table:
					return "(Table)";
				case DataType.Tuple:
					return string.Join(", ", Tuple.Select(t => t.ToString()).ToArray());
				case DataType.TailCallRequest:
					return "Tail:(" + string.Join(", ", Tuple.Select(t => t.ToString()).ToArray()) + ")";
				case DataType.UserData:
					return "(UserData)";
				case DataType.Thread:
					return "(Thread)";
				default:
					return "(???)";
			}
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			if (m_HashCode != -1)
				return m_HashCode;

			int baseValue = ((int)(Type)) << 27;

			switch (Type)
			{
				case DataType.Nil:
					m_HashCode = 0;
					break;
				case DataType.Boolean:
					m_HashCode = Boolean ? 1 : 2;
					break;
				case DataType.Number:
					m_HashCode = baseValue ^ Number.GetHashCode();
					break;
				case DataType.String:
					m_HashCode = baseValue ^ String.GetHashCode();
					break;
				case DataType.Function:
					m_HashCode = baseValue ^ Function.GetHashCode();
					break;
				case DataType.ClrFunction:
					m_HashCode = baseValue ^ Callback.GetHashCode();
					break;
				case DataType.Table:
					m_HashCode = baseValue ^ Table.GetHashCode();
					break;
				case DataType.Tuple:
				case DataType.TailCallRequest:
					m_HashCode = baseValue ^ Tuple.GetHashCode();
					break;
				case DataType.UserData:
				case DataType.Thread:
				default:
					m_HashCode = 999;
					break;
			}

			return m_HashCode;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			DynValue other = obj as DynValue;

			if (other == null) return false;
			if (other.Type != this.Type) return false;

			if (other.MetaTable != this.MetaTable) return false;

			switch (Type)
			{
				case DataType.Nil:
					return true;
				case DataType.Boolean:
					return Boolean == other.Boolean;
				case DataType.Number:
					return Number == other.Number;
				case DataType.String:
					return String == other.String;
				case DataType.Function:
					return Function == other.Function;
				case DataType.ClrFunction:
					return Callback == other.Callback;
				case DataType.Table:
					return Table == other.Table;
				case DataType.Tuple:
				case DataType.TailCallRequest:
					return Tuple == other.Tuple;
				case DataType.UserData:
				case DataType.Thread:
				default:
					return object.ReferenceEquals(this, other);
			}
		}


		/// <summary>
		/// Casts this DynValue to string, using coercion if the type is number.
		/// </summary>
		/// <returns>The string representation, or null if not number, not string.</returns>
		public string CastToString()
		{
			DynValue rv = ToScalar();
			if (rv.Type == DataType.Number)
			{
				return rv.Number.ToString();
			}
			else if (rv.Type == DataType.String)
			{
				return rv.String;
			}
			return null;
		}

		/// <summary>
		/// Casts this DynValue to a double, using coercion if the type is string.
		/// </summary>
		/// <returns>The string representation, or null if not number, not string or non-convertible-string.</returns>
		public double? CastToNumber()
		{
			DynValue rv = ToScalar();
			if (rv.Type == DataType.Number)
			{
				return rv.Number;
			}
			else if (rv.Type == DataType.String)
			{
				double num;
				if (double.TryParse(rv.String, NumberStyles.Any, CultureInfo.InvariantCulture, out num))
					return num;
			}
			return null;
		}


		/// <summary>
		/// Casts this DynValue to a bool
		/// </summary>
		/// <returns>False if value is false or nil, true otherwise.</returns>
		public bool CastToBool()
		{
			DynValue rv = ToScalar();
			if (rv.Type == DataType.Boolean)
				return rv.Boolean;
			else return (rv.Type != DataType.Nil);
		}

		/// <summary>
		/// Converts a tuple to a scalar value. If it's already a scalar value, this function returns "this".
		/// </summary>
		public DynValue ToScalar()
		{
			if (Type != DataType.Tuple)
				return this;

			if (Tuple.Length == 0)
				return DynValue.Nil;

			return Tuple[0].ToScalar();
		}

		/// <summary>
		/// Performs an assignment, overwriting the value with the specified one.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <exception cref="ScriptRuntimeException">If the value is readonly.</exception>
		public void Assign(DynValue value)
		{
			if (this.ReadOnly)
				throw new ScriptRuntimeException(null, "Assigning on r-value");

			this.Boolean = value.Boolean;
			this.Callback = value.Callback;
			this.Function = value.Function;
			this.Number = value.Number;
			this.ReadOnly = false;
			this.String = value.String;
			this.Table = value.Table;
			this.Tuple = value.Tuple;
			this.Type = value.Type;
			this.MetaTable = value.MetaTable;
			this.m_HashCode = -1;
		}



		/// <summary>
		/// Gets the length of a string or table value.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ScriptRuntimeException">Value is not a table or string.</exception>
		public DynValue GetLength()
		{
			if (this.Type == DataType.Table)
				return DynValue.NewNumber(this.Table.Length);
			if (this.Type == DataType.String)
				return DynValue.NewNumber(this.String.Length);

			throw new ScriptRuntimeException(null, "Can't get length of type {0}", this.Type);
		}

		/// <summary>
		/// Determines whether this instance is nil.
		/// </summary>
		public bool IsNil()
		{
			return this.Type == DataType.Nil;
		}

		/// <summary>
		/// Determines whether is nil or NaN (and thus unsuitable for using as a table key).
		/// </summary>
		public bool IsNilOrNan()
		{
			return (this.Type == DataType.Nil) || (this.Type == DataType.Number && double.IsNaN(this.Number));
		}

		/// <summary>
		/// Changes the numeric value of a number DynValue.
		/// </summary>
		internal void AssignNumber(double num)
		{
			if (this.ReadOnly)
				throw new InternalErrorException(null, "Writing on r-value");

			if (this.Type != DataType.Number)
				throw new InternalErrorException("Can't assign number to type {0}", this.Type);

			this.Number = num;
		}

	}




}
