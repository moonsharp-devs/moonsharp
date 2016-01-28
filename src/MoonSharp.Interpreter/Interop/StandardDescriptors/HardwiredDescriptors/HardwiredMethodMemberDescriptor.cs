using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors
{
	public abstract class HardwiredMethodMemberDescriptor : FunctionMemberDescriptorBase
	{
		public override DynValue Execute(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			this.CheckAccess(MemberDescriptorAccess.CanExecute, obj);

			List<int> outParams = null;
			object[] pars = base.BuildArgumentList(script, obj, context, args, out outParams);
			object retv = Invoke(obj, pars, CalcArgsCount(pars));

			return DynValue.FromObject(script, retv);
		}

		private int CalcArgsCount(object[] pars)
		{
			int count = pars.Length;

			for(int i = 0; i < pars.Length; i++)
				if (Parameters[i].HasDefaultValue && (pars[i] is DefaultValue))
				{
					count -= 1;
				}

			return count;
		}

		protected abstract object Invoke(object obj, object[] pars, int argscount);
	}
}
