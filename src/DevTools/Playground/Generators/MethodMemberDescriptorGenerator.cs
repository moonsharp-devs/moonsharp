using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;

namespace Playground.Generators
{
	class MethodMemberDescriptorGenerator : IHardwireGenerator
	{
		public string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.MethodMemberDescriptor"; }
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerator generator, CodeTypeMemberCollection members)
		{
			string className = "HardwiredDescriptor_Method_" + Guid.NewGuid().ToString("N");

			CodeTypeDeclaration classCode = new CodeTypeDeclaration(className);

			classCode.TypeAttributes = System.Reflection.TypeAttributes.NestedPrivate | System.Reflection.TypeAttributes.Sealed;

			classCode.BaseTypes.Add(typeof(HardwiredMethodMemberDescriptor));

			// ctor:
			//protected void Initialize(string funcName, bool isStatic, ParameterDescriptor[] parameters, bool isExtensionMethod)

			CodeConstructor ctor = new CodeConstructor();
			ctor.Attributes = MemberAttributes.Assembly;

			List<CodeExpression> paramExps = new List<CodeExpression>();
			List<ParameterDescriptor> paramDescs = new List<ParameterDescriptor>();

			Table tpars = table.Get("params").Table;
			int paramNum = 0;
			int optionalNum = 0;

			for (int i = 1; i < tpars.Length; i++)
			{
				Table tpar = tpars.Get(i).Table;

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

				paramExps.Add(new CodeObjectCreateExpression(typeof(ParameterDescriptor), new CodeExpression[] {
					ename, etype, hasDefaultValue, defaultValue, isOut, isRef,
					isVarArg }
				));

				paramNum += 1;

				paramDescs.Add(new ParameterDescriptor(tpar.Get("origtype").String, typeof(object), 
					tpar.Get("default").Boolean, null, tpar.Get("out").Boolean, 
					tpar.Get("ref").Boolean, tpar.Get("varargs").Boolean));

				if (tpar.Get("default").Boolean) 
					optionalNum += 1;
			}

			List<CodeExpression> initParams = new List<CodeExpression>();

			initParams.Add(new CodePrimitiveExpression(table.Get("name").String));
			initParams.Add(new CodePrimitiveExpression(table.Get("static").Boolean));

			initParams.Add(new CodeArrayCreateExpression(typeof(ParameterDescriptor), paramExps.ToArray())); 

			initParams.Add(new CodePrimitiveExpression(table.Get("extension").Boolean));
			

			ctor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Initialize", initParams.ToArray()));

			classCode.Members.Add(ctor);

			// protected abstract object Invoke(object[] pars);

			CodeMemberMethod m = new CodeMemberMethod();
			m.Name = "Invoke";
			m.Attributes = MemberAttributes.Override | MemberAttributes.Family;
			m.ReturnType = new CodeTypeReference(typeof(object));
			m.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "obj"));
			m.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object[]), "pars"));
			m.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "argscount"));

			bool isVoid = table.Get("ret").String == "System.Void";
			bool isCtor = table.Get("ctor").Boolean;
			bool isStatic = table.Get("static").Boolean;
			bool isExtension = table.Get("extension").Boolean;

			List<CodeExpression[]> calls = new List<CodeExpression[]>();

			var paramArray = new CodeVariableReferenceExpression("pars");
			var paramThis =
				new CodeCastExpression(table.Get("decltype").String, new CodeVariableReferenceExpression("obj"));
			var paramArgsCount = new CodeVariableReferenceExpression("argscount");

			for(int callidx = paramNum - optionalNum; callidx <= paramNum; callidx++)
			{
				List<CodeExpression> pars = new List<CodeExpression>();

				for(int i = 0; i < callidx; i++)
				{
					var objexp = new CodeArrayIndexerExpression(paramArray, new CodePrimitiveExpression(i));

					var castexp = new CodeCastExpression(paramDescs[i].Name, objexp);

					pars.Add(castexp);
				}

				calls.Add(pars.ToArray());
			}

			for (int i = 0; i < calls.Count - 1; i++)
			{
				int argcnt = calls[i].Length;

				CodeExpression condition = new CodeBinaryOperatorExpression(paramArgsCount,
						CodeBinaryOperatorType.LessThanOrEqual, new CodePrimitiveExpression(argcnt));

				var ifs = new CodeConditionStatement(condition, GenerateCall(table, isVoid, isCtor, isStatic, isExtension, calls[i], paramArray, paramThis).OfType < CodeStatement>().ToArray());

				m.Statements.Add(ifs);
			}

			m.Statements.AddRange(GenerateCall(table, isVoid, isCtor, isStatic, isExtension, calls[calls.Count - 1], paramArray, paramThis));



			classCode.Members.Add(m);
			members.Add(classCode);
			return new CodeExpression[] { new CodeObjectCreateExpression(className) };
		}

		private CodeStatementCollection GenerateCall(Table table, bool isVoid, bool isCtor, bool isStatic, bool isExtension, CodeExpression[] codeExpression, CodeExpression paramArray, CodeExpression paramThis)
		{
			CodeStatementCollection coll = new CodeStatementCollection();

			if (isCtor)
			{
				coll.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(table.Get("ret").String, codeExpression)));
			}
			else if (isStatic)
			{
				var expr = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(UserData)), table.Get("name").String, codeExpression);

				GenerateReturnStatement(isVoid, coll, expr);
			}
			else
			{
				var expr = new CodeMethodInvokeExpression(paramThis, table.Get("name").String, codeExpression);

				GenerateReturnStatement(isVoid, coll, expr);
			}

			return coll;
		}

		private static void GenerateReturnStatement(bool isVoid, CodeStatementCollection coll, CodeMethodInvokeExpression expr)
		{
			if (isVoid)
			{
				coll.Add(new CodeExpressionStatement(expr));
				coll.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
			}
			else
				coll.Add(new CodeMethodReturnStatement(expr));
		}
	}
}
