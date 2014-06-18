using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace MoonSharp.Interpreter.Execution
{
	public sealed class RValue
	{
		static int s_RefIDCounter = 0;
		private int m_RefID = ++s_RefIDCounter;

		public int ReferenceID { get { return m_RefID; } }

		public DataType Type { get; private set; }
		public Closure Function { get; private set; }
		public double Number { get; private set; }
		public RValue[] Tuple { get; private set; }
		public Table Table { get; private set; }
		public bool Boolean { get; private set; }
		public string String { get; private set; }
		public bool ReadOnly { get; internal set; }
		public LRef Symbol { get; private set; }
		public CallbackFunction Callback { get; set; }
		public RValue Meta { get; set; }

		private int m_HashCode = -1;

		public RValue()
		{
			Type = DataType.Nil;
		}

		public RValue(bool v)
		{
			Boolean = v;
			Type = DataType.Boolean;
		}
		public RValue(double num)
		{
			Number = num;
			Type = DataType.Number;
			m_HashCode = -1;
		}

		public RValue(LRef symbol)
		{
			this.Symbol = symbol;
			this.Type = DataType.Symbol;
		}

		public RValue(string str)
		{
			String = str;
			Type = DataType.String;
			m_HashCode = -1;
		}


		public RValue(Closure function)
		{
			Function = function;
			Type = DataType.Function;
			m_HashCode = -1;
		}

		public RValue(CallbackFunction function)
		{
			Callback = function;
			Type = DataType.ClrFunction;
			m_HashCode = -1;
		}

		public RValue(Table table)
		{
			Table = table;
			Type = DataType.Table;
			m_HashCode = -1;
		}

		public RValue(RValue tailFn, params RValue[] args)
		{
			Meta = tailFn;
			Tuple = args;
			Type = DataType.TailCallRequest;
		}

		public RValue(RValue[] tuple)
		{
			Tuple = tuple;
			Type = DataType.Tuple;
			m_HashCode = -1;
		}

		public RValue AsReadOnly()
		{
			if (ReadOnly)
				return this;
			else
			{
				RValue v = Clone();
				v.ReadOnly = true;
				return v;
			}
		}

		public RValue Clone()
		{
			if (this.Type == DataType.Symbol)
				throw new ArgumentException("Can't clone Symbol values");

			RValue v = new RValue();
			v.Boolean = this.Boolean;
			v.Callback = this.Callback;
			v.Function = this.Function;
			v.Number = this.Number;
			v.ReadOnly = this.ReadOnly;
			v.String = this.String;
			v.Table = this.Table;
			v.Tuple = this.Tuple;
			v.Type = this.Type;
			v.Meta = this.Meta;
			v.m_HashCode = this.m_HashCode;
			return v;
		}

		public RValue CloneAsWritable()
		{
			if (this.Type == DataType.Symbol)
				throw new ArgumentException("Can't clone Symbol values");

			RValue v = new RValue();
			v.Boolean = this.Boolean;
			v.Function = this.Function;
			v.Callback = this.Callback;
			v.Number = this.Number;
			v.ReadOnly = false;
			v.String = this.String;
			v.Table = this.Table;
			v.Tuple = this.Tuple;
			v.Type = this.Type;
			v.Meta = this.Meta;
			v.m_HashCode = this.m_HashCode;
			return v;
		}


		public static RValue Nil { get; private set; }
		public static RValue True { get; private set; }
		public static RValue False { get; private set; }
		static RValue()
		{
			Nil = new RValue().AsReadOnly();
			True = new RValue(true).AsReadOnly();
			False = new RValue(false).AsReadOnly();
		}

		public string AsSimpleString()
		{
			RValue rv = ToSimplestValue();
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



		public string AsString()
		{
			switch (Type)
			{
				case DataType.String:
					return String;
				case DataType.Table:
					return "(Table)";
				case DataType.Tuple:
					return string.Join("\t", Tuple.Select(t => t.AsString()).ToArray());
				case DataType.Symbol:
					return "(Symbol -- INTERNAL!)";
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
				case DataType.Symbol:
					return Symbol.ToString();
				case DataType.UserData:
					return "(UserData)";
				case DataType.Thread:
					return "(Thread)";
				default:
					return "(???)";
			}
		}

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

		public override bool Equals(object obj)
		{
			RValue other = obj as RValue;

			if (other == null) return false;
			if (other.Type != this.Type) return false;

			if (other.Meta != this.Meta) return false;

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

		public IEnumerable<RValue> UnpackedTuple()
		{
			if (this.Type == DataType.Tuple)
			{
				return Tuple.SelectMany(rv => rv.UnpackedTuple());
			}
			else
			{
				return new RValue[] { this };
			}
		}

		public static RValue FromPotentiallyNestedTuple(RValue[] values)
		{
			return new RValue(values.SelectMany(vv => vv.UnpackedTuple()).ToArray());
		}

		public RValue[] ToArrayOfValues()
		{
			if (Type == DataType.Tuple)
				return Tuple;
			else
				return new RValue[] { this };
		}

		public double? AsNumber()
		{
			RValue rv = ToSimplestValue();
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

		public RValue AsBoolean()
		{
			RValue rv = ToSimplestValue();
			if (rv.Type == DataType.Boolean)
				return rv;
			else return new RValue((rv.Type != DataType.Nil));
		}

		public bool TestAsBoolean()
		{
			RValue rv = ToSimplestValue();
			if (rv.Type == DataType.Boolean)
				return rv.Boolean;
			else return (rv.Type != DataType.Nil);
		}

		public RValue ToSingleValue()
		{
			if (Type != DataType.Tuple)
				return this;

			if (Tuple.Length == 0)
				return RValue.Nil;

			return Tuple[0].ToSimplestValue();
		}



		public RValue ToSimplestValue()
		{
			if (Type != DataType.Tuple)
				return this;

			if (Tuple.Length == 0)
				return RValue.Nil;

			if (Tuple.Length == 1)
				return Tuple[0].ToSimplestValue();

			return this;
		}

		public void Assign(RValue value)
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
			this.Meta = value.Meta;
			this.m_HashCode = -1;
		}



		public RValue GetLength()
		{
			if (this.Type == DataType.Table)
				return new RValue(this.Table.Length);
			if (this.Type == DataType.String)
				return new RValue(this.String.Length);

			throw new ScriptRuntimeException(null, "Can't get length of type {0}", this.Type);
		}

		public bool IsNil()
		{
			return this.Type == DataType.Nil;
		}

		public bool IsNilOrNan()
		{
			return (this.Type == DataType.Nil) || (this.Type == DataType.Number && double.IsNaN(this.Number));
		}

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
