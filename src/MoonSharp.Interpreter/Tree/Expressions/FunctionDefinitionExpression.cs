using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class FunctionDefinitionExpression : Expression, IClosureBuilder
	{
		LRef[] m_ParamNames;
		Statement m_Statement;
		RuntimeScopeFrame m_StackFrame;
		List<LRef> m_Closure = new List<LRef>();
		public object UpvalueCreationTag { get; set; }

		public FunctionDefinitionExpression(LuaParser.AnonfunctiondefContext context, ScriptLoadingContext lcontext, bool pushSelfParam = false)
			: this(context.funcbody(), lcontext, pushSelfParam)
		{
		}

		public LRef CreateUpvalue(BuildTimeScope scope, LRef symbol)
		{
			for (int i = 0; i < m_Closure.Count; i++)
			{
				if (m_Closure[i].i_Name == symbol.i_Name)
				{
					return LRef.Upvalue(symbol.i_Name, i);
				}
			}

			m_Closure.Add(symbol);
			return LRef.Upvalue(symbol.i_Name, m_Closure.Count - 1);
		}

		public FunctionDefinitionExpression(LuaParser.FuncbodyContext context, ScriptLoadingContext lcontext, bool pushSelfParam = false)
			: base(context, lcontext)
		{
			var parlist = context.parlist();
			string[] paramnames;

			if (!pushSelfParam)
			{
				if (parlist != null)
				{
					paramnames = parlist.namelist().NAME()
						.Select(t => t.GetText())
						.ToArray();
				}
				else
				{
					paramnames = new string[0];
				}
			}
			else
			{
				if (parlist != null)
				{
					paramnames = new string[] { "self" }.Union(
						parlist.namelist().NAME()
						.Select(t => t.GetText()))
						.ToArray();
				}
				else
				{
					paramnames = new string[] { "self" };
				}
			}

			lcontext.Scope.EnterClosure(this);

			lcontext.Scope.PushFunction();

			m_ParamNames = DefineArguments(paramnames, lcontext);

			m_Statement = NodeFactory.CreateStatement(context.block(), lcontext);

			m_StackFrame = lcontext.Scope.PopFunction();

			lcontext.Scope.LeaveClosure();
		}

		private LRef[] DefineArguments(string[] paramnames, ScriptLoadingContext lcontext)
		{
			LRef[] ret = new LRef[paramnames.Length];

			for (int i = 0; i < paramnames.Length; i++)
				ret[i] = lcontext.Scope.DefineLocal(paramnames[i]);

			return ret;
		}



		public void Compile(Chunk bc, Action afterDecl, string friendlyName)
		{
			bc.Closure(m_Closure.ToArray(), bc.GetJumpPointForNextInstruction() + 3);
			afterDecl();

			Instruction I = bc.Jump(OpCode.Jump, -1);

			bc.BeginFn(m_StackFrame, friendlyName ?? "<anonymous>");

			if (m_ParamNames.Length > 0)
				bc.Args(m_ParamNames);

			m_Statement.Compile(bc);

			bc.Ret(0);

			I.NumVal = bc.GetJumpPointForNextInstruction();
		}


		public override void Compile(Chunk bc)
		{
			Compile(bc, () => bc.Nop(null), null);
		}

	}
}
