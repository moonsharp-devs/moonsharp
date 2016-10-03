using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	// This part is practically written procedural style - it looks more like C than C#.
	// This is intentional so to avoid this-calls and virtual-calls as much as possible.
	// Same reason for the "sealed" declaration.
	sealed partial class Processor
	{
		internal Instruction FindMeta(ref int baseAddress)
		{
			Instruction meta = m_RootChunk.Code[baseAddress];

			// skip nops
			while (meta.OpCode == OpCode.Nop)
			{
				baseAddress++;
				meta = m_RootChunk.Code[baseAddress];
			}

			if (meta.OpCode != OpCode.Meta)
				return null;

			return meta;
		}


		internal void AttachDebugger(IDebugger debugger)
		{
			m_Debug.DebuggerAttached = debugger;
		}

		internal bool DebuggerEnabled
		{
			get { return m_Debug.DebuggerEnabled; }
			set { m_Debug.DebuggerEnabled = value; }
		}


		private void ListenDebugger(Instruction instr, int instructionPtr)
		{
			if (m_Debug.DebuggerAttached.IsPauseRequested() ||
				(instr.SourceCodeRef != null && instr.SourceCodeRef.Breakpoint && instr.SourceCodeRef != m_Debug.LastHlRef))
			{
				m_Debug.DebuggerCurrentAction = DebuggerAction.ActionType.None;
				m_Debug.DebuggerCurrentActionTarget = -1;
			}

			switch (m_Debug.DebuggerCurrentAction)
			{
				case DebuggerAction.ActionType.Run:
					return;
				case DebuggerAction.ActionType.ByteCodeStepOver:
					if (m_Debug.DebuggerCurrentActionTarget != instructionPtr) return;
					break;
				case DebuggerAction.ActionType.ByteCodeStepOut:
				case DebuggerAction.ActionType.StepOut:
					if (m_ExecutionStack.Count >= m_Debug.ExStackDepthAtStep) return;
					break;
				case DebuggerAction.ActionType.StepIn:
					if ((m_ExecutionStack.Count >= m_Debug.ExStackDepthAtStep) && (instr.SourceCodeRef == null || instr.SourceCodeRef == m_Debug.LastHlRef)) return;
					break;
				case DebuggerAction.ActionType.StepOver:
					if (instr.SourceCodeRef == null || instr.SourceCodeRef == m_Debug.LastHlRef || m_ExecutionStack.Count > m_Debug.ExStackDepthAtStep) return;
					break;
			}


			RefreshDebugger(false, instructionPtr);

			while (true)
			{
				var action = m_Debug.DebuggerAttached.GetAction(instructionPtr, instr.SourceCodeRef);

				switch (action.Action)
				{
					case DebuggerAction.ActionType.StepIn:
					case DebuggerAction.ActionType.StepOver:
					case DebuggerAction.ActionType.StepOut:
					case DebuggerAction.ActionType.ByteCodeStepOut:
						m_Debug.DebuggerCurrentAction = action.Action;
						m_Debug.LastHlRef = instr.SourceCodeRef;
						m_Debug.ExStackDepthAtStep = m_ExecutionStack.Count;
						return;
					case DebuggerAction.ActionType.ByteCodeStepIn:
						m_Debug.DebuggerCurrentAction = DebuggerAction.ActionType.ByteCodeStepIn;
						m_Debug.DebuggerCurrentActionTarget = -1;
						return;
					case DebuggerAction.ActionType.ByteCodeStepOver:
						m_Debug.DebuggerCurrentAction = DebuggerAction.ActionType.ByteCodeStepOver;
						m_Debug.DebuggerCurrentActionTarget = instructionPtr + 1;
						return;
					case DebuggerAction.ActionType.Run:
						m_Debug.DebuggerCurrentAction = DebuggerAction.ActionType.Run;
						m_Debug.DebuggerCurrentActionTarget = -1;
						return;
					case DebuggerAction.ActionType.ToggleBreakpoint:
						ToggleBreakPoint(action, null);
						RefreshDebugger(true, instructionPtr);
						break;
					case DebuggerAction.ActionType.SetBreakpoint:
						ToggleBreakPoint(action, true);
						RefreshDebugger(true, instructionPtr);
						break;
					case DebuggerAction.ActionType.ClearBreakpoint:
						ToggleBreakPoint(action, false);
						RefreshDebugger(true, instructionPtr);
						break;
					case DebuggerAction.ActionType.Refresh:
						RefreshDebugger(false, instructionPtr);
						break;
					case DebuggerAction.ActionType.HardRefresh:
						RefreshDebugger(true, instructionPtr);
						break;
					case DebuggerAction.ActionType.None:
					default:
						break;
				}
			}
		}



		private bool ToggleBreakPoint(DebuggerAction action, bool? state)
		{
			SourceCode src = m_Script.GetSourceCode(action.SourceID);

			bool found = false;
			foreach (SourceRef srf in src.Refs)
			{
				if (srf.CannotBreakpoint)
					continue;

				if (srf.IncludesLocation(action.SourceID, action.SourceLine, action.SourceCol))
				{
					found = true;

					//System.Diagnostics.Debug.WriteLine(string.Format("BRK: found {0} for {1} on contains", srf, srf.Type));

					if (state == null)
						srf.Breakpoint = !srf.Breakpoint;
					else
						srf.Breakpoint = state.Value;

					if (srf.Breakpoint)
					{
						m_Debug.BreakPoints.Add(srf);
					}
					else
					{
						m_Debug.BreakPoints.Remove(srf);
					}
				}
			}

			if (!found)
			{
				int minDistance = int.MaxValue;
				SourceRef nearest = null;

				foreach (SourceRef srf in src.Refs)
				{
					if (srf.CannotBreakpoint)
						continue;

					int dist = srf.GetLocationDistance(action.SourceID, action.SourceLine, action.SourceCol);

					if (dist < minDistance)
					{
						minDistance = dist;
						nearest = srf;
					}
				}

				if (nearest != null)
				{
					//System.Diagnostics.Debug.WriteLine(string.Format("BRK: found {0} for {1} on distance {2}", nearest, nearest.Type, minDistance));

					if (state == null)
						nearest.Breakpoint = !nearest.Breakpoint;
					else
						nearest.Breakpoint = state.Value;

					if (nearest.Breakpoint)
					{
						m_Debug.BreakPoints.Add(nearest);
					}
					else
					{
						m_Debug.BreakPoints.Remove(nearest);
					}

					return true;
				}
				else
					return false;
			}
			else
				return true;
		}

		private void RefreshDebugger(bool hard, int instructionPtr)
		{
			SourceRef sref = GetCurrentSourceRef(instructionPtr);
			ScriptExecutionContext context = new ScriptExecutionContext(this, null, sref);

			List<DynamicExpression> watchList = m_Debug.DebuggerAttached.GetWatchItems();
			List<WatchItem> callStack = Debugger_GetCallStack(sref);
			List<WatchItem> watches = Debugger_RefreshWatches(context, watchList);
			List<WatchItem> vstack = Debugger_RefreshVStack();

			m_Debug.DebuggerAttached.Update(WatchType.CallStack, callStack);
			m_Debug.DebuggerAttached.Update(WatchType.Watches, watches);
			m_Debug.DebuggerAttached.Update(WatchType.VStack, vstack);

			if (hard)
				m_Debug.DebuggerAttached.RefreshBreakpoints(m_Debug.BreakPoints);
		}

		private List<WatchItem> Debugger_RefreshVStack()
		{
			List<WatchItem> lwi = new List<WatchItem>();
			for (int i = 0; i < Math.Min(32, m_ValueStack.Count); i++)
			{
				lwi.Add(new WatchItem()
				{
					Address = i,
					Value = m_ValueStack.Peek(i)
				});
			}

			return lwi;
		}

		private List<WatchItem> Debugger_RefreshWatches(ScriptExecutionContext context, List<DynamicExpression> watchList)
		{
			return watchList.Select(w => Debugger_RefreshWatch(context, w)).ToList();
		}

		private WatchItem Debugger_RefreshWatch(ScriptExecutionContext context, DynamicExpression dynExpr)
		{
			try
			{
				SymbolRef L = dynExpr.FindSymbol(context);
				DynValue v = dynExpr.Evaluate(context);

				return new WatchItem()
				{
					IsError = dynExpr.IsConstant(),
					LValue = L,
					Value = v,
					Name = dynExpr.ExpressionCode
				};
			}
			catch (Exception ex)
			{
				return new WatchItem()
				{
					IsError = true,
					Value = DynValue.NewString(ex.Message),
					Name = dynExpr.ExpressionCode
				};
			}
		}

		internal List<WatchItem> Debugger_GetCallStack(SourceRef startingRef)
		{
			List<WatchItem> wis = new List<WatchItem>();

			for (int i = 0; i < m_ExecutionStack.Count; i++)
			{
				var c = m_ExecutionStack.Peek(i);

				var I = m_RootChunk.Code[c.Debug_EntryPoint];

				string callname = I.OpCode == OpCode.Meta ? I.Name : null;

				if (c.ClrFunction != null)
				{
					wis.Add(new WatchItem()
					{
						Address = -1,
						BasePtr = -1,
						RetAddress = c.ReturnAddress,
						Location = startingRef,
						Name = c.ClrFunction.Name 
					});
				}
				else
				{
					wis.Add(new WatchItem()
					{
						Address = c.Debug_EntryPoint,
						BasePtr = c.BasePointer,
						RetAddress = c.ReturnAddress,
						Name = callname,
						Location = startingRef,
					});
				}

				startingRef = c.CallingSourceRef;

				if (c.Continuation != null)
				{
					wis.Add(new WatchItem()
					{
						Name = c.Continuation.Name,
						Location = SourceRef.GetClrLocation()
					});
				}


			}

			return wis;
		}
	}
}
