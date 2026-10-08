using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class GotoTests
	{
		[Test]
		public void Goto_Simple_Fwd()
		{
			string script = @"
				function test()
					x = 3
					goto skip	
					x = x + 2;
					::skip::
					return x;
				end				

				return test();
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		public void Goto_Simple_Bwd()
		{
			string script = @"
				function test()
					x = 5;
	
					::jump::
					if (x == 3) then return x; end
					
					x = 3
					goto jump

					x = 4
					return x;
				end				

				return test();
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void Goto_UndefinedLabel()
		{
			string script = @"
				goto there
				";

			Script.RunString(script);
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void Goto_DoubleDefinedLabel()
		{
			string script = @"
				::label::
				::label::
				";

			Script.RunString(script);
		}

		[Test]
		public void Goto_RedefinedLabel()
		{
			string script = @"
				::label::
				do
					::label::
				end
				";

			Script.RunString(script);
		}

		[Test]
		public void Goto_RedefinedLabel_Goto()
		{
			string script = @"
				::label::
				do
					goto label
					do return 5 end
					::label::
					return 3
				end
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void Goto_UndefinedLabel_2()
		{
			string script = @"
				goto label
				do
					do return 5 end
					::label::
					return 3
				end
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		[ExpectedException(typeof(SyntaxErrorException))]
		public void Goto_VarInScope()
		{
			string script = @"
				goto f
				local x
				::f::
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}


		[Test]
		public void Goto_JumpOutOfBlocks()
		{
			string script = @"
				local u = 4

				do
					local x = 5
	
					do
						local y = 6
		
						do
							local z = 7
						end
		
						goto out
					end
				end

				do return 5 end

				::out::

				return 3
			";

			DynValue res = Script.RunString(script);
			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(3, res.Number);
		}

		[Test]
		public void Goto_LabelInEmptyScope_PreservesOuterLocals()
		{
			string script = @"
				local outer = 1
				if true then
					-- no locals here
					if true then
						local inner
					end
					::label::
				end
				return outer
				";

			DynValue res = Script.RunString(script);
			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1, res.Number);
		}

		[Test]
		public void Goto_JumpOutOfScopes()
		{
			string script = @"
				local u = 4

				do
					local x = 5
					do
						local y = 6
						do
							goto out
							local z = 7
						end
		
					end
				end

				::out::

				do 
					local a
					local b = 55

					if (a == nil) then
						b = b + 12
					end

					return b
				end

			";

			DynValue res = Script.RunString(script);
			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(67, res.Number);
		}

		[Test]
		public void Goto_OutOfLoopToLabelInIfBlock_PreservesEnclosingLocals()
		{
			// https://github.com/moonsharp-devs/moonsharp/issues/309
			string script = @"
				local a = 7

				if true then
					for x = 1, 10 do
						goto label1
					end
					::label1::
				end

				return a;
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(7, res.Number);
		}

		[Test]
		public void Label_AfterNumericForInIfBlock_PreservesGlobalsTable()
		{
			// No goto is needed to trigger this: the label alone is enough. 'tostring' is a
			// global, so this fails if the chunk's _ENV local slot has been cleared.
			string script = @"
				if true then
					for x = 1, 10 do end
					::label1::
				end

				return tostring(42);
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("42", res.String);
		}

		[Test]
		public void Label_AfterGenericForInIfBlock_PreservesGlobalsTable()
		{
			string script = @"
				if true then
					for k, v in pairs({ 1, 2 }) do end
					::label1::
				end

				return tostring(42);
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("42", res.String);
		}
	}
}
