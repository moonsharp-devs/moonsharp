using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Tree
{
	static class NodeFactory
	{

		public static Statement CreateStatement(IParseTree tree, ScriptLoadingContext lcontext)
		{
			if (tree is Antlr4.Runtime.Tree.TerminalNodeImpl)
			{ }

			if (tree is LuaParser.BlockContext)
				return new CompositeStatement((LuaParser.BlockContext)tree, lcontext);

			//if (tree is LuaParser.ChunkContext)
			//	return new ChunkStatement((LuaParser.ChunkContext)tree, lcontext, null);

			if (tree is LuaParser.Stat_funcdefContext)
				return new ANTLR_FunctionDefinitionStatement((LuaParser.Stat_funcdefContext)tree, lcontext);

			if (tree is LuaParser.Stat_localfuncdefContext)
				return new ANTLR_FunctionDefinitionStatement((LuaParser.Stat_localfuncdefContext)tree, lcontext);

			if (tree is LuaParser.Stat_functioncallContext)
				return new ANTLR_FunctionCallStatement((LuaParser.Stat_functioncallContext)tree, lcontext);

			if (tree is LuaParser.RetstatContext)
				return new ReturnStatement((LuaParser.RetstatContext)tree, lcontext);

			if (tree is LuaParser.Stat_ifblockContext)
				return new IfStatement((LuaParser.Stat_ifblockContext)tree, lcontext);

			if (tree is LuaParser.Stat_nulstatementContext)
				return new NullStatement((LuaParser.Stat_nulstatementContext)tree, lcontext);

			if (tree is LuaParser.Stat_assignmentContext)
				return new AssignmentStatement((LuaParser.Stat_assignmentContext)tree, lcontext);

			if (tree is LuaParser.Stat_labelContext)
				return new LabelStatement((LuaParser.Stat_labelContext)tree, lcontext);

			if (tree is LuaParser.Stat_localassignmentContext)
				return new AssignmentStatement((LuaParser.Stat_localassignmentContext)tree, lcontext);

			if (tree is LuaParser.Stat_breakContext)
				return new BreakStatement((LuaParser.Stat_breakContext)tree, lcontext);

			if (tree is LuaParser.Stat_forloopContext)
				return new ForLoopStatement((LuaParser.Stat_forloopContext)tree, lcontext);

			if (tree is LuaParser.Stat_foreachloopContext)
				return new ForEachLoopStatement((LuaParser.Stat_foreachloopContext)tree, lcontext);

			if (tree is LuaParser.Stat_whiledoloopContext)
				return new WhileStatement((LuaParser.Stat_whiledoloopContext)tree, lcontext);

			if (tree is LuaParser.Stat_repeatuntilloopContext)
				return new RepeatStatement((LuaParser.Stat_repeatuntilloopContext)tree, lcontext);

			if (tree is LuaParser.Stat_doblockContext)
				return new ScopeBlockStatement((LuaParser.Stat_doblockContext)tree, lcontext);

			throw new SyntaxErrorException(tree, "Unexpected statement type: {0}", tree.GetType());
		}




		public static Expression CreateExpression(IParseTree tree, ScriptLoadingContext lcontext)
		{
			IParseTree originalTree = tree;

			// prune dummy tree nodes 
			while (true)
			{
				//if (tree is LuaParser.Exp_logicOrfallbackContext) tree = ((LuaParser.Exp_logicOrfallbackContext)tree).logicAndExp();
				//else if (tree is LuaParser.Exp_logicAndfallbackContext) tree = ((LuaParser.Exp_logicAndfallbackContext)tree).compareExp();
				//else if (tree is LuaParser.Exp_comparefallbackContext) tree = ((LuaParser.Exp_comparefallbackContext)tree).strcatExp();
				//else if (tree is LuaParser.Exp_strcastfallbackContext) tree = ((LuaParser.Exp_strcastfallbackContext)tree).addsubExp();
				//else if (tree is LuaParser.Exp_addsubfallbackContext) tree = ((LuaParser.Exp_addsubfallbackContext)tree).muldivExp();
				//else if (tree is LuaParser.Exp_muldivfallbackContext) tree = ((LuaParser.Exp_muldivfallbackContext)tree).unaryExp();
				//else if (tree is LuaParser.Exp_unaryfallbackContext) tree = ((LuaParser.Exp_unaryfallbackContext)tree).powerExp();
				//else if (tree is LuaParser.Exp_powerfallbackContext) tree = ((LuaParser.Exp_powerfallbackContext)tree).expterm();
				//else if (tree is LuaParser.ExptermContext) tree = tree.GetChild(0);
				//else 
				if (tree is LuaParser.VarOrExpContext)
				{
					// this whole rubbish just to detect adjustments to 1 arg of tuples
					if ((tree.ChildCount > 0))
					{
						Antlr4.Runtime.Tree.TerminalNodeImpl token = tree.GetChild(0) as Antlr4.Runtime.Tree.TerminalNodeImpl;

						if (token != null && token.GetText() == "(")
						{
							var subTree = tree.EnumChilds().Single(t => !(t is Antlr4.Runtime.Tree.TerminalNodeImpl));
							return new AdjustmentExpression(tree, lcontext, subTree);
						}
					}

					tree = tree.EnumChilds().Single(t => !(t is Antlr4.Runtime.Tree.TerminalNodeImpl));
				}
				else break;
			}

			//if (tree is LuaParser.ParenthesizedExpressionContext)
			//{
			//	return new AdjustmentExpression(tree, lcontext, ((LuaParser.ParenthesizedExpressionContext)tree).exp());
			//}

			//if (tree is LuaParser.Exp_addsubContext ||
			//	tree is LuaParser.Exp_compareContext ||
			//	tree is LuaParser.Exp_logicAndContext ||
			//	tree is LuaParser.Exp_logicOrContext ||
			//	tree is LuaParser.Exp_muldivContext ||
			//	tree is LuaParser.Exp_powerContext ||
			//	tree is LuaParser.Exp_strcatContext ||
			//	tree is LuaParser.Exp_unaryContext)
			//{
			//	return new OperatorExpression(tree, lcontext);
			//}

			if (tree is LuaParser.Exp_nilContext) return new ANTLR_LiteralExpression(tree, lcontext, DynValue.Nil);
			if (tree is LuaParser.Exp_trueContext) return new ANTLR_LiteralExpression(tree, lcontext, DynValue.True);
			if (tree is LuaParser.Exp_falseContext) return new ANTLR_LiteralExpression(tree, lcontext, DynValue.False);

			if (tree is LuaParser.Exp_numberContext) tree = ((LuaParser.Exp_numberContext)tree).number();
			if (tree is LuaParser.Exp_stringContext) tree = ((LuaParser.Exp_stringContext)tree).@string();
			if (tree is LuaParser.Exp_varargsContext) tree = ((LuaParser.Exp_varargsContext)tree).vararg();

			if (tree is LuaParser.Exp_anonfuncContext) return new ANTLR_FunctionDefinitionExpression(((LuaParser.Exp_anonfuncContext)tree), lcontext);
			if (tree is LuaParser.Exp_prefixexpContext) tree = ((LuaParser.Exp_prefixexpContext)tree).prefixexp();
			if (tree is LuaParser.Exp_tabctorContext) tree = ((LuaParser.Exp_tabctorContext)tree).tableconstructor();
			if (tree is LuaParser.Exp_powerContext) return new ANTLR_PowerOperatorExpression(tree, lcontext);
			if (tree is LuaParser.Exp_unaryContext) return new UnaryOperatorExpression(tree, lcontext);
			if (tree is LuaParser.Exp_binaryContext) return ANTLR_BinaryOperatorExpression.CreateSubTree(tree, lcontext);

			if (tree is Antlr4.Runtime.Tree.TerminalNodeImpl)
			{
				string txt = tree.GetText();
				if (txt == null) return null;
				//else if (txt == "nil") return new LiteralExpression(tree, lcontext, DynValue.Nil);
				//else if (txt == "false") return new LiteralExpression(tree, lcontext, DynValue.False);
				//else if (txt == "true") return new LiteralExpression(tree, lcontext, DynValue.True);
				else return null;
			}

			if (tree is LuaParser.PrefixexpContext)
			{
				var prefix = (LuaParser.PrefixexpContext)tree;
				if (tree.EnumChilds().OfType<LuaParser.NameAndArgsContext>().Any())
					return new ANTLR_FunctionCallChainExpression(prefix, lcontext);
				else
					return CreateExpression(prefix.varOrExp(), lcontext);
			}

			if (tree is LuaParser.VarContext)
				return CreateVariableExpression((LuaParser.VarContext)tree, lcontext);

			if (tree is LuaParser.ExplistContext)
				return new ExprListExpression((LuaParser.ExplistContext)tree, lcontext);

			if (tree is LuaParser.AnonfunctiondefContext)
				return new ANTLR_FunctionDefinitionExpression((LuaParser.AnonfunctiondefContext)tree, lcontext);

			if (tree is LuaParser.StringContext)
				return new ANTLR_LiteralExpression((LuaParser.StringContext)tree, lcontext);

			if (tree is LuaParser.NumberContext)
				return new ANTLR_LiteralExpression((LuaParser.NumberContext)tree, lcontext);

			if (tree is LuaParser.TableconstructorContext)
				return new TableConstructor((LuaParser.TableconstructorContext)tree, lcontext);

			if (tree is LuaParser.VarargContext)
				return new SymbolRefExpression((LuaParser.VarargContext)tree, lcontext);

			throw new SyntaxErrorException(tree, "Unexpected expression type: {0}", tree.GetType());
		}

		public static Expression CreateVariableExpression(LuaParser.VarContext varContext, ScriptLoadingContext lcontext)
		{
			Expression varExp;
			var NAME = varContext.NAME();

			if (NAME != null)
			{
				varExp = new SymbolRefExpression(NAME, lcontext);
			}
			else
			{
				varExp = CreateExpression(varContext.exp(), lcontext);
			}

			foreach (var suffix in varContext.varSuffix())
			{
				var nameAndArgs = suffix.nameAndArgs();
				var exp = suffix.exp();
				var suff_NAME = suffix.NAME();
				Expression indexExp;
				if (exp != null)
					indexExp = CreateExpression(exp, lcontext);
				else
					indexExp = new ANTLR_LiteralExpression(suff_NAME, lcontext, DynValue.NewString(suff_NAME.GetText()));

				if (nameAndArgs != null && nameAndArgs.Length > 0)
				{
					varExp = new ANTLR_FunctionCallChainExpression(suffix, lcontext, varExp, nameAndArgs);
				}

				varExp = new IndexExpression(suffix, lcontext, varExp, indexExp);
			}

			return varExp;
		}



		public static Expression[] CreateExpressions(IParseTree tree, ScriptLoadingContext lcontext)
		{
			if (tree is LuaParser.ExplistContext)
				return new ExprListExpression((LuaParser.ExplistContext)tree, lcontext).GetExpressions();

			return new Expression[] { CreateExpression(tree, lcontext) };
		}

		public static Expression[] CreateExpessionArray(IList<IParseTree> expressionNodes, ScriptLoadingContext lcontext)
		{
			List<Expression> exps = new List<Expression>();

			foreach (var c in expressionNodes)
			{
				var e = NodeFactory.CreateExpression(c, lcontext);

				if (e != null)
					exps.Add(e);
			}

			return exps.ToArray();
		}

		public static IVariable[] CreateVariablesArray(IList<LuaParser.VarContext> expressionNodes, ScriptLoadingContext lcontext)
		{
			List<IVariable> exps = new List<IVariable>();

			foreach (var c in expressionNodes)
			{
				var e = NodeFactory.CreateVariableExpression(c, lcontext) as IVariable;

				if (e != null)
					exps.Add(e);
			}

			return exps.ToArray();
		}


		internal static IVariable[] CreateLocalVariablesArray(IParseTree context, ITerminalNode[] terminalNodes, ScriptLoadingContext lcontext)
		{
			List<IVariable> exps = new List<IVariable>();

			foreach (var n in terminalNodes)
			{
				string name = n.GetText();
				var localVar = lcontext.Scope.TryDefineLocal(name);
				var symbol = new SymbolRefExpression(context, lcontext, localVar) as IVariable;

				if (symbol != null)
					exps.Add(symbol);
			}

			return exps.ToArray();
		}
	}
}
