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

			if (tree is LuaParser.ChunkContext)
				return new ChunkStatement((LuaParser.ChunkContext)tree, lcontext);

			if (tree is LuaParser.Stat_funcdefContext)
				return new FunctionDefinitionStatement((LuaParser.Stat_funcdefContext)tree, lcontext);

			if (tree is LuaParser.Stat_localfuncdefContext)
				return new FunctionDefinitionStatement((LuaParser.Stat_localfuncdefContext)tree, lcontext);

			if (tree is LuaParser.Stat_functioncallContext)
				return new FunctionCallStatement((LuaParser.Stat_functioncallContext)tree, lcontext);

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
				return new LocalAssignmentStatement((LuaParser.Stat_localassignmentContext)tree, lcontext);

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




		private static Expression CreateFromExpContext(LuaParser.ExpContext tree, ScriptLoadingContext lcontext)
		{
			if (OperatorExpression.IsOperatorExpression(tree))
			{
				return new OperatorExpression(tree, lcontext);
			}
			else if (tree.ChildCount > 1)
			{
				throw new NotImplementedException();
			}
			else
			{
				return CreateExpression(tree.GetChild(0), lcontext);
			}
		}





		public static Expression CreateExpression(IParseTree tree, ScriptLoadingContext lcontext)
		{
			if (tree is LuaParser.VarOrExpContext)
				tree = tree.EnumChilds().Single(t => !(t is Antlr4.Runtime.Tree.TerminalNodeImpl));

			if (tree is Antlr4.Runtime.Tree.TerminalNodeImpl)
			{
				string txt = tree.GetText();
				if (txt == null) return null;
				else if (txt == "nil") return new LiteralExpression(tree, lcontext, RValue.Nil);
				else if (txt == "false") return new LiteralExpression(tree, lcontext, RValue.False);
				else if (txt == "true") return new LiteralExpression(tree, lcontext, RValue.True);
				else return null;
			}

			if (tree is LuaParser.PrefixexpContext)
			{
				var prefix = (LuaParser.PrefixexpContext)tree;
				if (tree.EnumChilds().OfType<LuaParser.NameAndArgsContext>().Any())
					return new FunctionCallChainExpression(prefix, lcontext);
				else
					return CreateExpression(prefix.varOrExp(), lcontext);
			}

			if (tree is LuaParser.VarContext)
				return CreateVariableExpression((LuaParser.VarContext)tree, lcontext);

			if (tree is LuaParser.ExplistContext)
				return new ExprListExpression((LuaParser.ExplistContext)tree, lcontext);

			if (tree is LuaParser.AnonfunctiondefContext)
				return new FunctionDefinitionExpression((LuaParser.AnonfunctiondefContext)tree, lcontext);

			if (tree is LuaParser.ExpContext)
				return CreateFromExpContext((LuaParser.ExpContext)tree, lcontext);

			if (tree is LuaParser.StringContext)
				return new LiteralExpression((LuaParser.StringContext)tree, lcontext);

			if (tree is LuaParser.NumberContext)
				return new LiteralExpression((LuaParser.NumberContext)tree, lcontext);

			if (tree is LuaParser.TableconstructorContext)
				return new TableConstructor((LuaParser.TableconstructorContext)tree, lcontext);

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
					indexExp = new LiteralExpression(suff_NAME, lcontext, new RValue(suff_NAME.GetText()));

				if (nameAndArgs != null)
				{
					varExp = new FunctionCallChainExpression(suffix, lcontext, varExp, nameAndArgs);
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
	}
}
