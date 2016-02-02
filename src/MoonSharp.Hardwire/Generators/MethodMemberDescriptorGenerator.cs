using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Hardwire.Utils;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors;

namespace MoonSharp.Hardwire.Generators
{
	class MethodMemberDescriptorGenerator : IHardwireGenerator
	{
		public string ManagedType
		{
			get { return "MoonSharp.Interpreter.Interop.MethodMemberDescriptor"; }
		}

		public CodeExpression[] Generate(Table table, HardwireCodeGenerationContext generator, CodeTypeMemberCollection members)
		{
			string className = "M_" + Guid.NewGuid().ToString("N");

			CodeTypeDeclaration classCode = new CodeTypeDeclaration(className);

			classCode.TypeAttributes = System.Reflection.TypeAttributes.NestedPrivate | System.Reflection.TypeAttributes.Sealed;

			classCode.BaseTypes.Add(typeof(HardwiredMethodMemberDescriptor));

			// ctor:
			//protected void Initialize(string funcName, bool isStatic, ParameterDescriptor[] parameters, bool isExtensionMethod)

			CodeConstructor ctor = new CodeConstructor();
			ctor.Attributes = MemberAttributes.Assembly;

			List<HardwireParameterDescriptor> paramDescs = HardwireParameterDescriptor.LoadDescriptorsFromTable(table.Get("params").Table);


			int paramNum = paramDescs.Count;
			int optionalNum = paramDescs.Where(p => p.HasDefaultValue).Count();

			List<CodeExpression> initParams = new List<CodeExpression>();

			initParams.Add(new CodePrimitiveExpression(table.Get("name").String));
			initParams.Add(new CodePrimitiveExpression(table.Get("static").Boolean));

			initParams.Add(new CodeArrayCreateExpression(typeof(ParameterDescriptor), paramDescs.Select(e => e.Expression).ToArray())); 

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

			string declType = table.Get("decltype").String;
			var paramArray = new CodeVariableReferenceExpression("pars");
			var paramThis = isStatic
				? (CodeExpression)(new CodeTypeReferenceExpression(declType))
				: (CodeExpression)(new CodeCastExpression(declType, new CodeVariableReferenceExpression("obj")));

			var paramArgsCount = new CodeVariableReferenceExpression("argscount");

			bool specialName = table.Get("special").Boolean;

			for(int callidx = paramNum - optionalNum; callidx <= paramNum; callidx++)
			{
				List<CodeExpression> pars = new List<CodeExpression>();

				for(int i = 0; i < callidx; i++)
				{
					var objexp = new CodeArrayIndexerExpression(paramArray, new CodePrimitiveExpression(i));

					var castexp = new CodeCastExpression(paramDescs[i].ParamType, objexp);

					pars.Add(castexp);
				}

				calls.Add(pars.ToArray());
			}

			for (int i = 0; i < calls.Count - 1; i++)
			{
				int argcnt = calls[i].Length;

				CodeExpression condition = new CodeBinaryOperatorExpression(paramArgsCount,
						CodeBinaryOperatorType.LessThanOrEqual, new CodePrimitiveExpression(argcnt));

				var ifs = new CodeConditionStatement(condition, GenerateCall(table, generator, isVoid, isCtor, isStatic, isExtension, calls[i], paramThis, declType, specialName).OfType<CodeStatement>().ToArray());

				m.Statements.Add(ifs);
			}

			m.Statements.AddRange(GenerateCall(table, generator, isVoid, isCtor, isStatic, isExtension, calls[calls.Count - 1],  paramThis, declType, specialName));



			classCode.Members.Add(m);
			members.Add(classCode);
			return new CodeExpression[] { new CodeObjectCreateExpression(className) };
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



		private CodeStatementCollection GenerateCall(Table table, HardwireCodeGenerationContext generator, bool isVoid, bool isCtor, bool isStatic, bool isExtension, CodeExpression[] arguments, CodeExpression paramThis, string declaringType, bool specialName)
		{
			CodeStatementCollection coll = new CodeStatementCollection();

			if (isCtor)
			{
				coll.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(table.Get("ret").String, arguments)));
			}
			else if (specialName)
			{
				GenerateSpecialNameCall(table, generator, isVoid, isCtor, isStatic, isExtension,
					arguments, paramThis, declaringType, table.Get("name").String, coll);
			}
			else
			{
				var expr = new CodeMethodInvokeExpression(paramThis, table.Get("name").String, arguments);

				GenerateReturnStatement(isVoid, coll, expr);
			}

			return coll;
		}



