using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class CompositeStatement : Statement 
	{
		Statement[] m_Statements;

		public CompositeStatement(LuaParser.StatContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			if (context.ChildCount > 0)
			{

				m_Statements = context.children
					.Select(t => NodeFactory.CreateStatement(t, lcontext))
					.Where(s => s != null)
					.ToArray();
			}
		}

		public CompositeStatement(LuaParser.BlockContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			if (context.ChildCount > 0)
			{
				m_Statements = context.children
					.Select(t => NodeFactory.CreateStatement(t, lcontext))
					.Where(s => s != null)
					.ToArray();
			}
		}

		public CompositeStatement(LuaParser.ChunkContext context, ScriptLoadingContext lcontext)
			: this((LuaParser.BlockContext)context.children.First(), lcontext)
		{
		}

		public RValue ExecRoot(RuntimeScope scope)
		{
			ExecutionFlow flow = this.Exec(scope);
			return base.GetReturnValueAtReturnPoint(flow);
		}



		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			if (m_Statements != null)
			{
				foreach (Statement s in m_Statements)
				{
					ExecutionFlow flow = s.Exec(scope);

					if (flow.ChangesFlow()) 
						return flow;
				}
			}

			return ExecutionFlow.None;
		}

		public override void Compile(Execution.VM.Chunk bc)
		{
			if (m_Statements != null)
			{
				foreach (Statement s in m_Statements)
				{
					if (!(s is NullStatement))
					{
						bc.Debug(s.TreeNode);
						s.Compile(bc);
					}
				}
			}
		}
	}
}
