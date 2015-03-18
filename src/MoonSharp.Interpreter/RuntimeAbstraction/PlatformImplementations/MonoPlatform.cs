using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class MonoPlatform : Clr2Platform
	{
		private bool m_IsAOT;

		private static void AttemptJit()
		{
			Expression e = Expression.Constant(5, typeof(int));
			var lambda = Expression.Lambda<Func<int>>(e);
			lambda.Compile();
		}

		private static bool IsRunningOnAOT()
		{
			try
			{
				AttemptJit();
				return false;
			}
			catch (ExecutionEngineException)
			{
				return true;
			}
		}


		public MonoPlatform()
		{
			m_IsAOT = IsRunningOnAOT();
		}

		public override string Name
		{
			get { return DecorateName("mono"); }
		}

		public override bool IsAOT()
		{
			return m_IsAOT;
		}

		protected string DecorateName(string name)
		{
			return m_IsAOT ? name + "-aot" : name;
		}
	}
}
