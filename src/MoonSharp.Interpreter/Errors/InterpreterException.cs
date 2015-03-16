using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter
{
	public class InterpreterException : Exception 
	{
		protected InterpreterException(Exception ex)
			: base(ex.Message, ex)
		{

		}

		protected InterpreterException(string message)
			: base(message)
		{

		}

		protected InterpreterException(string format, params object[] args)
			: base(string.Format(format, args))
		{

		}

		public int InstructionPtr { get; internal set; }

		public IList<MoonSharp.Interpreter.Debugging.WatchItem> CallStack { get; internal set; }

		public string DecoratedMessage { get; internal set; }

		internal void DecorateMessage(Script script, SourceRef sref, int ip = -1)
		{
			if (sref != null)
			{
				this.DecoratedMessage = string.Format("{0}: {1}", sref.FormatLocation(script), this.Message);
			}
			else
			{
				this.DecoratedMessage = string.Format("bytecode:{0}: {1}", ip, this.Message);
			}

		}


	}
}
