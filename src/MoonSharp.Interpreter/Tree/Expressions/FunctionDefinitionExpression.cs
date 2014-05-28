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

			m_ParamNames = paramnames.Select(n => lcontext.Scope.DefineLocal(n)).ToArray();

			m_Statement = NodeFactory.CreateStatement(context.block(), lcontext);

			m_StackFrame = lcontext.Scope.Pop();

			lcontext.Scope.LeaveClosure();
		}


		public override RValue Eval(RuntimeScope scope)
		{
			//List<RValue> closureValues = m_Closure.Select(s => scope.Get(s)).ToList();
			//scope.EnterClosure(closureValues);
			////var closureFunc = new Closure(scope, m_Statement, m_ParamNames, m_StackFrame, closureValues);
			//scope.LeaveClosure();

			//return new RValue(closureFunc);
			return null;
		}

		public void Compile(Execution.VM.Chunk bc, Action afterDecl)
		{
			bc.Closure(m_Closure.ToArray(), bc.GetJumpPointForNextInstruction() + 3);
			afterDecl();

			Instruction I = bc.Jump(OpCode.Jump, -1);

			bc.Debug(this.TreeNode);

			bc.Enter(m_StackFrame);

			if (m_ParamNames.Length > 0)
				bc.Args(m_ParamNames);

			m_Statement.Compile(bc);
			bc.Leave(m_StackFrame);

			bc.ExitClsr();

			bc.Ret(0);

			I.NumVal = bc.GetJumpPointForNextInstruction();
		}


		public override void Compile(Execution.VM.Chunk bc)
		{
			Compile(bc, () => bc.Nop(null));
		}



	}
}
