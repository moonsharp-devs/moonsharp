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
		private DynValue Processing_Loop(int instructionPtr)
		{
			while (true)
			{
				Instruction i = m_RootChunk.Code[instructionPtr];

				if (m_DebuggerAttached != null)
				{
					ListenDebugger(i, instructionPtr);
				}

				++instructionPtr;

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
					case OpCode.Add:
						instructionPtr = ExecAdd(i, instructionPtr);
						break;
					case OpCode.Concat:
						instructionPtr = ExecConcat(i, instructionPtr);
						break;
					case OpCode.Neg:
						instructionPtr = ExecNeg(i, instructionPtr);
						break;
					case OpCode.Sub:
						instructionPtr = ExecSub(i, instructionPtr);
						break;
					case OpCode.Mul:
						instructionPtr = ExecMul(i, instructionPtr);
						break;
					case OpCode.Div:
						instructionPtr = ExecDiv(i, instructionPtr);
						break;
					case OpCode.Mod:
						instructionPtr = ExecMod(i, instructionPtr);
						break;
					case OpCode.Power:
						instructionPtr = ExecPower(i, instructionPtr);
						break;
					case OpCode.Eq:
						instructionPtr = ExecEq(i, instructionPtr);
						break;
					case OpCode.LessEq:
						instructionPtr = ExecLessEq(i, instructionPtr);
						break;
					case OpCode.Less:
						instructionPtr = ExecLess(i, instructionPtr);
						break;
					case OpCode.Len:
						instructionPtr = ExecLen(i, instructionPtr);
						break;
					case OpCode.Call:
						instructionPtr = Internal_ExecCall(i.NumVal, instructionPtr);
						break;
					case OpCode.TailChk:
						instructionPtr = ExecTailChk(i, instructionPtr);
						break;
					case OpCode.Scalar:
						m_ValueStack.Push(m_ValueStack.Pop().ToScalar());
						break;
					case OpCode.Not:
						ExecNot(i);
						break;
					case OpCode.JfOrPop:
					case OpCode.JtOrPop:
						instructionPtr = ExecShortCircuitingOperator(i, instructionPtr);
						break;
					case OpCode.JNil:
						{
							DynValue v = m_ValueStack.Pop();

							if (v.Type == DataType.Nil)
								instructionPtr = i.NumVal;
						}
						break;
					case OpCode.Jf:
						instructionPtr = JumpBool(i, false, instructionPtr);
						break;
					case OpCode.Store:
						ExecStore(i);
						break;
					case OpCode.Symbol:
						m_ValueStack.Push(DynValue.NewReference(i.Symbol));
						break;
					case OpCode.Assign:
						ExecAssign(i);
						break;
					case OpCode.Jump:
						instructionPtr = i.NumVal;
						break;
					case OpCode.MkTuple:
						ExecMkTuple(i);
						break;
					case OpCode.Enter:
						NilifyBlockData(i.Block);
						break;
					case OpCode.Leave:
					case OpCode.Exit:
						ClearBlockData(i.Block, i.OpCode == OpCode.Exit);
						break;
					case OpCode.Closure:
						m_ValueStack.Push(DynValue.NewClosure(new Closure(i.NumVal, i.SymbolList, m_ExecutionStack.Peek().LocalScope)));
						break;
					case OpCode.BeginFn:
						ExecBeginFn(i);
						break;
					case OpCode.ToBool:
						m_ValueStack.Push(DynValue.NewBoolean(m_ValueStack.Pop().CastToBool()));
						break;
					case OpCode.Args:
						ExecArgs(i);
						break;
					case OpCode.Ret:
						instructionPtr = ExecRet(i);
						if (instructionPtr < 0)
							goto return_to_native_code;
						break;
					case OpCode.Incr:
						ExecIncr(i);
						break;
					case OpCode.ToNum:
						ExecToNum(i);
						break;
					case OpCode.SymStorN:
						ExecSymStorN(i);
						break;
					case OpCode.JFor:
						instructionPtr = ExecJFor(i, instructionPtr);
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
						m_ValueStack.Push(DynValue.NewTable(new Table()));
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
						throw new NotImplementedException(string.Format("Invalid opcode : {0}", i.Name));
					default:
						throw new NotImplementedException(string.Format("Execution for {0} not implented yet!", i.OpCode));
				}
			}

		return_to_native_code:

			if (m_ValueStack.Count == 1)
				return m_ValueStack.Pop();
			else if (m_ValueStack.Count == 0)
				return DynValue.Nil;
			else
				throw new InternalErrorException("Unexpected value stack count at program end : {0}", m_ValueStack.Count);

		}

		private void ExecMkTuple(Instruction i)
		{
			Slice<DynValue> slice = new Slice<DynValue>(m_ValueStack, m_ValueStack.Count - i.NumVal, i.NumVal, false);

			var v = Internal_AdjustTuple(slice);

			m_ValueStack.Push(DynValue.NewTuple(v));
		}

		private void ExecToNum(Instruction i)
		{
			double? v = m_ValueStack.Pop().CastToNumber();
			if (v.HasValue)
				m_ValueStack.Push(DynValue.NewNumber(v.Value));
			else
				throw new ScriptRuntimeException(null, "Can't convert value to number");
		}


		private void ExecIterUpd(Instruction i)
		{
			DynValue v = m_ValueStack.Peek(0);
			DynValue t = m_ValueStack.Peek(1);
			t.Tuple[2] = v;
		}

		private void ExecExpTuple(Instruction i)
		{
			DynValue t = m_ValueStack.Peek(i.NumVal);

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
			DynValue v = m_ValueStack.Pop();

			if (v.Type != DataType.Tuple)
			{
				v = DynValue.NewTuple(v, DynValue.Nil, DynValue.Nil);
			}
			else if (v.Tuple.Length > 3)
			{
				v = DynValue.NewTuple(v.Tuple[0], v.Tuple[1], v.Tuple[2]);
			}
			else if (v.Tuple.Length == 2)
			{
				v = DynValue.NewTuple(v.Tuple[0], v.Tuple[1], DynValue.Nil);
			}
			else if (v.Tuple.Length == 1)
			{
				v = DynValue.NewTuple(v.Tuple[0], DynValue.Nil, DynValue.Nil);
			}

			m_ValueStack.Push(v);
		}


		private int ExecJFor(Instruction i, int instructionPtr)
		{
			double val = m_ValueStack.Peek(0).Number;
			double step = m_ValueStack.Peek(1).Number;
			double stop = m_ValueStack.Peek(2).Number;

			bool whileCond = (step > 0) ? val <= stop : val >= stop;

			if (!whileCond)
				return i.NumVal;
			else
				return instructionPtr;
		}



		private void ExecIncr(Instruction i)
		{
			DynValue top = m_ValueStack.Peek(0);
			DynValue btm = m_ValueStack.Peek(i.NumVal);

			if (top.ReadOnly)
			{
				m_ValueStack.Pop();
				top = top.CloneAsWritable();
				m_ValueStack.Push(top);
			}

			top.AssignNumber(top.Number + btm.Number);
		}



		private void ExecNot(Instruction i)
		{
			DynValue v = m_ValueStack.Pop();
			m_ValueStack.Push(DynValue.NewBoolean(!(v.CastToBool())));
		}


		private int ExecRet(Instruction i)
		{
			if (i.NumVal == 0)
			{
				int retpoint = PopToBasePointer();
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(DynValue.Nil);
				return retpoint;
			}
			else if (i.NumVal == 1)
			{
				var retval = m_ValueStack.Pop();
				int retpoint = PopToBasePointer();
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(retval);
				return retpoint;
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
			c.LocalScope = new DynValue[i.NumVal];

			if (i.NumVal2 >= 0 && i.NumVal > 0)
			{
				for (int idx = 0; idx < i.NumVal2; idx++)
				{
					c.LocalScope[idx] = DynValue.NewNil();
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
					this.AssignGenericSymbol(I.SymbolList[i], DynValue.NewNil());
				}
				else
				{
					this.AssignGenericSymbol(I.SymbolList[i], m_ValueStack.Peek(numargs - i).CloneAsWritable());
				}
			}
		}


		private int Internal_ExecCall(int argsCount, int instructionPtr)
		{
			DynValue fn = m_ValueStack.Peek(argsCount);

			if (fn.Type == DataType.ClrFunction)
			{
				IList<DynValue> args = new Slice<DynValue>(m_ValueStack, m_ValueStack.Count - argsCount, argsCount, false);
				var ret = fn.Callback.Invoke(this, args);
				m_ValueStack.RemoveLast(argsCount + 1);
				m_ValueStack.Push(ret);
				return instructionPtr;
			}
			else if (fn.Type == DataType.Function)
			{
				m_ValueStack.Push(DynValue.NewNumber(argsCount));
				m_ExecutionStack.Push(new CallStackItem()
				{
					BasePointer = m_ValueStack.Count,
					ReturnAddress = instructionPtr,
					Debug_EntryPoint = fn.Function.ByteCodeLocation,
					ClosureScope = fn.Function.ClosureContext
				});
				return fn.Function.ByteCodeLocation;
			}
			else
			{
				if (fn.Meta != null)
				{
					var m = fn.Meta.Table.RawGet("__call");

					if (m != null && m.Type != DataType.Nil)
					{
						DynValue[] tmp = new DynValue[argsCount + 1];
						for (int i = 0; i < argsCount + 1; i++)
							tmp[i] = m_ValueStack.Pop();

						m_ValueStack.Push(m);

						for (int i = argsCount; i >= 0; i--)
							m_ValueStack.Push(tmp[i]);

						return Internal_ExecCall(argsCount + 1, instructionPtr);
					}
				}

				throw new NotImplementedException("Can't call non function");
			}
		}



		private int ExecTailChk(Instruction i, int instructionPtr)
		{
			DynValue tail = m_ValueStack.Peek(0);

			if (tail.Type == DataType.TailCallRequest)
			{
				m_ValueStack.Pop(); // discard tail call request

				m_ValueStack.Push(tail.Meta);

				for (int ii = 0; ii < tail.Tuple.Length; ii++ )
					m_ValueStack.Push(tail.Tuple[ii]);

				instructionPtr -= 1;
				return Internal_ExecCall(tail.Tuple.Length, instructionPtr);
			}


			return instructionPtr;
		}



		private int JumpBool(Instruction i, bool expectedValueForJump, int instructionPtr)
		{
			DynValue op = m_ValueStack.Pop();

			if (op.CastToBool() == expectedValueForJump)
				return i.NumVal;

			return instructionPtr;
		}

		private int ExecShortCircuitingOperator(Instruction i, int instructionPtr)
		{
			bool expectedValToShortCircuit = i.OpCode == OpCode.JtOrPop;

			DynValue op = m_ValueStack.Peek();

			if (op.CastToBool() == expectedValToShortCircuit)
			{
				return i.NumVal;
			}
			else
			{
				m_ValueStack.Pop();
				return instructionPtr;
			}
		}

		private void Internal_Assign(SymbolRef l, DynValue r)
		{
			if (l.i_Type == SymbolRefType.Index)
			{
				l.i_TableRefObject.Table[l.i_TableRefIndex] = r;
			}
			else
			{
				this.AssignGenericSymbol(l, r.ToScalar());
			}
		}

		private void Internal_Assign(DynValue l, DynValue r)
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
			DynValue indexValue = m_ValueStack.Pop();
			DynValue baseValue = m_ValueStack.Pop();

			if (baseValue.Type != DataType.Table)
			{
				throw new NotImplementedException("META! : Can't index non-table yet");
			}
			else
			{
				DynValue v = baseValue.Table[indexValue];
				m_ValueStack.Push(v.AsReadOnly());
			}

			if (methodCall)
				m_ValueStack.Push(baseValue);
		}

		private void ExecIndexRef(Instruction i, bool keepOnStack)
		{
			DynValue indexValue = m_ValueStack.Pop();
			DynValue baseValue = keepOnStack ? m_ValueStack.Peek() : m_ValueStack.Pop();

			if (baseValue.Type != DataType.Table)
			{
				throw new NotImplementedException("META! : Can't index non-table yet");
			}
			else
			{
				SymbolRef s = SymbolRef.ObjIndex(baseValue, indexValue);
				m_ValueStack.Push(DynValue.NewReference(s));
			}
		}

		private void ExecStore(Instruction i)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			Internal_Assign(l, r);
		}


		private void ExecSymStorN(Instruction i)
		{
			this.AssignGenericSymbol(i.Symbol, m_ValueStack.Peek());
		}


		private void ExecAssign(Instruction i)
		{
			Slice<DynValue> rvalues = new Slice<DynValue>(m_ValueStack, m_ValueStack.Count - i.NumVal2, i.NumVal2, false);
			Slice<DynValue> lvalues = new Slice<DynValue>(m_ValueStack, m_ValueStack.Count - i.NumVal2 - i.NumVal, i.NumVal, false);

			Internal_MultiAssign(lvalues, rvalues);

			m_ValueStack.CropAtCount(m_ValueStack.Count - i.NumVal - i.NumVal2);
		}

		private void Internal_MultiAssign(Slice<DynValue> lValues, Slice<DynValue> rValues)
		{
			int li = 0;
			int rValues_Count = rValues.Count;
			int lValues_Count = lValues.Count;

			for (int ri = 0; ri < rValues_Count && li < lValues_Count; ri++, li++)
			{
				DynValue vv = rValues[ri];

				if ((ri != rValues_Count - 1) || (vv.Type != DataType.Tuple))
				{
					Internal_Assign(lValues[li], vv.ToScalar());
				}
				else
				{
					for (int rri = 0, len = vv.Tuple.Length; rri < len && li < lValues_Count; rri++, li++)
					{
						Internal_Assign(lValues[li], vv.Tuple[rri].ToScalar());
					}
				}
			}
		}










		private int ExecAdd(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			double? rn = r.CastToNumber();
			double? ln = l.CastToNumber();

			if (ln.HasValue && rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(ln.Value + rn.Value));
				return instructionPtr;
			}
			else 
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__add", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non numbers");
			}
		}

		private int ExecSub(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			double? rn = r.CastToNumber();
			double? ln = l.CastToNumber();

			if (ln.HasValue && rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(ln.Value - rn.Value));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__sub", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non numbers");
			}
		}


		private int ExecMul(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			double? rn = r.CastToNumber();
			double? ln = l.CastToNumber();

			if (ln.HasValue && rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(ln.Value * rn.Value));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__mul", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non numbers");
			}
		}		
		
		private int ExecMod(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			double? rn = r.CastToNumber();
			double? ln = l.CastToNumber();

			if (ln.HasValue && rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(Math.IEEERemainder(ln.Value, rn.Value)));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__div", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non numbers");
			}
		}

		private int ExecDiv(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			double? rn = r.CastToNumber();
			double? ln = l.CastToNumber();

			if (ln.HasValue && rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(ln.Value / rn.Value));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__div", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non numbers");
			}
		}
		private int ExecPower(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			double? rn = r.CastToNumber();
			double? ln = l.CastToNumber();

			if (ln.HasValue && rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(Math.Pow(ln.Value, rn.Value)));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__pow", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non numbers");
			}

		}


		private int ExecNeg(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			double? rn = r.CastToNumber();

			if (rn.HasValue)
			{
				m_ValueStack.Push(DynValue.NewNumber(-rn.Value));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeUnaryMetaMethod(r, "__unm", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Arithmetic on non number");
			}
		}


		private int ExecEq(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			if (object.ReferenceEquals(r, l))
			{
				m_ValueStack.Push(DynValue.True);
			}
			else if (r.Type != l.Type)
			{
				m_ValueStack.Push(DynValue.False);
			}
			else if ((l.Type == DataType.Table || l.Type == DataType.UserData) && (l.Meta != null) && (l.Meta == r.Meta))
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__eq", instructionPtr);
				if (ip < 0)
					m_ValueStack.Push(DynValue.NewBoolean(r.Equals(l)));
				else
					return ip;
			}
			else
			{
				m_ValueStack.Push(DynValue.NewBoolean(r.Equals(l)));
			}

			return instructionPtr;
		}

		private int ExecLess(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			if (l.Type == DataType.Number && r.Type == DataType.Number)
			{
				m_ValueStack.Push(DynValue.NewBoolean(l.Number < r.Number));
			}
			else if (l.Type == DataType.String && r.Type == DataType.String)
			{
				m_ValueStack.Push(DynValue.NewBoolean(l.String.CompareTo(r.String) < 0));
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__lt", instructionPtr);
				if (ip < 0)
					throw new ScriptRuntimeException(null, "Can't compare on non number non string");
				else
					return ip;
			}

			return instructionPtr;
		}

		private int ExecLessEq(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			if (l.Type == DataType.Number && r.Type == DataType.Number)
			{
				m_ValueStack.Push(DynValue.NewBoolean(l.Number <= r.Number));
			}
			else if (l.Type == DataType.String && r.Type == DataType.String)
			{
				m_ValueStack.Push(DynValue.NewBoolean(l.String.CompareTo(r.String) <= 0));
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__le", instructionPtr);
				if (ip < 0)
				{
					ip = Internal_InvokeBinaryMetaMethod(r, l, "__lt", instructionPtr);

					if (ip < 0)
						throw new ScriptRuntimeException(null, "Can't compare on non number non string");
					else
						return ip;
				}
				else
					return ip;
			}

			return instructionPtr;
		}

		private int ExecLen(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();

			if (r.Type == DataType.String)
				m_ValueStack.Push(DynValue.NewNumber(r.String.Length));
			else
			{
				int ip = Internal_InvokeUnaryMetaMethod(r, "__len", instructionPtr);
				if (ip >= 0) 
					return ip;
				else if (r.Type == DataType.Table)
					m_ValueStack.Push(DynValue.NewNumber(r.Table.Length));
				else 
					throw new ScriptRuntimeException(null, "Arithmetic on non number");
			}

			return instructionPtr;
		}

		private int ExecConcat(Instruction i, int instructionPtr)
		{
			DynValue r = m_ValueStack.Pop();
			DynValue l = m_ValueStack.Pop();

			string rs = r.CastToString();
			string ls = l.CastToString();

			if (rs != null && ls != null)
			{
				m_ValueStack.Push(DynValue.NewString(ls + rs));
				return instructionPtr;
			}
			else
			{
				int ip = Internal_InvokeBinaryMetaMethod(l, r, "__concat", instructionPtr);
				if (ip >= 0) return ip;
				else throw new ScriptRuntimeException(null, "Concatenation on non strings");
			}

		}








	}
}
