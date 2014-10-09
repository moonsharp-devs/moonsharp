#define EMIT_DEBUG_OPS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class ByteCode : ITrackableReference
	{
		public List<Instruction> Code = new List<Instruction>();
		internal LoopTracker LoopTracker = new LoopTracker();


		#region ITrackableReference

		static int s_RefIDCounter = 0;
		private int m_RefID = Interlocked.Increment(ref s_RefIDCounter);
		public int ReferenceID { get { return m_RefID; } }

		#endregion

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

		public Instruction GetLastInstruction()
		{
			return Code[Code.Count - 1];
		}

		private Instruction AppendInstruction(Instruction c)
		{
			Code.Add(c);
			return c;
		}

		public Instruction Emit_Nop(string comment)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Nop, Name = comment });
		}

		public Instruction Emit_Invalid(string type)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Invalid, Name = type });
		}

		public Instruction Emit_Pop(int num = 1)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Pop, NumVal = num });
		}

		public void Emit_Call(int argCount, string debugName)
		{
			AppendInstruction(new Instruction() { OpCode = OpCode.Call, NumVal = argCount, Name = debugName });
		}

		public void Emit_ThisCall(int argCount, string debugName)
		{
			AppendInstruction(new Instruction() { OpCode = OpCode.ThisCall, NumVal = argCount, Name = debugName });
		}

		public Instruction Emit_Literal(DynValue value)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Literal, Value = value });
		}

		public Instruction Emit_Jump(OpCode jumpOpCode, int idx, int optPar = 0)
		{
			return AppendInstruction(new Instruction() { OpCode = jumpOpCode, NumVal = idx, NumVal2 = optPar });
		}

		public Instruction Emit_MkTuple(int cnt)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.MkTuple, NumVal = cnt });
		}

		public Instruction Emit_Operator(OpCode opcode)
		{
			var i = AppendInstruction(new Instruction() { OpCode = opcode });

			if (opcode == OpCode.LessEq)
				AppendInstruction(new Instruction() { OpCode = OpCode.CNot });

			if (opcode == OpCode.Eq || opcode == OpCode.Less)
				AppendInstruction(new Instruction() { OpCode = OpCode.ToBool });

			return i;
		}


		//[Conditional("EMIT_DEBUG_OPS")]
		public void Emit_Debug(string str)
		{
			AppendInstruction(new Instruction() { OpCode = OpCode.Debug, Name = str.Substring(0, Math.Min(32, str.Length)) });
		}

		public Instruction Emit_Enter(RuntimeScopeBlock runtimeScopeBlock)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Enter, NumVal = runtimeScopeBlock.From, NumVal2 = runtimeScopeBlock.ToInclusive });
		}

		public Instruction Emit_Leave(RuntimeScopeBlock runtimeScopeBlock)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Leave, NumVal = runtimeScopeBlock.From, NumVal2 = runtimeScopeBlock.To });
		}

		public Instruction Emit_Exit(RuntimeScopeBlock runtimeScopeBlock)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Exit, NumVal = runtimeScopeBlock.From, NumVal2 = runtimeScopeBlock.ToInclusive });
		}

		public Instruction Emit_Closure(SymbolRef[] symbols, int jmpnum)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Closure, SymbolList = symbols, NumVal = jmpnum });
		}

		public Instruction Emit_Args(SymbolRef[] symbols)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Args, SymbolList = symbols });
		}

		public Instruction Emit_Ret(int retvals)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Ret, NumVal = retvals });
		}

		public Instruction Emit_ToNum(int stage = 0)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.ToNum, NumVal = stage });
		}

		public Instruction Emit_Incr(int i)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Incr, NumVal = i });
		}

		public Instruction Emit_NewTable()
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.NewTable });
		}

		public Instruction Emit_IterPrep()
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.IterPrep });
		}

		public Instruction Emit_ExpTuple(int stackOffset)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.ExpTuple, NumVal = stackOffset });
		}

		public Instruction Emit_IterUpd()
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.IterUpd });
		}


		public Instruction Emit_BeginFn(RuntimeScopeFrame m_StackFrame, string funcName)
		{
			return AppendInstruction(new Instruction()
			{
				OpCode = OpCode.BeginFn,
				SymbolList = m_StackFrame.DebugSymbols.ToArray(),
				NumVal = m_StackFrame.Count,
				NumVal2 = m_StackFrame.ToFirstBlock,
				Name = funcName
			});
		}

		public Instruction Emit_Scalar()
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Scalar });
		}

		public int Emit_Load(SymbolRef sym)
		{
			switch (sym.Type)
			{
				case SymbolRefType.Global:
					Emit_Load(sym.i_Env);
					AppendInstruction(new Instruction() { OpCode = OpCode.Index, Value = DynValue.NewString(sym.i_Name) });
					return 2;
				case SymbolRefType.Local:
					AppendInstruction(new Instruction() { OpCode = OpCode.Local, Symbol = sym });
					return 1;
				case SymbolRefType.Upvalue:
					AppendInstruction(new Instruction() { OpCode = OpCode.Upvalue, Symbol = sym });
					return 1;
				default:
					throw new InternalErrorException("Unexpected symbol type : {0}", sym);
			}
		}

		public int Emit_Store(SymbolRef sym, int stackofs, int tupleidx)
		{
			switch (sym.Type)
			{
				case SymbolRefType.Global:
					Emit_Load(sym.i_Env);
					AppendInstruction(new Instruction() { OpCode = OpCode.IndexSet, Symbol = sym, NumVal = stackofs, NumVal2 = tupleidx, Value = DynValue.NewString(sym.i_Name) });
					return 2;
				case SymbolRefType.Local:
					AppendInstruction(new Instruction() { OpCode = OpCode.StoreLcl, Symbol = sym, NumVal = stackofs, NumVal2 = tupleidx });
					return 1;
				case SymbolRefType.Upvalue:
					AppendInstruction(new Instruction() { OpCode = OpCode.StoreUpv, Symbol = sym, NumVal = stackofs, NumVal2 = tupleidx });
					return 1;
				default:
					throw new InternalErrorException("Unexpected symbol type : {0}", sym);
			}
		}

		public Instruction Emit_TblInitN()
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.TblInitN });
		}

		public Instruction Emit_TblInitI(bool lastpos)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.TblInitI, NumVal = lastpos ? 1 : 0 });
		}

		public Instruction Emit_Index(DynValue index = null)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Index, Value = index });
		}

		public Instruction Emit_IndexSet(int stackofs, int tupleidx, DynValue index = null)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.IndexSet, NumVal = stackofs, NumVal2 = tupleidx, Value = index });
		}

		public Instruction Emit_Copy(int numval)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Copy, NumVal = numval });
		}

		public Instruction Emit_Swap(int p1, int p2)
		{
			return AppendInstruction(new Instruction() { OpCode = OpCode.Swap, NumVal = p1, NumVal2 = p2 });
		}

	}
}
