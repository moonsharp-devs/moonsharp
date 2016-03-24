using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class MetatableTests
	{
		[Test]
		public void TableIPairsWithMetatable()
		{
			string script = @"    
				test = { 2, 4, 6 }

				meta = { }

				function meta.__ipairs(t)
					local function ripairs_it(t,i)
						i=i-1
						local v=t[i]
						if v==nil then return v end
						return i,v
					end

					return ripairs_it, t, #t+1
				end

				setmetatable(test, meta);

				x = '';

				for i,v in ipairs(test) do
					x = x .. i;
				end

				return x;";

			DynValue res = (new Script()).DoString(script);

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("321", res.String);
		}

		[Test]
		public void TableAddWithMetatable()
		{
			string script = @"    
				v1 = { 'aaaa' }
				v2 = { 'aaaaaa' } 

				meta = { }

				function meta.__add(t1, t2)
					local o1 = #t1[1];
					local o2 = #t2[1];
	
					return o1 * o2;
				end


				setmetatable(v1, meta);


				return(v1 + v2);";

			var S = new Script();
			Table globalCtx = S.Globals;

			globalCtx.RegisterModuleType<TableIteratorsModule>();
			globalCtx.RegisterModuleType<MetaTableModule>();

			DynValue res = S.DoString(script);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(24, res.Number);
		}

		[Test]
		public void MetatableEquality()
		{
			string script = @"    
				t1a = {}
				t1b = {}
				t2  = {}
				mt1 = { __eq = function( o1, o2 ) return 'whee' end }
				mt2 = { __eq = function( o1, o2 ) return 'whee' end }

				setmetatable( t1a, mt1 )
				setmetatable( t1b, mt1 )
				setmetatable( t2,  mt2 )

				return ( t1a == t1b ), ( t1a == t2 ) 
				";

			DynValue res = (new Script()).DoString(script);

			Assert.AreEqual(DataType.Tuple, res.Type);
			Assert.AreEqual(true, res.Tuple[0].Boolean);
			Assert.AreEqual(false, res.Tuple[1].Boolean);

		}

		[Test]
		public void MetatableCall2()
		{
			string script = @"    
					t = { }
					meta = { }

					x = 0;

					function meta.__call(f, y)
						x = 156 * y;
						return x;
					end

					setmetatable(t, meta);

					return t;
				";

			Script S = new Script();

			DynValue tbl = S.DoString(script);
			DynValue res = S.Call(tbl, 3);


			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(468, res.Number);

		}

		[Test]
		public void MetatableCall()
		{
			string script = @"    
					t = { }
					meta = { }

					x = 0;

					function meta.__call(f, y)
						x = 156 * y;
					end

					setmetatable(t, meta);

					t(3);
					return x;
				";

			DynValue res = (new Script()).DoString(script);


			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(468, res.Number);

		}

		[Test]
		public void MetatableIndexAndSetIndexFuncs()
		{
			string script = @"    
					T = { a = 'a', b = 'b', c = 'c' };

					t = { };

					m = { };

					s = '';


					function m.__index(obj, idx)
						return T[idx];
					end

					function m.__newindex(obj, idx, val)
						T[idx] = val;
					end

					setmetatable(t, m);

					s = s .. t.a .. t.b .. t.c;

					t.a = '!';

					s = s .. t.a .. t.b .. t.c;

					return(s);
				";

			DynValue res = (new Script()).DoString(script);


			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("abc!bc", res.String);
		}

		[Test]
		public void MetatableIndexAndSetIndexBounce()
		{
			string script = @"    
					T = { a = 'a', b = 'b', c = 'c' };

					t = { };

					m = { __index = T, __newindex = T };

					s = '';

					setmetatable(t, m);

					s = s .. t.a .. t.b .. t.c;

					t.a = '!';

					s = s .. t.a .. t.b .. t.c;

					return(s);
				";

			DynValue res = (new Script()).DoString(script);


			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("abc!bc", res.String);
		}


		public class MyObject
		{
			public int GetSomething()
			{
				return 10;
			}
		}

		[Test]
		public void MetatableExtensibleObjectSample()
		{
			string code = @"    

				--declare this once for all
				extensibleObjectMeta = {
					__index = function(t, name) local obj = rawget(t, 'wrappedobj'); if (obj) then return obj[name]; end end
				}

				-- create a new wrapped object called myobj, wrapping the object o
				myobj = { wrappedobj = o };
				setmetatable(myobj, extensibleObjectMeta);

				function myobj.extended()
					return 12;	
				end


				return myobj.extended() * myobj.getSomething();
				";

			Script script = new Script();
			UserData.RegisterType<MyObject>();
			script.Globals["o"] = new MyObject();
			
			DynValue res = script.DoString(code);

			Assert.AreEqual(DataType.Number, res.Type);
			Assert.AreEqual(120, res.Number);
		}

		[Test]
		public void IndexSetDoesNotWrackStack()
		{
			string scriptCode = @"

local aClass = {}
setmetatable(aClass, {__newindex = function() end, __index = function() end })

local p = {a = 1, b = 2}
 
for x , v in pairs(p) do
	print (x, v)
	aClass[x] = v
end

";

			Script script = new Script(CoreModules.Basic | CoreModules.Table | CoreModules.TableIterators | CoreModules.Metatables);

			DynValue res = script.DoString(scriptCode);
		}




	}
}
