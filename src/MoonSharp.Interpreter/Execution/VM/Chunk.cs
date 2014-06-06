#define EMIT_DEBUG_OPS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class Chunk
	{
		public List<Instruction> Code = new List<Instruction>();
		internal LoopTracker LoopTracker = new LoopTracker();

		static int s_RefIDCounter = 0;
		private int m_RefID = Interlocked.Increment(ref s_RefIDCounter);


		public int ReferenceID { get { return m_RefID; } }

		private void TrackLoopStart()
		{

		}

		private void TrackLoopEnd()
		{

		}


		public void Dump(string file)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < Code.Count; i++)
			{
				if (Code[i].OpCode == OpCode.Debug)
					sb.AppendFormat("    {0}\n", Code[i]);
				else
					sb.AppendFormat("{0:X8}  {1}\n", i, Code[i]);
			}

			File.WriteAllText(file, sb.ToString());
		}

		public int GetJumpPointForNextInstruction()
		{
			return Code.Count;
		}
		public int GetJumpPointForLastInstruction()
		{
			return Code.Count - 1;
		}

		private Instruction Emit(Instruction c)
		{
			Code.Add(c);
			return c;
		}

		public Instruction Nop(string comment)
		{
			return Emit(new Instruction() { OpCode = OpCode.Nop, Name = comment });
		}

		public Instruction Invalid(string type)
		{
			return Emit(new Instruction() { OpCode = OpCode.Invalid, Name = type });
		}

		public Instruction Pop(int num = 1)
		{
			return Emit(new Instruction() { OpCode = OpCode.Pop, NumVal = num });
		}

		public Instruction Call(int argCount)
		{
			return Emit(new Instruction() { OpCode = OpCode.Call, NumVal = argCount });
		}

		public Instruction Load(LRef symref)
		{
			return Emit(new Instruction() { OpCode = OpCode.Load, Symbol = symref });
		}

		public Instruction Literal(RValue value)
		{
			return Emit(new Instruction() { OpCode = OpCode.Literal, Value = value });
		}

		public Instruction Assign(int cntL, int cntR)
		{
			return Emit(new Instruction() { OpCode = OpCode.Assign, NumVal = cntL, NumVal2 = cntR });
		}

		public Instruction Store()
		{
			return Emit(new Instruction() { OpCode = OpCode.Store });
		}

		public Instruction Symbol(LRef symref)
		{
			return Emit(new Instruction() { OpCode = OpCode.Symbol, Symbol = symref });
		}

		public Instruction Jump(OpCode jumpOpCode, int idx, int optPar = 0)
		{
			return Emit(new Instruction() { OpCode = jumpOpCode, NumVal = idx, NumVal2 = optPar });
		}

		public Instruction MkTuple(int cnt)
		{
			return Emit(new Instruction() { OpCode = OpCode.MkTuple, NumVal = cnt });
		}

		public Instruction Operator(OpCode opcode)
		{
			return Emit(new Instruction() { OpCode = opcode });
		}

		public Instruction Bool()
		{
			return Emit(new Instruction() { OpCode = OpCode.Bool });
		}

		//[Conditional("EMIT_DEBUG_OPS")]
		public void Debug(string str)
		{
			Emit(new Instruction() { OpCode = OpCode.Debug, Name = str.Substring(0, Math.Min(32, str.Length)) });
		}
		public void DebugFn(string fnname)
		{
			Emit(new Instruction() { OpCode = OpCode.DebugFn, Name = fnname });
		}

		//[Conditional("EMIT_DEBUG_OPS")]
		public void Debug(Antlr4.Runtime.Tree.IParseTree parseTree)
		{
			string str = parseTree.GetText();
			Emit(new Instruction() { OpCode = OpCode.Debug, Name = str.Substring(0, Math.Min(32, str.Length)) });
		}

		public Instruction Enter(RuntimeScopeFrame runtimeScopeFrame)
		{
			return Emit(new Instruction() { OpCode = OpCode.Enter, Frame = runtimeScopeFrame });
		}

		public Instruction Leave(RuntimeScopeFrame runtimeScopeFrame)
		{
			return Emit(new Instruction() { OpCode = OpCode.Leave });
		}

		public Instruction Exit(RuntimeScopeFrame runtimeScopeFrame = null)
		{
			return Emit(new Instruction() { OpCode = OpCode.Exit, Frame = runtimeScopeFrame });
		}

		public Instruction Closure(LRef[] symbols, int jmpnum)
		{
			return Emit(new Instruction() { OpCode = OpCode.Closure, SymbolList = symbols, NumVal = jmpnum });
		}

		public Instruction Args(LRef[] symbols)
		{
			return Emit(new Instruction() { OpCode = OpCode.Args, SymbolList = symbols });
		}

		public Instruction ExitClsr()
		{
			return Emit(new Instruction() { OpCode = OpCode.ExitClsr });
		}

		public Instruction Ret(int retvals)
		{
			return Emit(new Instruction() { OpCode = OpCode.Ret, NumVal = retvals });
		}

		public Instruction ToNum()
		{
			return Emit(new Instruction() { OpCode = OpCode.ToNum });
		}

		public Instruction SymStorN(LRef symb)
		{
			return Emit(new Instruction() { OpCode = OpCode.SymStorN, Symbol = symb });
		}

		public Instruction Incr(int i)
		{
			return Emit(new Instruction() { OpCode = OpCode.Incr, NumVal = i });
		}

		public Instruction Index()
		{
			return Emit(new Instruction() { OpCode = OpCode.Index });
		}

		public Instruction IndexRef()
		{
			return Emit(new Instruction() { OpCode = OpCode.IndexRef });
		}
		public Instruction IndexRefN()
		{
			return Emit(new Instruction() { OpCode = OpCode.IndexRefN });
		}

		public Instruction NewTable()
		{
			return Emit(new Instruction() { OpCode = OpCode.NewTable });
		}

		public Instruction TempOp(OpCode opCode, int regNum)
		{
			return Emit(new Instruction() { OpCode = opCode, NumVal = regNum });
		}

		public Instruction IterPrep()
		{
			return Emit(new Instruction() { OpCode = OpCode.IterPrep });
		}

		public Instruction ExpTuple(int stackOffset)
		{
			return Emit(new Instruction() { OpCode = OpCode.ExpTuple, NumVal = stackOffset });
		}

		public Instruction IterUpd()
		{
			return Emit(new Instruction() { OpCode = OpCode.IterUpd });
		}

		public Instruction Method()
		{
			return Emit(new Instruction() { OpCode = OpCode.Method });
		}
	}
}
