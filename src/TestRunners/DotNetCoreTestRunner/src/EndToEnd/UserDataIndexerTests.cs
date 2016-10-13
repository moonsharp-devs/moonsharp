using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class UserDataIndexerTests
	{
		public class IndexerTestClass
		{
			Dictionary<int, int> mymap = new Dictionary<int, int>();

			public int this[int idx]
			{
				get { return mymap[idx]; }
				set { mymap[idx] = value; }
			}

			public int this[int idx1, int idx2, int idx3]
			{
				get { int idx = (idx1 + idx2) * idx3; return mymap[idx]; }
				set { int idx = (idx1 + idx2) * idx3; mymap[idx] = value; }
			}
		}

		private void IndexerTest(string code, int expected)
		{
			Script S = new Script();

			IndexerTestClass obj = new IndexerTestClass();

			UserData.RegisterType<IndexerTestClass>();

			S.Globals.Set("o", UserData.Create(obj));

			DynValue v = S.DoString(code);

			Assert.AreEqual(DataType.Number, v.Type);
			Assert.AreEqual(expected, v.Number);
		}

		[Test]
		public void Interop_SingleSetterOnly()
		{
			string script = @"o[1] = 1; return 13";
			IndexerTest(script, 13);
		}


		[Test]
		public void Interop_SingleIndexerGetSet()
		{
			string script = @"o[5] = 19; return o[5];";
			IndexerTest(script, 19);
		}

		[Test]
		public void Interop_MultiIndexerGetSet()
		{
			string script = @"o[1,2,3] = 47; return o[1,2,3];";
			IndexerTest(script, 47);
		}

		[Test]
		public void Interop_MultiIndexerMetatableGetSet()
		{
			string script = @"
				m = { 
					__index = o,
					__newindex = o
				}

				t = { }

				setmetatable(t, m);

				t[10,11,12] = 1234; return t[10,11,12];";
			IndexerTest(script, 1234);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_MultiIndexerMetamethodGetSet()
		{
			string script = @"
				m = { 
					__index = function() end,
					__newindex = function() end
				}

				t = { }

				setmetatable(t, m);

				t[10,11,12] = 1234; return t[10,11,12];";
			IndexerTest(script, 1234);
		}





		[Test]
		public void Interop_MixedIndexerGetSet()
		{
			string script = @"o[3,2,3] = 119; return o[15];";
			IndexerTest(script, 119);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_ExpListIndexingCompilesButNotRun1()
		{
			string script = @"    
				x = { 99, 98, 97, 96 }				
				return x[2,3];
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(98, res.Number);
		}

		[Test]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public void Interop_ExpListIndexingCompilesButNotRun2()
		{
			string script = @"    
				x = { 99, 98, 97, 96 }				
				x[2,3] = 5;
				";

			DynValue res = Script.RunString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(98, res.Number);
		}



	}
}