		private void GenerateSpecialNameCall(Table table, HardwireCodeGenerationContext generator, bool isVoid, bool isCtor, bool isStatic, bool isExtension, CodeExpression[] arguments, CodeExpression paramThis, string declaringType, string specialName, CodeStatementCollection coll)
		{
			ReflectionSpecialName special = new ReflectionSpecialName(specialName);
			CodeExpression exp = null;
			CodeStatement stat = null;

			switch (special.Type)
			{
				case ReflectionSpecialNameType.IndexGetter:
					if (isStatic)
						EmitInvalid(generator, coll, "Static indexers are not supported by hardwired descriptors.");
					else
						exp = new CodeIndexerExpression(paramThis, arguments);
					break;
				case ReflectionSpecialNameType.IndexSetter:
					if (isStatic)
						EmitInvalid(generator, coll, "Static indexers are not supported by hardwired descriptors.");
					else
						stat = new CodeAssignStatement(new CodeIndexerExpression(paramThis,
							arguments.Take(arguments.Length - 1).ToArray()), arguments.Last());
					break;
				case ReflectionSpecialNameType.ImplicitCast:
				case ReflectionSpecialNameType.ExplicitCast:
					exp = paramThis;
					break;
				case ReflectionSpecialNameType.OperatorTrue:
					GenerateBooleanOperator(paramThis, coll, true);
					break;
				case ReflectionSpecialNameType.OperatorFalse:
					generator.Minor("'false' operator is implemented in terms of 'true' operator.");
					GenerateBooleanOperator(paramThis, coll, false);
					break;
				case ReflectionSpecialNameType.PropertyGetter:
					exp = new CodePropertyReferenceExpression(paramThis, special.Argument);
					break;
				case ReflectionSpecialNameType.PropertySetter:
					stat = new CodeAssignStatement(new CodePropertyReferenceExpression(paramThis, special.Argument), arguments[0]);
					break;
				case ReflectionSpecialNameType.OperatorAdd:
					exp = BinaryOperator(CodeBinaryOperatorType.Add, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorAnd:
					exp = BinaryOperator(CodeBinaryOperatorType.BitwiseAnd, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorOr:
					exp = BinaryOperator(CodeBinaryOperatorType.BitwiseOr, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorDec:
					exp = generator.TargetLanguage.UnaryDecrement(arguments[0]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support decrement operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.OperatorDiv:
					exp = BinaryOperator(CodeBinaryOperatorType.Divide, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorEq:
					exp = BinaryOperator(CodeBinaryOperatorType.ValueEquality, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorXor:
					exp = generator.TargetLanguage.BinaryXor(arguments[0], arguments[1]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support XOR operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.OperatorGt:
					exp = BinaryOperator(CodeBinaryOperatorType.GreaterThan, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorGte:
					exp = BinaryOperator(CodeBinaryOperatorType.GreaterThanOrEqual, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorInc:
					exp = generator.TargetLanguage.UnaryIncrement(arguments[0]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support increment operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.OperatorNeq:
					exp = BinaryOperator(CodeBinaryOperatorType.IdentityInequality, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorLt:
					exp = BinaryOperator(CodeBinaryOperatorType.LessThan, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorLte:
					exp = BinaryOperator(CodeBinaryOperatorType.LessThanOrEqual, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorNot:
					exp = generator.TargetLanguage.UnaryLogicalNot(arguments[0]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support logical NOT operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.OperatorMod:
					exp = BinaryOperator(CodeBinaryOperatorType.Modulus, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorMul:
					exp = BinaryOperator(CodeBinaryOperatorType.Multiply, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorCompl:
					exp = generator.TargetLanguage.UnaryOneComplement(arguments[0]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support bitwise NOT operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.OperatorSub:
					exp = BinaryOperator(CodeBinaryOperatorType.Subtract, paramThis, arguments);
					break;
				case ReflectionSpecialNameType.OperatorNeg:
					exp = generator.TargetLanguage.UnaryNegation(arguments[0]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support negation operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.OperatorUnaryPlus:
					exp = generator.TargetLanguage.UnaryPlus(arguments[0]);
					if (exp == null) EmitInvalid(generator, coll, string.Format("Language {0} does not support unary + operators.", generator.TargetLanguage.Name));
					break;
				case ReflectionSpecialNameType.AddEvent:
					break;
				case ReflectionSpecialNameType.RemoveEvent:
					break;
				default:
					break;
			}

			if (stat != null)
			{
				coll.Add(stat);
				exp = exp ?? new CodePrimitiveExpression(null);
			}

			if (exp != null)
				coll.Add(new CodeMethodReturnStatement(exp));
		}

		private CodeExpression BinaryOperator(CodeBinaryOperatorType codeBinaryOperatorType, CodeExpression paramThis, CodeExpression[] arguments)
		{
			return new CodeBinaryOperatorExpression(arguments[0], codeBinaryOperatorType, arguments[1]);
		}

		private void GenerateBooleanOperator(CodeExpression paramThis, CodeStatementCollection coll, bool boolOp)
		{
			coll.Add(new CodeConditionStatement(paramThis,
				new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(boolOp)) },
				new CodeStatement[] { new CodeMethodReturnStatement(new CodePrimitiveExpression(!boolOp)) }));
		}

		private void EmitInvalid(HardwireCodeGenerationContext generator, CodeStatementCollection coll, string message)
		{
			generator.Warning(message);
			coll.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(InvalidOperationException), 
				new CodePrimitiveExpression(message))));
		}





	}
}
