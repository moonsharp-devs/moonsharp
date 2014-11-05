using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class TableConstructor : Expression 
	{
		List<Expression> m_PositionalValues = new List<Expression>();
		List<KeyValuePair<Expression, Expression>> m_CtorArgs = new List<KeyValuePair<Expression, Expression>>();

		public TableConstructor(LuaParser.TableconstructorContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			var fieldlist = context.fieldlist();

			if (fieldlist != null)
			{
				foreach (var field in fieldlist.field())
				{
					var keyval = field.keyexp;
					var name = field.NAME();

					if (keyval != null)
					{
						Expression exp = NodeFactory.CreateExpression(keyval, lcontext);

						m_CtorArgs.Add(new KeyValuePair<Expression,Expression>(
							exp,
							NodeFactory.CreateExpression(field.keyedexp, lcontext)));
					}
					else if (name != null)
					{
						m_CtorArgs.Add(new KeyValuePair<Expression, Expression>(
							new LiteralExpression(field, lcontext, DynValue.NewString(name.GetText())),
							NodeFactory.CreateExpression(field.namedexp, lcontext)));
					}
					else 
					{
						m_PositionalValues.Add(NodeFactory.CreateExpression(field.positionalexp, lcontext));
					}
				}

			}
		}



		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_NewTable();

			foreach (var kvp in m_CtorArgs)
			{
				kvp.Key.Compile(bc);
				kvp.Value.Compile(bc);
				bc.Emit_TblInitN();
			}

			for (int i = 0; i < m_PositionalValues.Count; i++ )
			{
				m_PositionalValues[i].Compile(bc);
				bc.Emit_TblInitI(i == m_PositionalValues.Count - 1);
			}
		}


		public override DynValue Eval(ScriptExecutionContext context)
		{
			throw new DynamicExpressionException("Dynamic Expressions cannot define new tables.");
		}
	}
}
