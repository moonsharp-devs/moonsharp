using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class FunctionDefinitionStatement : Statement
	{
		SymbolRef m_FuncName;
		List<string> m_TableAccessors;
		string m_MethodName;

		bool m_Local;
		FunctionDefinitionExpression m_FuncDef;

		public FunctionDefinitionStatement(LuaParser.Stat_localfuncdefContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Local = true;
			m_FuncName = lcontext.Scope.TryDefineLocal(context.NAME().GetText());
			m_FuncDef = new FunctionDefinitionExpression(context.funcbody(), lcontext);
		}

		public FunctionDefinitionStatement(LuaParser.Stat_funcdefContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Local = false;
			var node_funcname = context.funcname();
			var fnname = node_funcname.fnname;
			var methodaccessor = node_funcname.methodaccessor;
			var tableaccessor = node_funcname.funcnametableaccessor();

			string nameOfMethodAccessor = methodaccessor != null ? methodaccessor.Text : null;
			m_TableAccessors = tableaccessor != null ? tableaccessor.Select(s => s.NAME().GetText()).ToList() : new List<string>();

			m_FuncDef = new FunctionDefinitionExpression(context.funcbody(), lcontext, nameOfMethodAccessor != null);

			if (nameOfMethodAccessor != null || m_TableAccessors.Count > 0)
			{
				m_FuncName = lcontext.Scope.Find(fnname.Text);
			}
			else
			{
				m_FuncName = lcontext.Scope.DefineGlobal(fnname.Text);
			}

			if (nameOfMethodAccessor != null)
			{
				m_MethodName = nameOfMethodAccessor;
			}
			else if (m_TableAccessors.Count > 0)
			{
				m_MethodName = m_TableAccessors[m_TableAccessors.Count - 1];
				m_TableAccessors.RemoveAt(m_TableAccessors.Count - 1);
			}
		}


		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			if (m_Local)
			{
				scope.Assign(m_FuncName, m_FuncDef.Eval(scope));
			}
			else
			{
				if (m_MethodName == null)
				{
					scope.Assign(m_FuncName, m_FuncDef.Eval(scope));
				}
				else
				{
					RValue rvalue = scope.Get(m_FuncName);

					foreach (string str in m_TableAccessors)
					{
						if (rvalue.Type != DataType.Table)
							throw RuntimeError("Table expected, got {0}", rvalue.Type);

						rvalue = rvalue.Table[new RValue(str)];
					}

					if (rvalue.Type != DataType.Table)
						throw RuntimeError("Table expected, got {0}", rvalue.Type);

					rvalue.Table[new RValue(m_MethodName)] = m_FuncDef.Eval(scope);
				}
			}

			return ExecutionFlow.None;
		}


		public override void Compile(Execution.VM.Chunk bc)
		{
			if (m_Local || m_MethodName == null)
			{
				bc.Symbol(m_FuncName);
				m_FuncDef.Compile(bc, () => bc.Store());
				return;
			}
			else
			{
				bc.Load(m_FuncName);

				foreach (string str in m_TableAccessors)
				{
					bc.Literal(new RValue(str));
					bc.Index();
				}

				bc.Literal(new RValue(m_MethodName));

				bc.IndexRef();

				m_FuncDef.Compile(bc);

				bc.Store();
			}
		}

	}
}
