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
		string m_FriendlyName;

		bool m_Local;
		FunctionDefinitionExpression m_FuncDef;

		public FunctionDefinitionStatement(LuaParser.Stat_localfuncdefContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Local = true;
			m_FuncName = lcontext.Scope.TryDefineLocal(context.NAME().GetText());
			m_FuncDef = new FunctionDefinitionExpression(context.funcbody(), lcontext);

			m_FriendlyName = string.Format("{0} (local)", m_FuncName.i_Name);
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

				m_FriendlyName = fnname.Text + "." + string.Join(".", m_TableAccessors.ToArray());

				if (nameOfMethodAccessor != null)
					m_FriendlyName += ":" + nameOfMethodAccessor;
			}
			else
			{
				m_FuncName = SymbolRef.Global(fnname.Text);
				m_FriendlyName = fnname.Text;
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



		public override void Compile(Execution.VM.ByteCode bc)
		{
			if (m_Local)
			{
				bc.Symbol(m_FuncName);
				bc.Literal(DynValue.Nil);
				bc.Store();
				bc.Symbol(m_FuncName);
				m_FuncDef.Compile(bc, () => bc.Store(), m_FriendlyName);
				return;
			}
			else if (m_MethodName == null)
			{
				bc.Symbol(m_FuncName);
				m_FuncDef.Compile(bc, () => bc.Store(), m_FriendlyName);
				return;
			}
			else
			{
				bc.Load(m_FuncName);

				foreach (string str in m_TableAccessors)
				{
					bc.Literal(DynValue.NewString(str));
					bc.Index();
				}

				bc.Literal(DynValue.NewString(m_MethodName));

				bc.IndexRef();

				m_FuncDef.Compile(bc, () => bc.Nop(null), m_FriendlyName);

				bc.Store();
			}
		}

	}
}
