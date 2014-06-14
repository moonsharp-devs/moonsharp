﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree
{
	internal class Loop : ILoop
	{
		public RuntimeScopeBlock Scope;
		public List<Instruction> BreakJumps = new List<Instruction>();

		public void CompileBreak(Chunk bc)
		{
			bc.Exit(Scope);
			BreakJumps.Add(bc.Jump(OpCode.Jump, -1));
		}
	}

}