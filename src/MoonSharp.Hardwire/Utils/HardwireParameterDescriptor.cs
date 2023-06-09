using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;

namespace MoonSharp.Hardwire.Utils
{
	public class HardwireParameterDescriptor
	{
		public CodeExpression Expression { get; private set; }
		public string ParamType { get; private set; }
		public bool HasDefaultValue { get; private set; }
		public bool IsOut { get; private set; }
		public bool IsRef { get; private set; }
		public string TempVarName { get; private set; }

		public HardwireParameterDescriptor(Table tpar)
		{
			CodeExpression ename = new CodePrimitiveExpression(tpar.Get("name").String);
			CodeExpression etype = new CodeTypeOfExpression(tpar.Get("origtype").String);
			CodeExpression hasDefaultValue = new CodePrimitiveExpression(tpar.Get("default").Boolean);
			CodeExpression defaultValue = tpar.Get("default").Boolean ? (CodeExpression)(new CodeObjectCreateExpression(typeof(DefaultValue))) :
				(CodeExpression)(new CodePrimitiveExpression(null));
			CodeExpression isOut = new CodePrimitiveExpression(tpar.Get("out").Boolean);
			CodeExpression isRef = new CodePrimitiveExpression(tpar.Get("ref").Boolean);
			CodeExpression isVarArg = new CodePrimitiveExpression(tpar.Get("varargs").Boolean);
			CodeExpression restrictType = tpar.Get("restricted").Boolean ? (CodeExpression)(new CodeTypeOfExpression(tpar.Get("type").String)) :
				(CodeExpression)(new CodePrimitiveExpression(null));

			Expression = new CodeObjectCreateExpression(typeof(ParameterDescriptor), new CodeExpression[] {
					ename, etype, hasDefaultValue, defaultValue, isOut, isRef,
					isVarArg }
			);

			ParamType = tpar.Get("origtype").String;
			HasDefaultValue = tpar.Get("default").Boolean;
			IsOut = tpar.Get("out").Boolean;
			IsRef = tpar.Get("ref").Boolean;
		}

		public static List<HardwireParameterDescriptor> LoadDescriptorsFromTable(Table t)
		{
			List<HardwireParameterDescriptor> list = new List<HardwireParameterDescriptor>();

			for (int i = 1; i <= t.Length; i++)
			{
				list.Add(new HardwireParameterDescriptor(t.Get(i).Table));
			}

			return list;
		}


		public void SetTempVar(string varName)
		{
			if (!IsOut && !IsRef)
				throw new InvalidOperationException("ReplaceExprWithVar on byval param");

			TempVarName = varName;
		}
	}
}
