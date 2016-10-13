using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;
using MoonSharp.Interpreter.CoreLib;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class DynamicTests
	{
		[Test]
		public void DynamicAccessEval()
		{
			string script = @"
				return dynamic.eval('5+1');		
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6, res.Number);
		}

		[Test]
		public void DynamicAccessPrepare()
		{
			string script = @"
				x = dynamic.prepare('5+1');		
				return dynamic.eval(x);
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6, res.Number);
		}

		[Test]
		public void DynamicAccessScope()
		{
			string script = @"
				a = 3;

				x = dynamic.prepare('a+1');		

				function f()
					a = 5;
					return dynamic.eval(x);
				end

				return f();
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(6, res.Number);
		}

		[Test]
		public void DynamicAccessScopeSecurity()
		{
			string script = @"
				a = 5;

				local x = dynamic.prepare('a');		

				local eval = dynamic.eval;

				local _ENV = { }

				function f()
					return eval(x);
				end

				return f();
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Nil, res.Type);
			//Assert.AreEqual(6, res.Number);
		}

		[Test]
		public void DynamicAccessFromCSharp()
		{
			string code = @"
				t = { ciao = { 'hello' } }
				";

			Script script = new Script();
			script.DoString(code);

			DynValue v = script.CreateDynamicExpression("t.ciao[1] .. ' world'").Evaluate();

			Assert.AreEqual(v.String, "hello world");
		}


	}
}
