#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.Behaviours
{
	public abstract class Behaviour : IFunction
	{
		public const int STANDARD_RELEVANCE = 0;
		public const int METATABLE_RELEVANCE = 10;

		public abstract bool IsCompatibleWithType(DataType t);
		public abstract RValue OnAdd(RValue op1, RValue op2);
		public abstract RValue OnSub(RValue op1, RValue op2);
		public abstract RValue OnMul(RValue op1, RValue op2);
		public abstract RValue OnDiv(RValue op1, RValue op2);
		public abstract RValue OnMod(RValue op1, RValue op2);
		public abstract RValue OnPow(RValue op1, RValue op2);
		public abstract RValue OnNeg(RValue op1);
		public abstract RValue OnCat(RValue op1, RValue op2);
		public abstract RValue OnLen(RValue op1);
		public abstract RValue OnEq(RValue op1, RValue op2);
		public abstract RValue OnLtEq(RValue op1, RValue op2);
		public abstract RValue OnLt(RValue op1, RValue op2);
		public abstract RValue Index(RValue table, RValue key);
		public abstract RValue NewIndex(RValue table, RValue key);
		public abstract RValue Invoke(RuntimeScope scope, RValue[] args);
		public abstract string AsString(RValue op);
		public abstract double AsNumber(RValue op);
		public abstract bool AsBoolean(RValue op);

		public abstract int Relevance { get; }


		public static RValue Dispatch_OnAdd(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnAdd(op1, op2) : op2.Behaviour.OnAdd(op1, op2);
		}
		public static RValue Dispatch_OnSub(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnSub(op1, op2) : op2.Behaviour.OnSub(op1, op2);
		}
		public static RValue Dispatch_OnMul(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnMul(op1, op2) : op2.Behaviour.OnMul(op1, op2);
		}
		public static RValue Dispatch_OnDiv(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnDiv(op1, op2) : op2.Behaviour.OnDiv(op1, op2);
		}
		public static RValue Dispatch_OnMod(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnMod(op1, op2) : op2.Behaviour.OnMod(op1, op2);
		}
		public static RValue Dispatch_OnPow(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnPow(op1, op2) : op2.Behaviour.OnPow(op1, op2);
		}
		public static RValue Dispatch_OnCat(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnCat(op1, op2) : op2.Behaviour.OnCat(op1, op2);
		}
		public static RValue Dispatch_OnEq(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnEq(op1, op2) : op2.Behaviour.OnEq(op1, op2);
		}
		public static RValue Dispatch_OnLtEq(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnLtEq(op1, op2) : op2.Behaviour.OnLtEq(op1, op2);
		}
		public static RValue Dispatch_OnLt(RValue op1, RValue op2)
		{
			return (op1.Behaviour.Relevance > op2.Behaviour.Relevance) ? op1.Behaviour.OnLt(op1, op2) : op2.Behaviour.OnLt(op1, op2);
		}





		public VM.Chunk ByteCode
		{
			get { throw new NotImplementedException(); }
		}
	}
}

#endif