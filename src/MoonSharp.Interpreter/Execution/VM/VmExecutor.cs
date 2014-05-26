using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed class VmExecutor
	{
		Chunk m_RootChunk;
		Chunk m_CurChunk;
		int m_ProgramCounter;

		List<RValue> m_ValueStack = new List<RValue>();
		List<int> m_ExecutionStack = new List<int>();

		RuntimeScope m_Scope;

		public VmExecutor(Chunk rootChunk, RuntimeScope scope)
		{
			m_RootChunk = m_CurChunk = rootChunk;
			m_ProgramCounter = 0;
			m_Scope = scope;
		}

		private RValue[] StackTopToArray(int items, bool pop)
		{
			RValue[] values = new RValue[items];

			if (pop)
			{
				for (int i = 0; i < items; i++)
				{
					values[i] = m_ValueStack.Pop();
				}
			}
			else
			{
				for (int i = 0; i < items; i++)
				{
					values[i] = m_ValueStack[m_ValueStack.Count - 1 - i];
				}
			}

			return values;
		}

		private RValue[] StackTopToArrayReverse(int items, bool pop)
		{
			RValue[] values = new RValue[items];

			if (pop)
			{
				for (int i = 0; i < items; i++)
				{
					values[items - 1 - i] = m_ValueStack.Pop();
				}
			}
			else
			{
				for (int i = 0; i < items; i++)
				{
					values[items - 1 - i] = m_ValueStack[m_ValueStack.Count - 1 - i];
				}
			}

			return values;
		}

		private string DumpValueStack()
		{
			int cnt = 6;
			List<RValue> values = new List<RValue>();

			for(int i = m_ValueStack.Count - 1; i >= 0 && cnt > 0; i--)
			{
				values.Add(m_ValueStack[i]);
				cnt--;
			}

			return string.Join(", ", values.Select(s => s.ToString()).ToArray());
		}

		bool m_DoDebug = true;
		bool m_StepEnabled = true;
		public RValue Execute()
		{
			while (m_ProgramCounter < m_CurChunk.Code.Count)
			{
				Instruction i = m_CurChunk.Code[m_ProgramCounter];


				//if (m_DoDebug)
				//{
				//	DebugInterface(i);
				//}

				//if (System.Diagnostics.Debugger.IsAttached && m_StepEnabled)
				//{
				//	ConsoleKeyInfo cki = Console.ReadKey();
				//	if (cki.Key == ConsoleKey.Escape)
				//		m_StepEnabled = false;
				//}

				++m_ProgramCounter;

				switch (i.OpCode)
				{
					case OpCode.Nop:
					case OpCode.Debug:
						break;
					case OpCode.Pop:
						m_ValueStack.RemoveLast(i.NumVal);
						break;
					case OpCode.Load:
						m_ValueStack.Push(m_Scope.Get(i.Symbol));
						break;
					case OpCode.Literal:
						m_ValueStack.Push(i.Value);
						break;
					case OpCode.Bool:
						Bool(i);
						break;
					case OpCode.Reduce:
						Reduce(i);
						break;
					case OpCode.Add:
						Add(i);
						break;
					case OpCode.Neg:
						Neg(i);
						break;
					case OpCode.Sub:
						Sub(i);
						break;
					case OpCode.Mul:
						Mul(i);
						break;
					case OpCode.Div:
						Div(i);
						break;
					case OpCode.Power:
						Power(i);
						break;
					case OpCode.Eq:
						Eq(i);
						break;
					case OpCode.LessEq:
						LessEq(i);
						break;
					case OpCode.Call:
						ExecCall(i);
						break;
					case OpCode.Jf:
						JumpBool(i, false);
						break;
					case OpCode.Not:
						ExecNot(i);
						break;
					case OpCode.JfOrPop:
					case OpCode.JtOrPop:
						ExecShortCircuitingOperator(i);
						break;
					case OpCode.Store:
						ExecStore(i);
						break;
					case OpCode.Symbol:
						m_ValueStack.Push(new RValue(i.Symbol));
						break;
					case OpCode.Jump:
						m_ProgramCounter = i.NumVal;
						break;
					case OpCode.MkTuple:
						m_ValueStack.Push(RValue.FromPotentiallyNestedTuple(StackTopToArrayReverse(i.NumVal, true)));
						break;
					case OpCode.Enter:
						m_Scope.PushFrame(i.Frame);
						break;
					case OpCode.Leave:
						m_Scope.PopFrame();
						break;
					case OpCode.Exit:
						ExecExit(i);
						break;
					case OpCode.Closure:
						m_ValueStack.Push(new RValue(new Closure(i.NumVal, i.SymbolList, m_Scope)));
						break;
					case OpCode.ExitClsr:
						m_Scope.LeaveClosure();
						break;
					case OpCode.Args:
						ExecArgs(i);
						break;
					case OpCode.Ret:
						ExecRet(i);
						break;
					case OpCode.Incr:
						ExecIncr(i);
						break;
					case OpCode.ToNum:
						m_ValueStack.Push(m_ValueStack.Pop().AsNumber());
						break;
					case OpCode.NSymStor:
						ExecNSymStor(i);
						break;
					case OpCode.JFor:
						ExecJFor(i);
						break;
					case OpCode.Invalid:
						throw new NotImplementedException(string.Format("Compilation for {0} not implented yet!", i.Name));
					default:
						throw new NotImplementedException(string.Format("Execution for {0} not implented yet!", i.OpCode));
				}
			}

			if (m_ValueStack.Count == 1)
				return m_ValueStack.Pop();
			else if (m_ValueStack.Count == 0)
				return RValue.Nil;
			else
				throw new InternalErrorException("Unexpected value stack count at program end : {0}", m_ValueStack.Count);

		}


		private void ExecExit(Instruction i)
		{
			if (i.Frame == null)
			{
				m_Scope.PopFramesToFunction();
				if (m_ExecutionStack.Count > 0)
					m_Scope.LeaveClosure();
			}
			else
			{
				m_Scope.PopFramesToFrame(i.Frame);
			}
		}

		private void DebugInterface(Instruction i)
		{
			if (i.OpCode == OpCode.Debug)
				Console.Write("    {0}", i);
			else
				Console.Write("{0:X8}  {1}", m_ProgramCounter, i);

			Console.SetCursorPosition(40, Console.CursorTop);
			Console.WriteLine("|| VS={1:X4}  XS={2:X4} || {0}", DumpValueStack(), m_ValueStack.Count, m_ExecutionStack.Count);

			//Console.ReadKey();
		}


		private void ExecJFor(Instruction i)
		{
			double val = m_ValueStack.Peek(0).Number;
			double step = m_ValueStack.Peek(1).Number;
			double stop = m_ValueStack.Peek(2).Number;

			bool whileCond = (step > 0) ? val <= stop : val >= stop;

			if (!whileCond)
				m_ProgramCounter = i.NumVal;
		}

		private void ExecNSymStor(Instruction i)
		{
			m_Scope.Assign(i.Symbol, m_ValueStack.Peek());
		}

		private void ExecIncr(Instruction i)
		{
			RValue top = m_ValueStack.Peek(0);
			RValue btm = m_ValueStack.Peek(i.NumVal);

			if (top.ReadOnly)
			{
				m_ValueStack.Pop();
				top = top.CloneAsWritable();
				m_ValueStack.Push(top);
			}

			top.Assign(top.Number + btm.Number);
		}



		private void ExecNot(Instruction i)
		{
			RValue v = m_ValueStack.Pop();
			m_ValueStack.Push(new RValue(!(v.TestAsBoolean())));
		}

		
		private void ExecRet(Instruction i)
		{
			if (m_ExecutionStack.Count == 0)
				return;

			if (i.NumVal == 0)
			{
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ProgramCounter = m_ExecutionStack.Pop();
				m_ValueStack.Push(RValue.Nil);
			}
			else if (i.NumVal == 1)
			{
				var retval = m_ValueStack.Pop();
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(retval);
				m_ProgramCounter = m_ExecutionStack.Pop();
			}
			else
			{
				throw new InternalErrorException("RET supports only 0 and 1 ret val scenarios");
			}
		}

		private void ExecArgs(Instruction I)
		{
			for (int i = 0; i < I.SymbolList.Length; i++)
			{
				m_Scope.Assign(I.SymbolList[i], m_ValueStack.Peek(i + 1));
			}
		}

		private void ExecCall(Instruction i)
		{
			RValue fn = m_ValueStack.Peek(i.NumVal);

			if (fn.Type == DataType.ClrFunction)
			{
				RValue[] args = StackTopToArray(i.NumVal, true);
				m_ValueStack.Pop();
				var ret = fn.Callback.Invoke(m_Scope, args);
				m_ValueStack.Push(ret);
			}
			else if (fn.Type == DataType.Function)
			{
				m_ValueStack.Push(new RValue(i.NumVal));
				m_ExecutionStack.Push(m_ProgramCounter);
				m_ProgramCounter = fn.Function.ByteCodeLocation;
				fn.Function.EnterClosureBeforeCall(m_Scope);
			}
			else
			{
				throw new NotImplementedException("Meta");
			}
		}




		private void JumpBool(Instruction i, bool expectedValueForJump)
		{
			RValue op = m_ValueStack.Pop();

			if (op.TestAsBoolean() == expectedValueForJump)
				m_ProgramCounter = i.NumVal;
		}

		private void ExecShortCircuitingOperator(Instruction i)
		{
			bool expectedValToShortCircuit = i.OpCode == OpCode.JfOrPop;

			RValue op = m_ValueStack.Peek();

			if (op.TestAsBoolean() == expectedValToShortCircuit)
				m_ProgramCounter = i.NumVal;
			else
				m_ValueStack.Pop();
		}

		private void Bool(Instruction i)
		{
			RValue v = m_ValueStack.Peek();
			if (v.Type != DataType.Boolean)
			{
				m_ValueStack.Pop();
				m_ValueStack.Push(v.ToSimplestValue().AsBoolean());
			}
		}

		private void Reduce(Instruction i)
		{
			RValue v = m_ValueStack.Peek();
			if (v.Type == DataType.Tuple)
			{
				m_ValueStack.Pop();
				m_ValueStack.Push(v.ToSimplestValue());
			}
		}

		private void Add(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number + r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void Sub(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number - r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void Neg(Instruction i)
		{
			RValue r = m_ValueStack.Pop();

			if (r.Type == DataType.Number)
				m_ValueStack.Push(new RValue( -r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}
		private void Power(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(Math.Pow(l.Number, r.Number)));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void Mul(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number * r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void Eq(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			m_ValueStack.Push(new RValue(r.Equals(l)));
		}


		private void LessEq(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && r.Type == DataType.Number)
			{
				m_ValueStack.Push(new RValue(l.Number <= r.Number));
			}
			else
			{
				throw new NotImplementedException("Comparison between non numbers!");
			}
		}

		private void Div(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number / r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void ExecStore(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (l.Type == DataType.Symbol)
			{
				m_Scope.Assign(l.Symbol, r.ToSimplestValue());
			}
			else
			{
				throw new InternalErrorException("Assignment on type {0}", l.Type);
			}
		}





	}
}
