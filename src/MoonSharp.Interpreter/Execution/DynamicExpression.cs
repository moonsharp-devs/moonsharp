using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter
{
	public class DynamicExpression : IScriptPrivateResource
	{
		DynamicExprExpression m_Exp;
		DynValue m_Constant;

		public readonly string ExpressionCode;

		internal DynamicExpression(Script S, string strExpr, DynamicExprExpression expr)
		{
			ExpressionCode = strExpr;
			OwnerScript = S;
			m_Exp = expr;
		}

		internal DynamicExpression(Script S, string strExpr, DynValue constant)
		{
			ExpressionCode = strExpr;
			OwnerScript = S;
			m_Constant = constant;
		}

		public DynValue Evaluate(ScriptExecutionContext context)
		{
			if (m_Constant != null)
				return m_Constant;

			return m_Exp.Eval(context);
		}

		public SymbolRef FindSymbol(ScriptExecutionContext context)
		{
			if (m_Exp != null)
				return m_Exp.FindDynamic(context);
			else
				return null;
		}

		public Script OwnerScript
		{
			get;
			private set;
		}

		public bool IsConstant()
		{
			return m_Constant != null;
		}

		public override int GetHashCode()
		{
			return ExpressionCode.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			DynamicExpression o = obj as DynamicExpression;
			
			if (o == null)
				return false;

			return o.ExpressionCode == this.ExpressionCode;
		}

	}
}
