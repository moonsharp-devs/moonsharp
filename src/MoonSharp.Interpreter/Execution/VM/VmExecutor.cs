using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed class VmExecutor
	{
		public class CallStackItem
		{
			public int IP;
			public int SP;
		}

		Chunk m_RootChunk;
		Chunk m_CurChunk;
		int m_InstructionPtr;

		FastStack<RValue> m_ValueStack = new FastStack<RValue>(131072);
		FastStack<CallStackItem> m_ExecutionStack = new FastStack<CallStackItem>(131072);
		bool m_Terminate = false;

		RuntimeScope m_Scope;

		RValue[] m_TempRegs = new RValue[8];


		public VmExecutor(Chunk rootChunk, RuntimeScope runtimeScope)
		{
			m_RootChunk = m_CurChunk = rootChunk;
			m_InstructionPtr = 0;
			m_Scope = runtimeScope;
		}

		public void Reset()
		{
			m_CurChunk = m_RootChunk ;
			m_InstructionPtr = 0;
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

			for (int i = m_ValueStack.Count - 1; i >= 0 && cnt > 0; i--)
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
			while (m_InstructionPtr < m_CurChunk.Code.Count && !m_Terminate)
			{
				Instruction i = m_CurChunk.Code[m_InstructionPtr];


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

				++m_InstructionPtr;

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
					case OpCode.Add:
						ExecAdd(i);
						break;
					case OpCode.Neg:
						ExecNeg(i);
						break;
					case OpCode.Sub:
						ExecSub(i);
						break;
					case OpCode.Mul:
						ExecMul(i);
						break;
					case OpCode.Div:
						ExecDiv(i);
						break;
					case OpCode.Power:
						ExecPower(i);
						break;
					case OpCode.Eq:
						ExecEq(i);
						break;
					case OpCode.LessEq:
						ExecLessEq(i);
						break;
					case OpCode.Less:
						ExecLess(i);
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
					case OpCode.JNil:
						ExecJNil(i);
						break;
					case OpCode.Store:
						ExecStore(i);
						break;
					case OpCode.Symbol:
						m_ValueStack.Push(new RValue(i.Symbol));
						break;
					case OpCode.Assign:
						ExecAssign(i);
						break;
					case OpCode.Jump:
						m_InstructionPtr = i.NumVal;
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
					case OpCode.SymStorN:
						ExecSymStorN(i);
						break;
					case OpCode.JFor:
						ExecJFor(i);
						break;
					case OpCode.Index:
						ExecIndexGet(i);
						break;
					case OpCode.IndexRef:
						ExecIndexRef(i, false);
						break;
					case OpCode.IndexRefN:
						ExecIndexRef(i, true);
						break;
					case OpCode.NewTable:
						m_ValueStack.Push(new RValue(new Table()));
						break;
					case OpCode.TmpClear:
						m_TempRegs[i.NumVal] = null;
						break;
					case OpCode.TmpPeek:
						m_TempRegs[i.NumVal] = m_ValueStack.Peek();
						break;
					case OpCode.TmpPop:
						m_TempRegs[i.NumVal] = m_ValueStack.Pop();
						break;
					case OpCode.Reverse:
						ExecReverse(i);
						break;
					case OpCode.Len:
						ExecLen(i);
						break;
					case OpCode.TmpPush:
						m_ValueStack.Push(m_TempRegs[i.NumVal]);
						break;
					case OpCode.IterPrep:
						ExecIterPrep(i);
						break;
					case OpCode.IterUpd:
						ExecIterUpd(i);
						break;
					case OpCode.ExpTuple:
						ExecExpTuple(i);
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

		private void ExecJNil(Instruction i)
		{
			RValue v = m_ValueStack.Pop();

			if (v.Type == DataType.Nil)
				m_InstructionPtr = i.NumVal;
		}

		private void ExecIterUpd(Instruction i)
		{
			RValue v = m_ValueStack.Peek(0);
			RValue t = m_ValueStack.Peek(1);
			t.Tuple[2] = v;
		}

		private void ExecExpTuple(Instruction i)
		{
			RValue t = m_ValueStack.Peek(i.NumVal);

			if (t.Type == DataType.Tuple)
			{
				for (int idx = 0; idx < t.Tuple.Length; idx++)
					m_ValueStack.Push(t.Tuple[idx]);
			}
			else
			{
				m_ValueStack.Push(t);
			}
			
		}

		private void ExecIterPrep(Instruction i)
		{
			RValue v = m_ValueStack.Pop();

			if (v.Type != DataType.Tuple)
			{
				v = new RValue(new RValue[] { v, RValue.Nil, RValue.Nil });
			}
			else if (v.Tuple.Length > 3)
			{
				v = new RValue(new RValue[] { v.Tuple[0], v.Tuple[1], v.Tuple[2] });
			}
			else if (v.Tuple.Length == 2)
			{
				v = new RValue(new RValue[] { v.Tuple[0], v.Tuple[1], RValue.Nil });
			}
			else if (v.Tuple.Length == 1)
			{
				v = new RValue(new RValue[] { v.Tuple[0], RValue.Nil, RValue.Nil });
			}

			m_ValueStack.Push(v);
		}

		private void ExecReverse(Instruction i)
		{
			int cnt = i.NumVal;
			int cnth = cnt / 2;

			int len = m_ValueStack.Count - 1;

			for (int idx = 0; idx < cnth; idx++)
			{
				var tmp = m_ValueStack[len - idx];
				m_ValueStack[len - idx] = m_ValueStack[len - (cnt - 1 - idx)];
				m_ValueStack[len - (cnt - 1 - idx)] = tmp;
			}
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
				Console.Write("{0:X8}  {1}", m_InstructionPtr, i);

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
				m_InstructionPtr = i.NumVal;
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
			{
				m_Terminate = true;
				return;
			}

			if (i.NumVal == 0)
			{
				int retpoint = PopToBasePointer();
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(RValue.Nil);
				m_InstructionPtr = retpoint;
			}
			else if (i.NumVal == 1)
			{
				var retval = m_ValueStack.Pop();
				int retpoint = PopToBasePointer();
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(retval);
				m_InstructionPtr = retpoint;
			}
			else
			{
				throw new InternalErrorException("RET supports only 0 and 1 ret val scenarios");
			}
		}

		private int PopToBasePointer()
		{
			var xs = m_ExecutionStack.Pop();
			m_ValueStack.CropAtCount(xs.SP);
			return xs.IP;
		}

		private int PopExecStackAndCheckVStack(int vstackguard)
		{
			var xs = m_ExecutionStack.Pop();
			if (vstackguard != xs.SP)
				throw new InternalErrorException("StackGuard violation");

			return xs.IP;
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
				var ret = fn.Callback.Invoke(args);
				m_ValueStack.Push(ret);
			}
			else if (fn.Type == DataType.Function)
			{
				m_ValueStack.Push(new RValue(i.NumVal));
				m_ExecutionStack.Push(new CallStackItem()
				{
					SP = m_ValueStack.Count,
					IP = m_InstructionPtr,
				});
				m_InstructionPtr = fn.Function.ByteCodeLocation;
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
				m_InstructionPtr = i.NumVal;
		}

		private void ExecShortCircuitingOperator(Instruction i)
		{
			bool expectedValToShortCircuit = i.OpCode == OpCode.JtOrPop;

			RValue op = m_ValueStack.Peek();

			if (op.TestAsBoolean() == expectedValToShortCircuit)
				m_InstructionPtr = i.NumVal;
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


		private void ExecLen(Instruction i)
		{
			RValue r = m_ValueStack.Pop();

			if (r.Type == DataType.Table)
				m_ValueStack.Push(new RValue(r.Table.Length));
			else if (r.Type == DataType.String)
				m_ValueStack.Push(new RValue(r.String.Length));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void ExecAdd(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number + r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void ExecSub(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number - r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void ExecNeg(Instruction i)
		{
			RValue r = m_ValueStack.Pop();

			if (r.Type == DataType.Number)
				m_ValueStack.Push(new RValue(-r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}
		private void ExecPower(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(Math.Pow(l.Number, r.Number)));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void ExecMul(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number * r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}

		private void ExecEq(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			m_ValueStack.Push(new RValue(r.Equals(l)));
		}

		private void ExecLess(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && r.Type == DataType.Number)
			{
				m_ValueStack.Push(new RValue(l.Number < r.Number));
			}
			else
			{
				throw new NotImplementedException("Comparison between non numbers!");
			}
		}

		private void ExecLessEq(Instruction i)
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

		private void ExecDiv(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if (r.Type == DataType.Number && l.Type == DataType.Number)
				m_ValueStack.Push(new RValue(l.Number / r.Number));
			else
				throw new NotImplementedException("Meta operators");
		}


		private void Internal_Assign(LRef l, RValue r)
		{
			if (l.i_Type == LRefType.Index)
			{
				l.i_TableRefObject.Table[l.i_TableRefIndex] = r;
			}
			else
			{
				m_Scope.Assign(l, r.ToSimplestValue());
			}
		}

		private void Internal_Assign(RValue l, RValue r)
		{
			if (l.Type == DataType.Symbol)
			{
				Internal_Assign(l.Symbol, r);
			}
			else
			{
				throw new NotImplementedException("How should we manage this ?");
			}
		}

		private void ExecIndexGet(Instruction i)
		{
			RValue indexValue = m_ValueStack.Pop();
			RValue baseValue = m_ValueStack.Pop();

			if (baseValue.Type != DataType.Table)
			{
				throw new NotImplementedException("META! : Can't index non-table yet");
			}
			else
			{
				RValue v = baseValue.Table[indexValue];
				m_ValueStack.Push(v.AsReadOnly());
			}
		}

		private void ExecIndexRef(Instruction i, bool keepOnStack)
		{
			RValue indexValue = m_ValueStack.Pop();
			RValue baseValue = keepOnStack ? m_ValueStack.Peek() : m_ValueStack.Pop();

			if (baseValue.Type != DataType.Table)
			{
				throw new NotImplementedException("META! : Can't index non-table yet");
			}
			else
			{
				LRef s = LRef.ObjIndex(baseValue, indexValue);
				m_ValueStack.Push(new RValue(s));
			}
		}

		private void ExecStore(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			Internal_Assign(l, r);
		}


		private void ExecSymStorN(Instruction i)
		{
			m_Scope.Assign(i.Symbol, m_ValueStack.Peek());
		}


		private void ExecAssign(Instruction i)
		{
			Slice<RValue> rvalues = new Slice<RValue>(m_ValueStack, m_ValueStack.Count - i.NumVal2, i.NumVal2, false);
			Slice<RValue> lvalues = new Slice<RValue>(m_ValueStack, m_ValueStack.Count - i.NumVal2 - i.NumVal, i.NumVal, false);

			Internal_MultiAssign(lvalues, rvalues);

			m_ValueStack.CropAtCount(m_ValueStack.Count - i.NumVal - i.NumVal2);
		}

		private void Internal_MultiAssign(Slice<RValue> lValues, Slice<RValue> rValues)
		{
			int li = 0;
			int rValues_Count = rValues.Count;
			int lValues_Count = lValues.Count;

			for (int ri = 0; ri < rValues_Count && li < lValues_Count; ri++, li++)
			{
				RValue vv = rValues[ri];

				if ((ri != rValues_Count - 1) || (vv.Type != DataType.Tuple))
				{
					Internal_Assign(lValues[li], vv.ToSingleValue());
					// Debug.WriteLine(string.Format("{0} <- {1}", li, vv.ToSingleValue()));
				}
				else
				{
					for (int rri = 0, len = vv.Tuple.Length; rri < len && li < lValues_Count; rri++, li++)
					{
						Internal_Assign(lValues[li], vv.Tuple[rri].ToSingleValue());
						// Debug.WriteLine(string.Format("{0} <- {1}", li, vv.Tuple[rri].ToSingleValue()));
					}
				}
			}
		}





	}
}
