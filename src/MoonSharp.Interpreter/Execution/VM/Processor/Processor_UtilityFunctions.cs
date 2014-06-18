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
		private int Internal_InvokeUnaryMetaMethod(RValue op1, string eventName, int instructionPtr)
		{
			RValue m = null;

			if (op1.Meta != null)
			{
				RValue meta1 = op1.Meta.Table.RawGet(eventName);
				if (meta1 != null && meta1.Type != DataType.Nil)
					m = meta1;
			}

			if (m != null)
			{
				m_ValueStack.Push(m);
				m_ValueStack.Push(op1);
				return Internal_ExecCall(1, instructionPtr);
			}
			else
			{
				return -1;
			}
		}
		private int Internal_InvokeBinaryMetaMethod(RValue l, RValue r, string eventName, int instructionPtr)
		{
			var m = Internal_GetBinHandler(l, r, eventName);

			if (m != null)
			{
				m_ValueStack.Push(m);
				m_ValueStack.Push(l);
				m_ValueStack.Push(r);
				return Internal_ExecCall(2, instructionPtr);
			}
			else
			{
				return -1;
			}
		}

		private RValue Internal_GetBinHandler(RValue op1, RValue op2, string eventName)
		{
			if (op1.Meta != null)
			{
				RValue meta1 = op1.Meta.Table.RawGet(eventName);
				if (meta1 != null && meta1.Type != DataType.Nil)
					return meta1;
			}
			if (op2.Meta != null)
			{
				RValue meta2 = op2.Meta.Table.RawGet(eventName);
				if (meta2 != null && meta2.Type != DataType.Nil)
					return meta2;
			}
			return null;
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

	}
}
