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
		SymbolRef m_FuncSymbol;
		List<string> m_TableAccessors;
		string m_MethodName;
		string m_FriendlyName;

		bool m_Local;
		FunctionDefinitionExpression m_FuncDef;

		public FunctionDefinitionStatement(LuaParser.Stat_localfuncdefContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			m_Local = true;
			m_FuncSymbol = lcontext.Scope.TryDefineLocal(context.NAME().GetText());
			m_FuncDef = new FunctionDefinitionExpression(context.funcbody(), lcontext);

			m_FriendlyName = string.Format("{0} (local)", m_FuncSymbol.i_Name);
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
				m_FuncSymbol = lcontext.Scope.Find(fnname.Text);

				m_FriendlyName = fnname.Text + "." + string.Join(".", m_TableAccessors.ToArray());

				if (nameOfMethodAccessor != null)
					m_FriendlyName += ":" + nameOfMethodAccessor;
			}
			else
			{
				m_FuncSymbol = SymbolRef.Global(fnname.Text);
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
				bc.Emit_Literal(DynValue.Nil);
				bc.Emit_Store(m_FuncSymbol, 0, 0);
				m_FuncDef.Compile(bc, () => SetFunction(bc, 2), m_FriendlyName);
			}
			else if (m_MethodName == null)
			{
				m_FuncDef.Compile(bc, () => SetFunction(bc, 1), m_FriendlyName);
			}
			else
			{
				m_FuncDef.Compile(bc, () => SetMethod(bc), m_FriendlyName);
			}
		}

		private int SetMethod(Execution.VM.ByteCode bc)
		{
			int cnt = 0;

			cnt += bc.Emit_Load(m_FuncSymbol);

			foreach (string str in m_TableAccessors)
			{
				bc.Emit_Literal(DynValue.NewString(str));
				bc.Emit_Index();
				cnt += 2;
			}

			bc.Emit_Literal(DynValue.NewString(m_MethodName));

			bc.Emit_IndexSet(0, 0);

			return 2 + cnt;
		}

		private int SetFunction(Execution.VM.ByteCode bc, int numPop)
		{
			int num = bc.Emit_Store(m_FuncSymbol, 0, 0);
			bc.Emit_Pop(numPop);
			return num + 1;
		}

	}
}
