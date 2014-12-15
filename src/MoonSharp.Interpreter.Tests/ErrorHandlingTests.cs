using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	[TestFixture]
	class ErrorHandlingTests
	{
		[Test]
		public void Errors_PCall_ClrFunction()
		{
			string script = @"
				r, msg = pcall(assert, false, 'catched')
				return r, msg;
								";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(2, res.Tuple.Length);
			Assert.AreEqual(DataType.Boolean, res.Tuple[0].Type);
			Assert.AreEqual(DataType.String, res.Tuple[1].Type);
			Assert.AreEqual(false, res.Tuple[0].Boolean);
		}

		[Test]
		public void Errors_PCall_Multiples()
		{
			string script = @"
function try(fn)
	local x, y = pcall(fn)
	
	if (x) then
		return y
	else
		return '!'
	end
end

function a()
	return try(b) .. 'a';
end

function b()
	return try(c) .. 'b';
end

function c()
	return try(d) .. 'c';
end

function d()
	local t = { } .. 'x'
end


return a()
";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("!cba", res.String);
		}

		[Test]
		public void Errors_TryCatch_Multiples()
		{
			string script = @"
function a()
	return try(b) .. 'a';
end

function b()
	return try(c) .. 'b';
end

function c()
	return try(d) .. 'c';
end

function d()
	local t = { } .. 'x'
end


return a()
";
			Script S = new Script(CoreModules.None);

			S.Globals["try"] = DynValue.NewCallback((c, a) =>
				{
					try
					{
						var v = a[0].Function.Call();
						return v;
					}
					catch(ScriptRuntimeException)
					{
						return DynValue.NewString("!");
					}
				});


			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("!cba", res.String);
		}


	}
}
