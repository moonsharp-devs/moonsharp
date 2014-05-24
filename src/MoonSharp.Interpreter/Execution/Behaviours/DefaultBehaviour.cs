#if false

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.Behaviours
{
	public class DefaultBehaviour : Behaviour 
	{
		public override bool IsCompatibleWithType(DataType t)
		{
			return t == DataType.Number;
		}

		public override RValue OnAdd(RValue op1, RValue op2)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			double n2 = op2.Behaviour.AsNumber(op2);
			return new RValue(n1 + n2);
		}

		public override RValue OnSub(RValue op1, RValue op2)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			double n2 = op2.Behaviour.AsNumber(op2);
			return new RValue(n1 - n2);
		}

		public override RValue OnMul(RValue op1, RValue op2)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			double n2 = op2.Behaviour.AsNumber(op2);
			return new RValue(n1 * n2);
		}

		public override RValue OnDiv(RValue op1, RValue op2)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			double n2 = op2.Behaviour.AsNumber(op2);
			return new RValue(n1 / n2);
		}

		public override RValue OnMod(RValue op1, RValue op2)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			double n2 = op2.Behaviour.AsNumber(op2);
			return new RValue(n1 % n2);

		}

		public override RValue OnPow(RValue op1, RValue op2)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			double n2 = op2.Behaviour.AsNumber(op2);
			return new RValue(Math.Pow(n1, n2));

		}

		public override RValue OnNeg(RValue op1)
		{
			double n1 = op1.Behaviour.AsNumber(op1);
			return new RValue(-n1);
		}

		public override RValue OnCat(RValue op1, RValue op2)
		{
			string s1 = op1.Behaviour.AsString(op1);
			string s2 = op1.Behaviour.AsString(op2);
			return new RValue(s1 + s2);
		}

		public override RValue OnLen(RValue op1)
		{
			throw new NotImplementedException();
		}

		public override RValue OnEq(RValue op1, RValue op2)
		{
			throw new NotImplementedException();
		}

		public override RValue OnLtEq(RValue op1, RValue op2)
		{
			throw new NotImplementedException();
		}

		public override RValue OnLt(RValue op1, RValue op2)
		{
			throw new NotImplementedException();
		}

		public override RValue Index(RValue table, RValue key)
		{
			throw new NotImplementedException();
		}

		public override RValue NewIndex(RValue table, RValue key)
		{
			throw new NotImplementedException();
		}

		public override RValue Invoke(RuntimeScope scope, RValue[] args)
		{
			throw new NotImplementedException();
		}

		public override string AsString(RValue op)
		{
			throw new NotImplementedException();
		}

		public override double AsNumber(RValue op)
		{
			throw new NotImplementedException();
		}

		public override bool AsBoolean(RValue op)
		{
			throw new NotImplementedException();
		}

		public override int Relevance
		{
			get { throw new NotImplementedException(); }
		}
	}
}


#endif