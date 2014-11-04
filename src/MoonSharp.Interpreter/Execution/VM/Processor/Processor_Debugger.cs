using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	// This part is practically written procedural style - it looks more like C than C#.
	// This is intentional so to avoid this-calls and virtual-calls as much as possible.
	// Same reason for the "sealed" declaration.
	sealed partial class Processor
	{
		internal void AttachDebugger(IDebugger debugger)
		{
			m_Debug.DebuggerAttached = debugger;
		}



		private void ListenDebugger(Instruction instr, int instructionPtr)
		{
			if (m_Debug.DebuggerAttached.IsPauseRequested() ||
				(instr.SourceCodeRef != null && instr.SourceCodeRef.Breakpoint && instr.SourceCodeRef != m_Debug.LastHlRef))
			{
				m_Debug.DebuggerCurrentAction = DebuggerAction.ActionType.None;
				m_Debug.DebuggerCurrentActionTarget = -1;
			}

			switch(m_Debug.DebuggerCurrentAction)
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
					if (instr.SourceCodeRef == null || instr.SourceCodeRef == m_Debug.LastHlRef) return;
					break;
				case DebuggerAction.ActionType.StepOver:
					if (instr.SourceCodeRef == null || instr.SourceCodeRef == m_Debug.LastHlRef || m_ExecutionStack.Count > m_Debug.ExStackDepthAtStep) return;
					break;
			}

						
			RefreshDebugger(false);

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
						ToggleBreakPoint(action);
						RefreshDebugger(true);
						break;
					case DebuggerAction.ActionType.Refresh:
						RefreshDebugger(false);
						break;
					case DebuggerAction.ActionType.HardRefresh:
						RefreshDebugger(true);
						break;
					case DebuggerAction.ActionType.None:
					default:
						break;
				}
			}
		}



		private void ToggleBreakPoint(DebuggerAction action)
		{
			SourceCode src = m_Script.GetSourceCode(action.SourceID);

			foreach (SourceRef srf in src.Refs)
			{
				if (srf.IncludesLocation(action.SourceID, action.SourceLine, action.SourceCol))
				{
					srf.Breakpoint = !srf.Breakpoint;

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

		}

		private void RefreshDebugger(bool hard)
		{
			List<string> watchList = m_Debug.DebuggerAttached.GetWatchItems();
			List<WatchItem> callStack = Debugger_GetCallStack();
			List<WatchItem> watches = Debugger_RefreshWatches(watchList);
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

		private List<WatchItem> Debugger_RefreshWatches(List<string> watchList)
		{
			return watchList.Select(w => Debugger_RefreshWatch(w)).ToList();
		}

		private WatchItem Debugger_RefreshWatch(string name)
		{
			SymbolRef L = FindSymbolByName(name);

			if (L != null)
			{
				DynValue v = this.GetGenericSymbol(L);

				return new WatchItem()
				{
					LValue = L,
					Value = v,
					Name = name
				};
			}
			else
			{
				return new WatchItem() { Name = name };
			}
		}

		private List<WatchItem> Debugger_GetCallStack()
		{
			List<WatchItem> wis = new List<WatchItem>();

			for (int i = 0; i < m_ExecutionStack.Count; i++)
			{
				var c = m_ExecutionStack.Peek(i);

				var I = m_RootChunk.Code[c.Debug_EntryPoint];

				string callname = I.OpCode == OpCode.BeginFn ? I.Name : null;


				wis.Add(new WatchItem()
				{
					Address = c.Debug_EntryPoint,
					BasePtr = c.BasePointer,
					RetAddress = c.ReturnAddress,
					Name = callname
				});
			}

			return wis;
		}
	}
}
