using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class Chunk
	{
		public List<Instruction> Code = new List<Instruction>();
		
		static int s_RefIDCounter = 0;
		private int m_RefID = Interlocked.Increment(ref s_RefIDCounter);

		private LoopTracker m_FlowTracker = new LoopTracker();

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

		public Instruction Invoke(int argCount)
		{
			return Emit(new Instruction() { OpCode = OpCode.Call, NumVal = argCount });
		}

		public Instruction Load(SymbolRef symref)
		{
			return Emit(new Instruction() { OpCode = OpCode.Load, Symbol = symref });
		}

		public Instruction Literal(RValue value)
		{
			return Emit(new Instruction() { OpCode = OpCode.Literal, Value = value });
		}

		public Instruction TableInvoke(int argCount, string name)
		{
			return Emit(new Instruction() { OpCode = OpCode.Method, Name = name, NumVal = argCount });
		}

		public Instruction Assign(int cntL, int cntR)
		{
			return Emit(new Instruction() { OpCode = OpCode.Assign, NumVal = cntL, NumVal2 = cntR });
		}

		public Instruction Store()
		{
			return Emit(new Instruction() { OpCode = OpCode.Store });
		}

		public Instruction Symbol(SymbolRef symref)
		{
			return Emit(new Instruction() { OpCode = OpCode.Symbol, Symbol = symref });
		}

		public Instruction Jump(OpCode jumpOpCode, int idx)
		{
			return Emit(new Instruction() { OpCode = jumpOpCode, NumVal = idx });
		}

		public Instruction MkTuple(int cnt)
		{
			return Emit(new Instruction() { OpCode = OpCode.MkTuple, NumVal = cnt });
		}

		public Instruction Operator(OpCode opcode)
		{
			return Emit(new Instruction() { OpCode = opcode });
		}

		public Instruction Reduce()
		{
			return Emit(new Instruction() { OpCode = OpCode.Reduce });
		}

		public Instruction Bool()
		{
			return Emit(new Instruction() { OpCode = OpCode.Bool });
		}

		public Instruction Debug(string str)
		{
			return Emit(new Instruction() { OpCode = OpCode.Debug, Name = str.Substring(0, Math.Min(32, str.Length)) });
		}
		public Instruction Debug(Antlr4.Runtime.Tree.IParseTree parseTree)
		{
			string str = parseTree.GetText();
			return Emit(new Instruction() { OpCode = OpCode.Debug, Name = str.Substring(0, Math.Min(32, str.Length)) });
		}

		public Instruction Enter(RuntimeScopeFrame runtimeScopeFrame)
		{
			//if (runtimeScopeFrame.RestartOfBase)
			//	m_FlowTracker.EnterFunction(runtimeScopeFrame);

			return Emit(new Instruction() { OpCode = OpCode.Enter, Frame = runtimeScopeFrame });
		}

		public Instruction Leave(RuntimeScopeFrame runtimeScopeFrame)
		{
			//if (runtimeScopeFrame.RestartOfBase)
			//	m_FlowTracker.LeaveFunction(runtimeScopeFrame);

			return Emit(new Instruction() { OpCode = OpCode.Leave });
		}

		public Instruction Exit()
		{
			return Emit(new Instruction() { OpCode = OpCode.Exit });
		}

		public Instruction Closure(SymbolRef[] symbols, int jmpnum)
		{
			return Emit(new Instruction() { OpCode = OpCode.Closure, SymbolList = symbols, NumVal = jmpnum });
		}

		public Instruction Args(SymbolRef[] symbols)
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

		public Instruction NSymStor(SymbolRef symb)
		{
			return Emit(new Instruction() { OpCode = OpCode.NSymStor, Symbol = symb });
		}

		public Instruction Incr(int i)
		{
			return Emit(new Instruction() { OpCode = OpCode.Incr, NumVal = i });
		}
	}
}
