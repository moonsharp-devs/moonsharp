using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class CollectionsRegisteredTests
	{
		public class RegCollMethods 
		{
			List<int> m_List = new List<int>() { 1, 2, 3 };
			int[] m_Array = new int[3] { 2, 4, 6 };

			public int[] GetArray()
			{
				return m_Array;
			}

			public List<int> GetList()
			{
				return m_List;
			}

			public IEnumerator<int> GetEnumerator()
			{
				return GetList().GetEnumerator();
			}
		}

		void Do(string code, Action<DynValue> asserts)
		{
			Do(code, (d, o) => asserts(d));
		}


		void Do(string code, Action<DynValue, RegCollMethods> asserts)
		{
			UserData.RegisterType<RegCollMethods>();

			try
			{
				UserData.RegisterType<List<int>>();
				UserData.RegisterType<int[]>();

				Script s = new Script();

				var obj = new RegCollMethods();
				s.Globals["o"] = obj;

				DynValue res = s.DoString(code);

				asserts(res, obj);
			}
			catch (ScriptRuntimeException ex)
			{
				Debug.WriteLine(ex.DecoratedMessage);
				throw;
			}
			finally
			{
				UserData.UnregisterType<List<int>>();
				UserData.UnregisterType<int[]>();
				UserData.UnregisterType<RegCollMethods>();
			}
		}





		[Test]
		public void RegColl_IteratorOnList_Auto()
		{
			Do(@"
				local list = o:GetList()

				local x = 0;
				for i in list do 
					x = x + i;
				end
				return x;
			",
			 (r) =>
			 {
				 Assert.AreEqual(DataType.Number, r.Type);
				 Assert.AreEqual(6, r.Number);
			 });
		}


		[Test]
		public void RegColl_IteratorOnList_Manual()
		{
			Do(@"
				function each(obj)
					local e = obj:GetEnumerator()
					return function()
						if e:MoveNext() then
							return e.Current
						end
					end
				end

				local list = o; 

				local x = 0;
				for i in each(list) do 
					x = x + i;
				end
				return x;

			",
			 (r) =>
			 {
				 Assert.AreEqual(DataType.Number, r.Type);
				 Assert.AreEqual(6, r.Number);
			 });
		}

		[Test]
		public void RegColl_IteratorOnList_ChangeElem()
		{
			Do(@"
				local list = o:GetList()

				list[1] = list[2] + list[1];

				local x = 0;
				for i in list do 
					x = x + i;
				end
				return x;
			",
			 (r, o) =>
			 {
				 Assert.AreEqual(DataType.Number, r.Type);
				 Assert.AreEqual(9, r.Number);
				 Assert.AreEqual(1, o.GetList()[0]);
				 Assert.AreEqual(5, o.GetList()[1]);
				 Assert.AreEqual(3, o.GetList()[2]);
			 });
		}


		[Test]
		public void RegColl_IteratorOnArray_Auto()
		{
			Do(@"
				local array = o:GetArray()

				local x = 0;
				for i in array do 
					x = x + i;
				end
				return x;			",
			 (r) =>
			 {
				 Assert.AreEqual(DataType.Number, r.Type);
				 Assert.AreEqual(12, r.Number);
			 });
		}


		[Test]
		public void RegColl_IteratorOnArray_ChangeElem()
		{
			Do(@"
				local array = o:get_array()

				array[1] = array[2] - 1;

				local x = 0;
				for i in array do 
					x = x + i;
				end
				return x;
			",
			 (r, o) =>
			 {
				 Assert.AreEqual(DataType.Number, r.Type);
				 Assert.AreEqual(13, r.Number);
				 Assert.AreEqual(2, o.GetArray()[0]);
				 Assert.AreEqual(5, o.GetArray()[1]);
				 Assert.AreEqual(6, o.GetArray()[2]);
			 });
		}


	}
}
