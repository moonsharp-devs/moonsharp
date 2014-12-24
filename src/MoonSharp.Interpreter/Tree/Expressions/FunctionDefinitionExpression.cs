using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class FunctionDefinitionExpression : Expression, IClosureBuilder
	{
		SymbolRef[] m_ParamNames = null;
		Statement m_Statement;
		RuntimeScopeFrame m_StackFrame;
		List<SymbolRef> m_Closure = new List<SymbolRef>();
		bool m_HasVarArgs = false;
		Instruction m_ClosureInstruction = null;

		Table m_GlobalEnv;
		SymbolRef m_Env;

		SourceRef m_Begin, m_End;

		public FunctionDefinitionExpression(LuaParser.AnonfunctiondefContext context, ScriptLoadingContext lcontext, bool pushSelfParam = false, Table globalContext = null)
			: this(context.funcbody(), lcontext, context, pushSelfParam, globalContext)
		{ }


		public FunctionDefinitionExpression(LuaParser.Exp_anonfuncContext context, ScriptLoadingContext lcontext, bool pushSelfParam = false, Table globalContext = null)
			: this(context.funcbody(), lcontext, context, pushSelfParam, globalContext)
		{ }

		public FunctionDefinitionExpression(LuaParser.FuncbodyContext context, ScriptLoadingContext lcontext, 
			ParserRuleContext declarationContext,
			bool pushSelfParam = false, Table globalContext = null)
			: base(context, lcontext)
		{
			var parlist = context.parlist();
			List<string> paramnames = new List<string>();

			if (pushSelfParam)
			{
				paramnames.Add("self");
			}


			if (parlist != null)
			{
				var namelist = parlist.namelist();

				if (namelist != null)
				{
					paramnames.AddRange(namelist.NAME()
						.Select(t => t.GetText()));
				}
			}

			m_HasVarArgs = (parlist != null && parlist.vararg() != null);

			if (m_HasVarArgs)
				paramnames.Add(WellKnownSymbols.VARARGS);

			lcontext.Scope.PushFunction(this, m_HasVarArgs);

			if (globalContext != null)
			{
				m_GlobalEnv = globalContext;
				m_Env = lcontext.Scope.TryDefineLocal(WellKnownSymbols.ENV);
			}
			else
			{
				lcontext.Scope.ForceEnvUpValue();
			}

			m_ParamNames = DefineArguments(paramnames, lcontext);

			m_Statement = NodeFactory.CreateStatement(context.block(), lcontext);

			m_StackFrame = lcontext.Scope.PopFunction();

			m_Begin = BuildSourceRef(declarationContext.Start, context.PAREN_CLOSE().Symbol);
			m_End = BuildSourceRef(context.Stop, context.END());
		}

		public SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol)
		{
			for (int i = 0; i < m_Closure.Count; i++)
			{
				if (m_Closure[i].i_Name == symbol.i_Name)
				{
					return SymbolRef.Upvalue(symbol.i_Name, i);
				}
			}

			m_Closure.Add(symbol);

			if (m_ClosureInstruction != null)
			{
				m_ClosureInstruction.SymbolList = m_Closure.ToArray();
			}

			return SymbolRef.Upvalue(symbol.i_Name, m_Closure.Count - 1);
		}

		private SymbolRef[] DefineArguments(List<string> paramnames, ScriptLoadingContext lcontext)
		{
			HashSet<string> names = new HashSet<string>();

			SymbolRef[] ret = new SymbolRef[paramnames.Count];

			for (int i = paramnames.Count - 1; i >= 0; i--)
			{
				if (!names.Add(paramnames[i]))
					paramnames[i] = paramnames[i] + "@" + i.ToString();
				
				ret[i] = lcontext.Scope.DefineLocal(paramnames[i]);
			}

			return ret;
		}

		public int CompileBody(ByteCode bc, string friendlyName)
		{
			bc.PushSourceRef(m_Begin);

			Instruction I = bc.Emit_Jump(OpCode.Jump, -1);

			bc.Emit_BeginFn(m_StackFrame, friendlyName ?? "<" + this.m_Begin.FormatLocation(this.LoadingContext.Script, true) + ">");

			bc.LoopTracker.Loops.Push(new LoopBoundary());

			int entryPoint = bc.GetJumpPointForLastInstruction();

			if (m_GlobalEnv != null)
			{
				bc.Emit_Literal(DynValue.NewTable(m_GlobalEnv));
				bc.Emit_Store(m_Env, 0, 0);
				bc.Emit_Pop();
			} 
			
			if (m_ParamNames.Length > 0)
				bc.Emit_Args(m_ParamNames);

			m_Statement.Compile(bc);

			bc.PopSourceRef();
			bc.PushSourceRef(m_End);

			if (bc.GetLastInstruction().OpCode != OpCode.Ret)
				bc.Emit_Ret(0);

			bc.LoopTracker.Loops.Pop();

			I.NumVal = bc.GetJumpPointForNextInstruction();

			bc.PopSourceRef();

			return entryPoint;
		}

		public int Compile(ByteCode bc, Func<int> afterDecl, string friendlyName)
		{
			using (bc.EnterSource(m_Begin))
			{
				SymbolRef[] symbs = m_Closure
					//.Select((s, idx) => s.CloneLocalAndSetFrame(m_ClosureFrames[idx]))
					.ToArray();

				m_ClosureInstruction = bc.Emit_Closure(symbs, bc.GetJumpPointForNextInstruction());
				int ops = afterDecl();

				m_ClosureInstruction.NumVal += 2 + ops;
			}

			return CompileBody(bc, friendlyName);
		}


		public override void Compile(ByteCode bc)
		{
			Compile(bc, () => 0, null);
		}


		public override DynValue Eval(ScriptExecutionContext context)
		{
			throw new DynamicExpressionException("Dynamic Expressions cannot define new functions.");
		}
	}
}
