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
			repeat_execution:

			try
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
						case OpCode.Copy:
							m_ValueStack.Push(m_ValueStack.Peek(i.NumVal));
							break;
						case OpCode.Swap:
							ExecSwap(i);
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
						case OpCode.Jump:
							instructionPtr = i.NumVal;
							break;
						case OpCode.MkTuple:
							ExecMkTuple(i);
							break;
						case OpCode.Enter:
							NilifyBlockData(i);
							break;
						case OpCode.Leave:
						case OpCode.Exit:
							ClearBlockData(i);
							break;
						case OpCode.Closure:
							ExecClosure(i);
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
						case OpCode.JFor:
							instructionPtr = ExecJFor(i, instructionPtr);
							break;
						case OpCode.NewTable:
							m_ValueStack.Push(DynValue.NewTable(this.m_Script));
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
						case OpCode.Local:
							m_ValueStack.Push(m_ExecutionStack.Peek().LocalScope[i.Symbol.i_Index]);
							break;
						case OpCode.Upvalue:
							m_ValueStack.Push(m_ExecutionStack.Peek().ClosureScope[i.Symbol.i_Index]);
							break;
						case OpCode.StoreUpv:
							ExecStoreUpv(i);
							break;
						case OpCode.StoreLcl:
							ExecStoreLcl(i);
							break;
						case OpCode.TblInitN:
							ExecTblInitN(i);
							break;
						case OpCode.TblInitI:
							ExecTblInitI(i);
							break;
						case OpCode.Index:
							instructionPtr = ExecIndex(i, instructionPtr);
							break;
						case OpCode.PushEnv:
							m_ValueStack.Push(DynValue.NewTable(m_GlobalTable));
							break;
						case OpCode.IndexSet:
							instructionPtr = ExecIndexSet(i, instructionPtr);
							break;
						case OpCode.Invalid:
							throw new NotImplementedException(string.Format("Invalid opcode : {0}", i.Name));
						default:
							throw new NotImplementedException(string.Format("Execution for {0} not implented yet!", i.OpCode));
					}
				}
			}
			catch (ScriptRuntimeException ex)
			{
				while (m_ExecutionStack.Count > 0)
				{
					CallStackItem csi = PopToBasePointer();

					if (csi.ErrorHandler != null)
					{
						instructionPtr = csi.ReturnAddress;
						var argscnt = (int)(m_ValueStack.Pop().Number);
						m_ValueStack.RemoveLast(argscnt + 1);

						// +++ todo: add a *real* call to the error handler

						m_ValueStack.Push(DynValue.NewTuple(DynValue.False, DynValue.NewString(ex.Message)));

						goto repeat_execution;
					}
				}

				if (m_ExecutionStack.Count == 0)
				{
					throw;
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


		private void AssignLocal(SymbolRef symref, DynValue value)
		{
			var stackframe = m_ExecutionStack.Peek();

			DynValue v = stackframe.LocalScope[symref.i_Index];
			if (v == null)
				stackframe.LocalScope[symref.i_Index] = v = DynValue.NewNil();

			v.Assign(value);
		}

		private void ExecStoreLcl(Instruction i)
		{
			DynValue value = GetStoreValue(i);
			SymbolRef symref = i.Symbol;

			AssignLocal(symref, value);
		}

		private void ExecStoreUpv(Instruction i)
		{
			DynValue value = GetStoreValue(i);
			SymbolRef symref = i.Symbol;

			var stackframe = m_ExecutionStack.Peek();

			DynValue v = stackframe.ClosureScope[symref.i_Index];
			if (v == null)
				stackframe.ClosureScope[symref.i_Index] = v = DynValue.NewNil();

			v.Assign(value);
		}

		private void ExecSwap(Instruction i)
		{
			DynValue v1 = m_ValueStack.Peek(i.NumVal);
			DynValue v2 = m_ValueStack.Peek(i.NumVal2);

			m_ValueStack.Set(i.NumVal, v2);
			m_ValueStack.Set(i.NumVal2, v1);
		}


		private DynValue GetStoreValue(Instruction i)
		{
			int stackofs = i.NumVal;
			int tupleidx = i.NumVal2;

			DynValue v = m_ValueStack.Peek(stackofs);

			if (v.Type == DataType.Tuple)
			{
				return (tupleidx < v.Tuple.Length) ? v.Tuple[tupleidx] : DynValue.NewNil();
			}
			else
			{
				return (tupleidx == 0) ? v : DynValue.NewNil();
			}
		}

		private void ExecClosure(Instruction i)
		{
			Closure c = new Closure(this.m_Script, i.NumVal, i.SymbolList,
				i.SymbolList.Select(s => this.GetUpvalueSymbol(s)).ToList());

			m_ValueStack.Push(DynValue.NewClosure(c));
		}

		private DynValue GetUpvalueSymbol(SymbolRef s)
		{
			if (s.Type == SymbolRefType.Local)
				return m_ExecutionStack.Peek().LocalScope[s.i_Index];
			else if (s.Type == SymbolRefType.Upvalue)
				return m_ExecutionStack.Peek().ClosureScope[s.i_Index];
			else
				throw new Exception("unsupported symbol type");
		}

		private void ExecMkTuple(Instruction i)
		{
			Slice<DynValue> slice = new Slice<DynValue>(m_ValueStack, m_ValueStack.Count - i.NumVal, i.NumVal, false);

			var v = Internal_AdjustTuple(slice);

			m_ValueStack.RemoveLast(i.NumVal);

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

		private CallStackItem PopToBasePointer()
		{
			var csi = m_ExecutionStack.Pop();
			m_ValueStack.CropAtCount(csi.BasePointer);
			return csi;
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
					this.AssignLocal(I.SymbolList[i], DynValue.NewNil());
				}
				else if ((i == I.SymbolList.Length - 1) && (I.SymbolList[i].i_Name == "..."))
				{
					int len = numargs - i;
					DynValue[] varargs = new DynValue[len];

					for (int ii = 0; ii < len; ii++)
					{
						varargs[ii] = m_ValueStack.Peek(numargs - i - ii).CloneAsWritable();
					}

					this.AssignLocal(I.SymbolList[i], DynValue.NewTuple(varargs));
				}
				else
				{
					this.AssignLocal(I.SymbolList[i], m_ValueStack.Peek(numargs - i).CloneAsWritable());
				}
			}
		}



		private int Internal_ExecCall(int argsCount, int instructionPtr, CallbackFunction handler = null, CallbackFunction continuation = null)
		{
			DynValue fn = m_ValueStack.Peek(argsCount);

			if (fn.Type == DataType.ClrFunction)
			{
				IList<DynValue> args = new Slice<DynValue>(m_ValueStack, m_ValueStack.Count - argsCount, argsCount, false);
				var ret = fn.Callback.Invoke(new ScriptExecutionContext(this, fn.Callback), args);
				m_ValueStack.RemoveLast(argsCount + 1);
				m_ValueStack.Push(ret);

				return Internal_CheckForTailRequests(null, instructionPtr);
			}
			else if (fn.Type == DataType.Function)
			{
				m_ValueStack.Push(DynValue.NewNumber(argsCount));
				m_ExecutionStack.Push(new CallStackItem()
				{
					BasePointer = m_ValueStack.Count,
					ReturnAddress = instructionPtr,
					Debug_EntryPoint = fn.Function.ByteCodeLocation,
					ClosureScope = fn.Function.ClosureContext,
					ErrorHandler = handler,
					Continuation = continuation 
				});
				return fn.Function.ByteCodeLocation;
			}
			else
			{
				if (fn.MetaTable != null)
				{
					var m = fn.MetaTable.RawGet("__call");

					if (m != null && m.Type != DataType.Nil)
					{
						DynValue[] tmp = new DynValue[argsCount + 1];
						for (int i = 0; i < argsCount + 1; i++)
							tmp[i] = m_ValueStack.Pop();

						m_ValueStack.Push(m);

						for (int i = argsCount; i >= 0; i--)
							m_ValueStack.Push(tmp[i]);

						return Internal_ExecCall(argsCount + 1, instructionPtr, handler, continuation);
					}
				}

				throw new NotImplementedException("Can't call non function - " + fn.ToString());
			}
		}


		private int ExecRet(Instruction i)
		{
			CallStackItem csi;
			int retpoint = 0;

			if (i.NumVal == 0)
			{
				csi = PopToBasePointer();
				retpoint = csi.ReturnAddress;
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(DynValue.Nil);
			}
			else if (i.NumVal == 1)
			{
				var retval = m_ValueStack.Pop();
				csi = PopToBasePointer();
				retpoint = csi.ReturnAddress;
				var argscnt = (int)(m_ValueStack.Pop().Number);
				m_ValueStack.RemoveLast(argscnt + 1);
				m_ValueStack.Push(retval);
				retpoint = Internal_CheckForTailRequests(i, retpoint);
			}
			else
			{
				throw new InternalErrorException("RET supports only 0 and 1 ret val scenarios");
			}

			if (csi.Continuation != null)
				m_ValueStack.Push(csi.Continuation.Invoke(new ScriptExecutionContext(this, csi.Continuation),
					new DynValue[1] { m_ValueStack.Pop() }));

			return retpoint;
		}



		private int Internal_CheckForTailRequests(Instruction i, int instructionPtr)
		{
			DynValue tail = m_ValueStack.Peek(0);

			if (tail.Type == DataType.TailCallRequest)
			{
				m_ValueStack.Pop(); // discard tail call request

				TailCallData tcd = (TailCallData)tail.UserObject;

				m_ValueStack.Push(tcd.Function);

				for (int ii = 0; ii < tcd.Args.Length; ii++)
					m_ValueStack.Push(tcd.Args[ii]);

				//instructionPtr -= 1;
				return Internal_ExecCall(tcd.Args.Length, instructionPtr, tcd.ErrorHandler, tcd.Continuation);
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
			else if ((l.Type == DataType.Table || l.Type == DataType.UserData) && (l.MetaTable != null) && (l.MetaTable == r.MetaTable))
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

		private void ExecTblInitI(Instruction i)
		{
			// stack: tbl - val
			DynValue val = m_ValueStack.Pop();
			DynValue tbl = m_ValueStack.Peek();

			if (tbl.Type != DataType.Table)
				throw new InternalErrorException("Unexpected type in table ctor : {0}", tbl);

			tbl.Table.InitNextArrayKeys(val);
		}

		private void ExecTblInitN(Instruction i)
		{
			// stack: tbl - key - val
			DynValue val = m_ValueStack.Pop();
			DynValue key = m_ValueStack.Pop();
			DynValue tbl = m_ValueStack.Peek();

			if (tbl.Type != DataType.Table)
				throw new InternalErrorException("Unexpected type in table ctor : {0}", tbl);

			tbl.Table[key] = val.ToScalar();
		}

		private int ExecIndexSet(Instruction i, int instructionPtr)
		{
			int nestedMetaOps = 100; // sanity check, to avoid potential infinite loop here
			
			// stack: vals.. - base - index
			DynValue idx = i.Value ?? m_ValueStack.Pop();
			DynValue obj = m_ValueStack.Pop();
			var value = GetStoreValue(i);
			DynValue h = null;

			while (nestedMetaOps > 0)
			{
				--nestedMetaOps;

				if (obj.Type == DataType.Table)
				{
					if (!obj.Table[idx].IsNil())
					{
						obj.Table[idx] = value;
						return instructionPtr;
					}

					if (obj.MetaTable != null)
						h = obj.MetaTable.RawGet("__newindex");

					if (h == null || h.IsNil())
					{
						obj.Table[idx] = value;
						return instructionPtr;
					}
				}
				else
				{
					h = obj.MetaTable.RawGet("__newindex");

					if (h == null || h.IsNil())
						throw new ScriptRuntimeException("Can't index non table: {0}", obj);
				}

				if (h.Type == DataType.Function || h.Type == DataType.ClrFunction)
				{
					m_ValueStack.Push(h);
					m_ValueStack.Push(obj);
					m_ValueStack.Push(idx);
					m_ValueStack.Push(value);
					return Internal_ExecCall(3, instructionPtr);
				}
				else
				{
					obj = h;
					h = null;
				}
			}
			throw new ScriptRuntimeException("__newindex returning too many nested tables");
		}

		private int ExecIndex(Instruction i, int instructionPtr)
		{
			int nestedMetaOps = 100; // sanity check, to avoid potential infinite loop here

			// stack: base - index
			DynValue idx = i.Value ?? m_ValueStack.Pop();
			DynValue obj = m_ValueStack.Pop();

			DynValue h = null;

			while (nestedMetaOps > 0)
			{
				--nestedMetaOps;

				if (obj.Type == DataType.Table)
				{
					var v = obj.Table[idx];

					if (!v.IsNil())
					{
						m_ValueStack.Push(v);
						return instructionPtr;
					}

					if (obj.MetaTable != null)
						h = obj.MetaTable.RawGet("__index");

					if (h == null || h.IsNil())
					{
						m_ValueStack.Push(DynValue.NewNil());
						return instructionPtr;
					}
				}
				else
				{
					if (obj.MetaTable != null)
						h = obj.MetaTable.RawGet("__index");

					if (h == null || h.IsNil())
						throw new ScriptRuntimeException("Can't index non table: {0}", obj);
				}

				if (h.Type == DataType.Function || h.Type == DataType.ClrFunction)
				{
					m_ValueStack.Push(h);
					m_ValueStack.Push(obj);
					m_ValueStack.Push(idx);
					return Internal_ExecCall(2, instructionPtr);
				}
				else
				{
					obj = h;
					h = null;
				}
			}

			throw new ScriptRuntimeException("__index returning too many nested tables");
		}





	}
}
