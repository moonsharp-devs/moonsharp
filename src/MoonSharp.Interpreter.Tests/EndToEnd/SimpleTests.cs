using System;
using System.Linq;
using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;
using MoonSharp.Interpreter.Loaders;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class SimpleTests
	{
		[Test]
		public void EmptyLongComment()
		{
			Script S = new Script(CoreModules.None);
			DynValue res = S.DoString("--[[]]");
		}


		[Test]
		public void EmptyChunk()
		{
			Script S = new Script(CoreModules.None);
			DynValue res = S.DoString("");
		}

		[Test]
		public void CSharpStaticFunctionCallStatement()
		{
			IList<DynValue> args = null;

			string script = "print(\"hello\", \"world\");";

			Script S = new Script();

			S.Globals.Set("print", DynValue.NewCallback(new CallbackFunction((x, a) => 
			{
				args = a.GetArray();
				return DynValue.NewNumber(1234.0); 
			})));

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Void, res.Type);
			Assert.AreEqual(2, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.String, args[1].Type);
			Assert.AreEqual("world", args[1].String);
		}

		[Test]
		public void CSharpStaticFunctionCallRedef()
		{
			IList<DynValue> args = null;

			string script = "local print = print; print(\"hello\", \"world\");";

			var S = new Script();
			S.Globals.Set("print", DynValue.NewCallback(new CallbackFunction((_x, a) => { args = a.GetArray(); return DynValue.NewNumber(1234.0); })));

			DynValue res = S.DoString(script);

			Assert.AreEqual(2, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.String, args[1].Type);
			Assert.AreEqual("world", args[1].String);
			Assert.AreEqual(DataType.Void, res.Type);
		}

		[Test]
		public void CSharpStaticFunctionCall4()
		{
			string script = "return callback()();";

			var callback2 = DynValue.NewCallback(new CallbackFunction((_x, a) => { return DynValue.NewNumber(1234.0); }));
			var callback = DynValue.NewCallback(new CallbackFunction((_x, a) => { return callback2; }));

			var S = new Script();
			S.Globals.Set("callback", callback);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1234.0, res.Number);
		}

		[Test]
		public void CSharpStaticFunctionCall3()
		{
			string script = "return callback();";

			var callback = DynValue.NewCallback(new CallbackFunction((_x, a) => { return DynValue.NewNumber(1234.0); }));

			var S = new Script();
			S.Globals.Set("callback", callback);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1234.0, res.Number);
		}

		[Test]
		public void CSharpStaticFunctionCall2()
		{
			IList<DynValue> args = null;

			string script = "return callback 'hello';";

			var S = new Script();
			S.Globals.Set("callback", DynValue.NewCallback(new CallbackFunction((_x, a) => { args = a.GetArray(); return DynValue.NewNumber(1234.0); })));

			DynValue res = S.DoString(script);

			Assert.AreEqual(1, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1234.0, res.Number);
		}

		[Test]
		public void CSharpStaticFunctionCall()
		{
			IList<DynValue> args = null;

			string script = "return print(\"hello\", \"world\");";

			var S = new Script();
			S.Globals.Set("print", DynValue.NewCallback(new CallbackFunction((_x, a) => { args = a.GetArray(); return DynValue.NewNumber(1234.0); })));

			DynValue res = S.DoString(script);

			Assert.AreEqual(2, args.Count);
			Assert.AreEqual(DataType.String, args[0].Type);
			Assert.AreEqual("hello", args[0].String);
			Assert.AreEqual(DataType.String, args[1].Type);
			Assert.AreEqual("world", args[1].String);
			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1234.0, res.Number);
		}

		[Test]
		//!!! DO NOT REFORMAT THIS METHOD !!!
		public void LongStrings()
		{
			string script = @"    
				x = [[
					ciao
				]];

				y = [=[ [[uh]] ]=];

				z = [===[[==[[=[[[eheh]]=]=]]===]

				return x,y,z";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(3, res.Tuple.Length);
			Assert.AreEqual(DataType.String, res.Tuple[0].Type);
			Assert.AreEqual(DataType.String, res.Tuple[1].Type);
			Assert.AreEqual(DataType.String, res.Tuple[2].Type);
			Assert.AreEqual("\t\t\t\t\tciao\n\t\t\t\t", res.Tuple[0].String);
			Assert.AreEqual(" [[uh]] ", res.Tuple[1].String);
			Assert.AreEqual("[==[[=[[[eheh]]=]=]", res.Tuple[2].String);
		}


		[Test]
		public void UnicodeEscapeLua53Style()
		{
			string script = @"    
				x = 'ciao\u{41}';
				return x;";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("ciaoA", res.String);
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void InvalidEscape()
		{
			string script = @"    
				x = 'ciao\k{41}';
				return x;";

			DynValue res = Script.RunString(script);
		}

		[Test]
		public void KeywordsInStrings()
		{
			string keywrd = "and break do else elseif end false end for function end goto if ::in:: in local nil not [or][[][==][[]] repeat return { then 0 end return; }; then true (x != 5 or == * 3 - 5) x";

			string script = string.Format(@"    
				x = '{0}';
				return x;", keywrd);

			DynValue res = Script.RunString(script);
			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual(keywrd, res.String);
		}



		[Test]
		public void ParserErrorMessage()
		{
			bool caught = false;
			string script = @"    
				return 'It's a wet floor warning saying wheat flour instead. \
				Probably, the cook thought it was funny. \
				He was wrong.'";

			try
			{
				DynValue res = Script.RunString(script);
			}
			catch (SyntaxErrorException ex)
			{
				caught = true;
				Assert.IsNotNullOrEmpty(ex.Message);
			}

			Assert.IsTrue(caught);
		}

		[Test]
		public void StringsWithBackslashLineEndings2()
		{
			string script = @"    
				return 'a\
				b\
				c'";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
		}

		[Test]
		public void StringsWithBackslashLineEndings()
		{
			string script = @"    
				return 'It is a wet floor warning saying wheat flour instead. \
				Probably, the cook thought it was funny. \
				He was wrong.'";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
		}

		[Test]
		public void FunctionCallWrappers()
		{
			string script = @"    
				function boh(x) 
					return 1912 + x;
				end
			";

			Script s = new Script();
			s.DoString(script);

			DynValue res = s.Globals.Get("boh").Function.Call(82);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1994, res.Number);
		}


		[Test]
		public void ReturnSimpleUnop()
		{
			string script = @"return -42";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(-42, res.Number);
		}

		[Test]
		public void ReturnSimple()
		{
			string script = @"return 42";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(42, res.Number);
		}


		[Test]
		public void OperatorSimple()
		{
			string script = @"return 6*7";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(42, res.Number);
		}


		[Test]
		public void SimpleBoolShortCircuit()
		{
			string script = @"    
				x = true or crash();
				y = false and crash();
			";

			Script S = new Script();
			S.Globals.Set("crash", DynValue.NewCallback(new CallbackFunction((_x, a) =>
			{
				throw new Exception("FAIL!");
			})));

			S.DoString(script);
		}

		[Test]
		public void FunctionOrOperator()
		{
			string script = @"    
				loadstring = loadstring or load;

				return loadstring;
			";

			Script S = new Script();
			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.ClrFunction, res.Type);

		}


		[Test]
		public void SelectNegativeIndex()
		{
			string script = @"    
				return select(-1,'a','b','c');
			";

			Script S = new Script();
			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("c", res.String);
		}





		[Test]
		public void BoolConversionAndShortCircuit()
		{
			string script = @"    
				i = 0;

				function f()
					i = i + 1;
					return '!';
				end					
				
				x = false;
				y = true;

				return false or f(), true or f(), false and f(), true and f(), i";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(5, res.Tuple.Length);
			Assert.AreEqual(DataType.String, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Boolean, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Boolean, res.Tuple[2].Type);
			Assert.AreEqual(DataType.String, res.Tuple[3].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[4].Type);
			Assert.AreEqual("!", res.Tuple[0].String);
			Assert.AreEqual(true, res.Tuple[1].Boolean);
			Assert.AreEqual(false, res.Tuple[2].Boolean);
			Assert.AreEqual("!", res.Tuple[3].String);
			Assert.AreEqual(2, res.Tuple[4].Number);
		}
		[Test]
		public void HanoiTowersDontCrash()
		{
			string script = @"
			function move(n, src, dst, via)
				if n > 0 then
					move(n - 1, src, via, dst)
					move(n - 1, via, dst, src)
				end
			end
 
			move(4, 1, 2, 3)
			";

			DynValue res = Script.RunString(script);
		}

		[Test]
		public void Factorial()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					if (n == 0) then
						return 1
					else
						return n*fact(n - 1)
					end
				end
    
				return fact(5)";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}

		[Test]
		public void IfStatmWithScopeCheck()
		{
			string script = @"    
				x = 0

				if (x == 0) then
					local i = 3;
					x = i * 2;
				end
    
				return i, x";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Nil, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[1].Number);
		}

		[Test]
		public void ScopeBlockCheck()
		{
			string script = @"    
				local x = 6;
				
				do
					local i = 33;
				end
		
				return i, x";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Nil, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[1].Number);
		}

		[Test]
		public void ForLoopWithBreak()
		{
			string script = @"    
				x = 0

				for i = 1, 10 do
					x = i
					break;
				end
    
				return x";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}


		[Test]
		public void ForEachLoopWithBreak()
		{
			string script = @"    
				x = 0
				y = 0

				t = { 2, 4, 6, 8, 10, 12 };

				function iter (a, ii)
				  ii = ii + 1
				  local v = a[ii]
				  if v then
					return ii, v
				  end
				end
    
				function ipairslua (a)
				  return iter, a, 0
				end

				for i,j in ipairslua(t) do
					x = x + i
					y = y + j

					if (i >= 3) then
						break
					end
				end
    
				return x, y";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[0].Number);
			Assert.AreEqual(12, res.Tuple[1].Number);
		}


		[Test]
		public void ForEachLoop()
		{
			string script = @"    
				x = 0
				y = 0

				t = { 2, 4, 6, 8, 10, 12 };

				function iter (a, ii)
				  ii = ii + 1
				  local v = a[ii]
				  if v then
					return ii, v
				  end
				end
    
				function ipairslua (a)
				  return iter, a, 0
				end

				for i,j in ipairslua(t) do
					x = x + i
					y = y + j
				end
    
				return x, y";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(21, res.Tuple[0].Number);
			Assert.AreEqual(42, res.Tuple[1].Number);
		}

		[Test]
		public void LengthOperator()
		{
			string script = @"    
				x = 'ciao'
				y = { 1, 2, 3 }
   
				return #x, #y";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(4, res.Tuple[0].Number);
			Assert.AreEqual(3, res.Tuple[1].Number);
		}


		[Test]
		public void ForLoopWithBreakAndScopeCheck()
		{
			string script = @"    
				x = 0

				for i = 1, 10 do
					x = x + i

					if (i == 3) then
						break
					end
				end
    
				return i, x";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Nil, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(6, res.Tuple[1].Number);
		}

		[Test]
		public void FactorialWithOneReturn()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					if (n == 0) then
						return 1
					end
					return n*fact(n - 1)
				end
    
				return fact(5)";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}

		[Test]
		public void VeryBasic()
		{
			string script = @"return 7";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(7, res.Number);
		}

		[Test]
		public void OperatorPrecedence1()
		{
			string script = @"return 1+2*3";

			Script s = new Script(CoreModules.None);
			DynValue res = s.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(7, res.Number);
		}
		[Test]
		public void OperatorPrecedence2()
		{
			string script = @"return 2*3+1";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(7, res.Number);
		}

		[Test]
		public void OperatorAssociativity()
		{
			string script = @"return 2^3^2";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(512, res.Number);
		}

		[Test]
		public void OperatorPrecedence3()
		{
			string script = @"return 5-3-2";
			Script S = new Script(CoreModules.None);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(0, res.Number);
		}

		[Test]
		public void OperatorPrecedence4()
		{
			string script = @"return 3 + -1";
			Script S = new Script(CoreModules.None);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(2, res.Number);
		}

		[Test]
		public void OperatorPrecedence5()
		{
			string script = @"return 3 * -1 + 5 * 3";
			Script S = new Script(CoreModules.None);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(12, res.Number);
		}

		[Test]
		public void OperatorPrecedence6()
		{
			string script = @"return -2^2";
			Script S = new Script(CoreModules.None);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(-4, res.Number);
		}

		[Test]
		public void OperatorPrecedence7()
		{
			string script = @"return -7 / 0.5";
			Script S = new Script(CoreModules.None);

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(-14, res.Number);
		}

		[Test]
		public void OperatorPrecedenceAndAssociativity()
		{
			string script = @"return 5+3*7-2*5+2^3^2";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(528, res.Number);
		}

		[Test]
		public void OperatorParenthesis()
		{
			string script = @"return (5+3)*7-2*5+(2^3)^2";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(110, res.Number);
		}

		[Test]
		public void GlobalVarAssignment()
		{
			string script = @"x = 1; return x;";    

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}
		[Test]
		public void TupleAssignment1()
		{
			string script = @"    
				function y()
					return 2, 3
				end

				function x()
					return 1, y()
				end

				w, x, y, z = 0, x()
    
				return w+x+y+z";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6, res.Number);
		}

		[Test]
		public void IterativeFactorialWithWhile()
		{
			string script = @"    
				function fact (n)
					local result = 1;
					while(n > 0) do
						result = result * n;
						n = n - 1;
					end
					return result;
				end
    
				return fact(5)";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}



		[Test]
		public void IterativeFactorialWithRepeatUntilAndScopeCheck()
		{
			string script = @"    
				function fact (n)
					local result = 1;
					repeat
						local checkscope = 1;
						result = result * n;
						n = n - 1;
					until (n == 0 and checkscope == 1)
					return result;
				end
    
				return fact(5)";

			Script s = new Script(CoreModules.None);
			DynValue res = s.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}

		[Test]

		public void SimpleForLoop()
		{
			string script = @"    
					x = 0
					for i = 1, 3 do
						x = x + i;
					end

					return x;
			";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6.0, res.Number);
		}

		[Test]
		public void SimpleFunc()
		{
			string script = @"    
				function fact (n)
					return 3;
				end
    
				return fact(3)";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		public void IterativeFactorialWithFor()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					x = 1
					for i = n, 1, -1 do
						x = x * i;
					end

					return x;
				end
    
				return fact(5)";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120.0, res.Number);
		}


		[Test]
		public void LocalFunctionsObscureScopeRule()
		{
			string script = @"    
				local function fact()
					return fact;
				end

				return fact();
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Function, res.Type);
		}

		[Test]
		public void FunctionWithStringArg2()
		{
			string script = @"    
				x = 0;

				fact = function(y)
					x = y
				end

				fact 'ciao';

				return x;
				";


			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("ciao", res.String);
		}

		[Test]
		public void FunctionWithStringArg()
		{
			string script = @"    
				x = 0;

				function fact(y)
					x = y
				end

				fact 'ciao';

				return x;
				";


			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("ciao", res.String);

		}

		[Test]
		public void FunctionWithTableArg()
		{
			string script = @"    
				x = 0;

				function fact(y)
					x = y
				end

				fact { 1,2,3 };

				return x;
				";


			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Table, res.Type);

		}


		[Test]
		public void TupleAssignment2()
		{
			string script = @"    
				function boh()
					return 1, 2;
				end

				x,y,z = boh(), boh()

				return x,y,z;
				";


			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[2].Type);
			Assert.AreEqual(1, res.Tuple[0].Number);
			Assert.AreEqual(1, res.Tuple[1].Number);
			Assert.AreEqual(2, res.Tuple[2].Number);
		}
		[Test]
		public void LoopWithReturn()
		{
			string script = @"function Allowed( )
									for i = 1, 20 do
  										return false 
									end
									return true
								end
						Allowed();
								";

			DynValue res = Script.RunString(script);

		}
		[Test]
		public void IfWithLongExpr()
		{
			string script = @"function Allowed( )
									for i = 1, 20 do
									if ( false ) or ( true and true ) or ( 7+i <= 9 and false ) then 
  										return false 
									end
									end		
									return true
								end
						Allowed();
								";

			DynValue res = Script.RunString(script);

		}

		[Test]
		public void IfWithLongExprTbl()
		{
			string script = @"
						t = { {}, {} }
						
						function Allowed( )
									for i = 1, 20 do
									if ( t[1][3] ) or ( i <= 17 and t[1][1] ) or ( 7+i <= 9 and t[1][1] ) then 
  										return false 
									end
									end		
									return true
								end
						Allowed();
								";

			DynValue res = Script.RunString(script);

		}

		[Test]
		public void ExpressionReducesTuples()
		{
			string script = @"
					function x()
						return 1,2
					end

					do return (x()); end
					do return x(); end
								";

			DynValue res = (new Script(CoreModules.None)).DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}


		[Test]
		public void ExpressionReducesTuples2()
		{
			string script = @"
					function x()
						return 3,4
					end

					return 1,x(),x()
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(4, res.Tuple.Length);
		}


		[Test]
		public void ArgsDoNotChange()
		{
			string script = @"
					local a = 1;
					local b = 2;

					function x(c, d)
						c = c + 3;
						d = d + 4;
						return c + d;
					end

					return x(a, b+1), a, b;
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(3, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[2].Type);
			Assert.AreEqual(11, res.Tuple[0].Number);
			Assert.AreEqual(1, res.Tuple[1].Number);
			Assert.AreEqual(2, res.Tuple[2].Number);
		}


		[Test]
		public void VarArgsNoError()
		{
			string script = @"
					function x(...)

					end

					function y(a, ...)

					end

					return 1;
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}

		[Test]
		public void VarArgsSum()
		{
			string script = @"
					function x(...)
						local t = pack(...);
						local sum = 0;

						for i = 1, #t do
							sum = sum + t[i];
						end
	
						return sum;
					end

					return x(1,2,3,4);
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}

		[Test]
		public void VarArgsSum2()
		{
			string script = @"
					function x(m, ...)
						local t = pack(...);
						local sum = 0;

						for i = 1, #t do
							sum = sum + t[i];
						end
	
						return sum * m;
					end

					return x(5,1,2,3,4);
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(50, res.Number);
		}

		[Test]
		public void VarArgsSumTb()
		{
			string script = @"
					function x(...)
						local t = {...};
						local sum = 0;

						for i = 1, #t do
							sum = sum + t[i];
						end
	
						return sum;
					end

					return x(1,2,3,4);
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}

		[Test]
		public void SwapPattern()
		{
			string script = @"
					local n1 = 1
					local n2 = 2
					local n3 = 3
					local n4 = 4
					n1,n2,n3,n4 = n4,n3,n2,n1

					return n1,n2,n3,n4;
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(4, res.Tuple.Length);
			Assert.AreEqual(4, res.Tuple[0].Number);
			Assert.AreEqual(3, res.Tuple[1].Number);
			Assert.AreEqual(2, res.Tuple[2].Number);
			Assert.AreEqual(1, res.Tuple[3].Number);
		}

		[Test]
		public void SwapPatternGlobal()
		{
			string script = @"
					n1 = 1
					n2 = 2
					n3 = 3
					n4 = 4
					n1,n2,n3,n4 = n4,n3,n2,n1

					return n1,n2,n3,n4;
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(4, res.Tuple.Length);
			Assert.AreEqual(4, res.Tuple[0].Number);
			Assert.AreEqual(3, res.Tuple[1].Number);
			Assert.AreEqual(2, res.Tuple[2].Number);
			Assert.AreEqual(1, res.Tuple[3].Number);
		}

		[Test]
		public void EnvTestSuite()
		{
			string script = @"
				local RES = { }

				RES.T1 = (_ENV == _G) 

				a = 1

				local function f(t)
				  local _ENV = t 

				  RES.T2 = (getmetatable == nil) 
  
				  a = 2 -- create a new entry in t, doesn't touch the original 'a' global
				  b = 3 -- create a new entry in t
				end

				local t = {}
				f(t)

				RES.T3 = a;
				RES.T4 = b;
				RES.T5 = t.a;
				RES.T6 = t.b;

				return RES;
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Table, res.Type);

			Table T = res.Table;

			Assert.AreEqual(DataType.Boolean, T.Get("T1").Type, "T1-Type");
			Assert.AreEqual(true, T.Get("T1").Boolean, "T1-Val");

			Assert.AreEqual(DataType.Boolean, T.Get("T2").Type, "T2-Type");
			Assert.AreEqual(true, T.Get("T2").Boolean, "T2-Val");

			Assert.AreEqual(DataType.Number, T.Get("T3").Type, "T3-Type");
			Assert.AreEqual(1, T.Get("T3").Number, "T3-Val");

			Assert.AreEqual(DataType.Nil, T.Get("T4").Type, "T4");

			Assert.AreEqual(DataType.Number, T.Get("T5").Type, "T5-Type");
			Assert.AreEqual(2, T.Get("T5").Number, "T5-Val");

			Assert.AreEqual(DataType.Number, T.Get("T6").Type, "T6-Type");
			Assert.AreEqual(3, T.Get("T6").Number, "T6-Val");
		}

		[Test]
		public void TupleToOperator()
		{
			string script = @"    
				function x()
					return 3, 'xx';
				end

				return x() == 3;	
			";

			Script S = new Script(CoreModules.None);
			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Boolean, res.Type);
			Assert.AreEqual(true, res.Boolean);
		}


		[Test]
		public void LiteralExpands()
		{
			string script = @"    
				x = 'a\65\66\67z';
				return x;	
			";

			Script S = new Script(CoreModules.None);
			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("aABCz", res.String);
		}

		[Test]
		public void HomonymArguments()
		{
			string script = @"    
				function test(_,value,_) return _; end

				return test(1, 2, 3);	
			";

			Script S = new Script(CoreModules.None);
			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		public void VarArgsSumMainChunk()
		{
			string script = @"
					local t = pack(...);
					local sum = 0;

					for i = 1, #t do
						sum = sum + t[i];
					end
	
					return sum;
								";

			DynValue fn = new Script().LoadString(script);

			DynValue res = fn.Function.Call(1, 2, 3, 4);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(10, res.Number);
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void VarArgsInNoVarArgsReturnsError()
		{
			string script = @"
					function x()
						local t = {...};
						local sum = 0;

						for i = 1, #t do
							sum = sum + t[i];
						end
	
						return sum;
					end

					return x(1,2,3,4);
								";

			DynValue res = Script.RunString(script);
		}

		[Test]
		public void HexFloats_1()
		{
			string script = "return 0x0.1E";
			DynValue result = Script.RunString(script);
			Assert.AreEqual((double)0x1E / (double)0x100, result.Number);
		}

		[Test]
		public void HexFloats_2()
		{
			string script = "return 0xA23p-4";
			DynValue result = Script.RunString(script);
			Assert.AreEqual((double)0xA23 / 16.0, result.Number);
		}

		[Test]
		public void HexFloats_3()
		{
			string script = "return 0X1.921FB54442D18P+1";
			DynValue result = Script.RunString(script);
			Assert.AreEqual((1 + (double)0x921FB54442D18 / (double)0x10000000000000) * 2, result.Number);
		}

		[Test]
		public void Simple_Delegate_Interop_1()
		{
			int a = 3;
			var script = new Script();
			script.Globals["action"] = new Action(() => a = 5);
			script.DoString("action()");
			Assert.AreEqual(5, a);
		}

		[Test]
		public void Simple_Delegate_Interop_2()
		{
			var oldPolicy = UserData.RegistrationPolicy;

			try
			{
				UserData.RegistrationPolicy = Interop.InteropRegistrationPolicy.Automatic;

				int a = 3;
				var script = new Script();
				script.Globals["action"] = new Action(() => a = 5);
				script.DoString("action()");
				Assert.AreEqual(5, a);
			}
			finally
			{
				UserData.RegistrationPolicy = oldPolicy;
			}
		}

		[Test]
		public void MissingArgsDefaultToNil()
		{
			Script S = new Script(CoreModules.None);
			DynValue res = S.DoString(@"
				function test(a)
					return a;
				end

				test();
				");
		}

		[Test]
		public void ParsingTest()
		{
			Script S = new Script(CoreModules.None);
			DynValue res = S.LoadString(@"
				t = {'a', 'b', 'c', ['d'] = 'f', ['e'] = 5, [65] = true, [true] = false}
				function myFunc()
				  return 'one', 'two'
				end

				print('Table Test 1:')
				for k,v in pairs(t) do
				  print(tostring(k) .. ' / ' .. tostring(v))
				end
				print('Table Test 2:')
				for X,X in pairs(t) do
				  print(tostring(X) .. ' / ' .. tostring(X))
				end
				print('Function Test 1:')
				v1,v2 = myFunc()
				print(v1)
				print(v2)
				print('Function Test 2:')
				v,v = myFunc()
				print(v)
				print(v)
				");			
		}


//		[Test]
//		public void TestModulesLoadingWithoutCrash()
//		{
//#if !PCL
//			var basePath = AppDomain.CurrentDomain.BaseDirectory;
//			var scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts\\test");
//			Script script = new Script();

//			((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths = new[]
//			{
//				System.IO.Path.Combine(basePath, "scripts\\test\\test.lua"),
//			};
//			var obj = script.LoadFile(System.IO.Path.Combine(scriptPath, "test.lua"));
//			obj.Function.Call();
//#endif
//		}

		[Test]
        	public void NumericConversionFailsIfOutOfBounds()
        	{
            		Script S = new Script();

            		S.Globals["my_function_takes_byte"] = (Action<byte>)(p => { });

            		try
            		{
                		S.DoString("my_function_takes_byte(2010191) -- a huge number that is definitely not a byte");

                		Assert.Fail(); // ScriptRuntimeException should have been thrown, if it doesn't Assert.Fail should execute
            		}
            		catch (ScriptRuntimeException e)
            		{
                		//Assert.Pass(e.DecoratedMessage);
            		}
        	}


	}
}
