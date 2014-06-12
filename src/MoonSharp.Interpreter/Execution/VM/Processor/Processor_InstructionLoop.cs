using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor
	{
		private RValue Processing_Loop()
		{
			while (m_InstructionPtr < m_CurChunk.Code.Count && m_InstructionPtr >= 0)
			{
				Instruction i = m_CurChunk.Code[m_InstructionPtr];

				if (m_DebuggerAttached != null)
				{
					ListenDebugger(i);
				}

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
						m_ValueStack.Push(this.GetGenericSymbol(i.Symbol));
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
					case OpCode.Concat:
						ExecConcat(i);
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
						NilifyBlockData(i.Block);
						break;
					case OpCode.Leave:
					case OpCode.Exit:
						ClearBlockData(i.Block, i.OpCode == OpCode.Exit);
						break;
					case OpCode.Closure:
						m_ValueStack.Push(new RValue(new Closure(i.NumVal, i.SymbolList, m_ExecutionStack.Peek().LocalScope)));
						break;
					case OpCode.ExitClsr:
						this.LeaveClosure();
						break;
					case OpCode.BeginFn:
						ExecBeginFn(i);
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
						ExecIndexGet(i, false);
						break;
					case OpCode.Method:
						ExecIndexGet(i, true);
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
					case OpCode.Len:
						ExecLen(i);
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

		private void ExecBeginFn(Instruction i)
		{
			CallStackItem c = m_ExecutionStack.Peek();
			c.Debug_Symbols = i.SymbolList;
			c.LocalScope = new RValue[i.NumVal];

			if (i.NumVal2 >= 0 && i.NumVal > 0)
			{
				for (int idx = 0; idx < i.NumVal2; idx++)
				{
					c.LocalScope[idx] = new RValue();
				}
			}
		}

		private int PopToBasePointer()
		{
			var xs = m_ExecutionStack.Pop();
			m_ValueStack.CropAtCount(xs.BasePointer);
			return xs.ReturnAddress;
		}

		private int PopExecStackAndCheckVStack(int vstackguard)
		{
			var xs = m_ExecutionStack.Pop();
			if (vstackguard != xs.BasePointer)
				throw new InternalErrorException("StackGuard violation");

			return xs.ReturnAddress;
		}

		private void ExecArgs(Instruction I)
		{
			int numargs = (int)m_ValueStack.Peek(0).Number;

			for (int i = 0; i < I.SymbolList.Length; i++)
			{
				if (i >= numargs)
				{
					this.AssignGenericSymbol(I.SymbolList[i], new RValue());
				}
				else
				{
					this.AssignGenericSymbol(I.SymbolList[i], m_ValueStack.Peek(numargs - i).CloneAsWritable());
				}
			}
		}

		private void ExecCall(Instruction i)
		{
			RValue fn = m_ValueStack.Peek(i.NumVal);

			if (fn.Type == DataType.ClrFunction)
			{
				IList<RValue> args = new Slice<RValue>(m_ValueStack, m_ValueStack.Count - i.NumVal, i.NumVal, false);
				//m_ValueStack.Pop();
				var ret = fn.Callback.Invoke(args);
				m_ValueStack.RemoveLast(i.NumVal + 1);
				m_ValueStack.Push(ret);
			}
			else if (fn.Type == DataType.Function)
			{
				m_ValueStack.Push(new RValue(i.NumVal));
				m_ExecutionStack.Push(new CallStackItem()
				{
					BasePointer = m_ValueStack.Count,
					ReturnAddress = m_InstructionPtr,
					Debug_EntryPoint = fn.Function.ByteCodeLocation
				});
				m_InstructionPtr = fn.Function.ByteCodeLocation;
				this.EnterClosure(fn.Function.ClosureContext);
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

		private void ExecConcat(Instruction i)
		{
			RValue r = m_ValueStack.Pop();
			RValue l = m_ValueStack.Pop();

			if ((r.Type == DataType.String || r.Type == DataType.Number) && (l.Type == DataType.String || l.Type == DataType.Number))
				m_ValueStack.Push(new RValue(l.AsString() + r.AsString()));
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
				this.AssignGenericSymbol(l, r.ToSimplestValue());
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

		private void ExecIndexGet(Instruction i, bool methodCall)
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

			if (methodCall)
				m_ValueStack.Push(baseValue);
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
			this.AssignGenericSymbol(i.Symbol, m_ValueStack.Peek());
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
				}
				else
				{
					for (int rri = 0, len = vv.Tuple.Length; rri < len && li < lValues_Count; rri++, li++)
					{
						Internal_Assign(lValues[li], vv.Tuple[rri].ToSingleValue());
					}
				}
			}
		}
	}
}
