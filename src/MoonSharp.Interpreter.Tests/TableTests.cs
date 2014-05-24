using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	[TestFixture]
	public class TableTests
	{
		[Test][Ignore("VM Transition")]
		public void TableAccessAndCtor()
		{
			string script = @"
						a = { 55, 2, 3, aurevoir=6, [false] = 7 }
						
						a[1] = 1;
						a.ciao = 4;
						a['hello'] = 5;

						return a[1], a[2], a[3], a['ciao'], a.hello, a.aurevoir, a[false]";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(7, res.Tuple.Length);
			Assert.AreEqual(DataType.Number, res.Tuple[0].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[1].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[2].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[3].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[4].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[5].Type);
			Assert.AreEqual(DataType.Number, res.Tuple[6].Type);
			Assert.AreEqual(1, res.Tuple[0].Number);
			Assert.AreEqual(2, res.Tuple[1].Number);
			Assert.AreEqual(3, res.Tuple[2].Number);
			Assert.AreEqual(4, res.Tuple[3].Number);
			Assert.AreEqual(5, res.Tuple[4].Number);
			Assert.AreEqual(6, res.Tuple[5].Number);
			Assert.AreEqual(7, res.Tuple[6].Number);
		}

		[Test][Ignore("VM Transition")]
		public void TableMethod1()
		{
			string script = @"
						x = 0
	
						a = 
						{ 
							value = 1912,

							val = function(self, num)
								x = self.value + num
							end
						}
						
						a.val(a, 82);

						return x";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1994, res.Number);
		}

		[Test][Ignore("VM Transition")]
		public void TableMethod2()
		{
			string script = @"
						x = 0
	
						a = 
						{ 
							value = 1912,

							val = function(self, num)
								x = self.value + num
							end
						}
						
						a:val(82);

						return x";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1994, res.Number);
		}

		[Test][Ignore("VM Transition")]
		public void TableMethod3()
		{
			string script = @"
						x = 0
	
						a = 
						{ 
							value = 1912,
						}

						function a.val(self, num)
							x = self.value + num
						end
						
						a:val(82);

						return x";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1994, res.Number);
		}


		[Test][Ignore("VM Transition")]
		public void TableMethod4()
		{
			string script = @"
						x = 0
	
						a = 
						{ 
							value = 1912,
						}

						function a:val(num)
							x = self.value + num
						end
						
						a:val(82);

						return x";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1994, res.Number);
		}

		[Test][Ignore("VM Transition")]
		public void TableMethod5()
		{
			string script = @"
						x = 0

						a = 
						{ 
							value = 1912,
						}

						b = { tb = a };
						c = { tb = b };

						function c.tb.tb:val(num)
							x = self.value + num
						end
						
						a:val(82);

						return x";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(1994, res.Number);
		}


		[Test][Ignore("VM Transition")]
		public void TableMethod6()
		{
			string script = @"
						do
						  local a = {x=0}
						  function a:add (x) self.x, a.y = self.x+x, 20; return self end
						  return (a:add(10):add(20):add(30).x == 60 and a.y == 20)
						end";

			RValue res = MoonSharpInterpreter.LoadFromString(script, null).Execute();

			Assert.AreEqual(DataType.Boolean, res.Type);
			Assert.AreEqual(true, res.TestAsBoolean());
		}

	}
}
