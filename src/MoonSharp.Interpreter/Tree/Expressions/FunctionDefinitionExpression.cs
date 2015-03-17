using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

using MoonSharp.Interpreter.Tree.Statements;

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


		public FunctionDefinitionExpression(ScriptLoadingContext lcontext, Table globalContext)
			: this(lcontext, false, globalContext)
		{ }

		public FunctionDefinitionExpression(ScriptLoadingContext lcontext, bool pushSelfParam)
			: this(lcontext, pushSelfParam, null)
		{ }


		private FunctionDefinitionExpression(ScriptLoadingContext lcontext, bool pushSelfParam, Table globalContext)
			: base(lcontext)
		{
			if (globalContext != null)
				CheckTokenType(lcontext, TokenType.Function);

			// here lexer should be at the '('.
			Token openRound = CheckTokenType(lcontext, TokenType.Brk_Open_Round);

			List<string> paramnames = BuildParamList(lcontext, pushSelfParam, openRound);
			// here lexer is at first token of body

			m_Begin = openRound.GetSourceRefUpTo(lcontext.Lexer.Current);

			// create scope
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

			m_Statement = CreateBody(lcontext);

			m_StackFrame = lcontext.Scope.PopFunction();

			lcontext.Source.Refs.Add(m_Begin);
			lcontext.Source.Refs.Add(m_End);

		}

		private Statement CreateBody(ScriptLoadingContext lcontext)
		{
			Statement s = new CompositeStatement(lcontext);

			if (lcontext.Lexer.Current.Type != TokenType.End)
				throw new SyntaxErrorException(lcontext.Lexer.Current, "'end' expected near '{0}'", lcontext.Lexer.Current.Text);

			m_End = lcontext.Lexer.Current.GetSourceRef();

			lcontext.Lexer.Next();
			return s;
		}

		private List<string> BuildParamList(ScriptLoadingContext lcontext, bool pushSelfParam, Token openBracketToken)
		{
			List<string> paramnames = new List<string>();

			// method decls with ':' must push an implicit 'self' param
			if (pushSelfParam)
				paramnames.Add("self");

			while (lcontext.Lexer.Current.Type != TokenType.Brk_Close_Round)
			{
				Token t = lcontext.Lexer.Current;

				if (t.Type == TokenType.Name)
				{
					paramnames.Add(t.Text);
				}
				else if (t.Type == TokenType.VarArgs)
				{
					m_HasVarArgs = true;
					paramnames.Add(WellKnownSymbols.VARARGS);
				}
				else
					throw new SyntaxErrorException(t, "unexpected symbol near '{0}'", t.Text);

				lcontext.Lexer.Next();

				t = lcontext.Lexer.Current;

				if (t.Type == TokenType.Comma)
				{
					lcontext.Lexer.Next();
				}
				else
				{
					CheckMatch(lcontext, openBracketToken, TokenType.Brk_Close_Round, ")");
					break;
				}
			}

			if (lcontext.Lexer.Current.Type == TokenType.Brk_Close_Round)
				lcontext.Lexer.Next();

			return paramnames;
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

		public override DynValue Eval(ScriptExecutionContext context)
		{
			throw new DynamicExpressionException("Dynamic Expressions cannot define new functions.");
		}

		public int CompileBody(ByteCode bc, string friendlyName)
		{
			string funcName = friendlyName ?? ("<" + this.m_Begin.FormatLocation(bc.Script, true) + ">");

			bc.PushSourceRef(m_Begin);

			Instruction I = bc.Emit_Jump(OpCode.Jump, -1);

			Instruction meta = bc.Emit_FuncMeta(funcName);
			int metaip = bc.GetJumpPointForLastInstruction();

			bc.Emit_BeginFn(m_StackFrame);

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

			bc.Emit_Ret(0);

			bc.LoopTracker.Loops.Pop();

			I.NumVal = bc.GetJumpPointForNextInstruction();
			meta.NumVal = bc.GetJumpPointForLastInstruction() - metaip;

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
	}
}
