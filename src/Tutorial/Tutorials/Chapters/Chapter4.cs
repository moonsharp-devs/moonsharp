using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter04
	{
		#region TableTest1

		private static List<int> GetNumberList()
		{
			List<int> lst = new List<int>();

			for (int i = 1; i <= 10; i++)
				lst.Add(i);

			return lst;
		}

		[Tutorial]
		public static double TableTest1()
		{
			string scriptCode = @"    
				total = 0;

				tbl = getNumbers()
        
				for _, i in ipairs(tbl) do
					total = total + i;
				end

				return total;
			";

			Script script = new Script();

			script.Globals["getNumbers"] = (Func<List<int>>)GetNumberList;

			DynValue res = script.DoString(scriptCode);

			return res.Number;
		}

		#endregion


		#region TableTest2

		private static Table GetNumberTable(Script script)
		{
			Table tbl = new Table(script);

			for (int i = 1; i <= 10; i++)
				tbl[i] = i;

			return tbl;
		}

		[Tutorial]
		public static double TableTest2()
		{
			string scriptCode = @"    
				total = 0;

				tbl = getNumbers()
        
				for _, i in ipairs(tbl) do
					total = total + i;
				end

				return total;
			";

			Script script = new Script();

			script.Globals["getNumbers"] = (Func<Script, Table>)(GetNumberTable);

			DynValue res = script.DoString(scriptCode);

			return res.Number;
		}

		#endregion


		#region TableTestReverse

		[Tutorial]
		public static double TableTestReverse()
		{
			string scriptCode = @"    
				return dosum { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
			";

			Script script = new Script();

			script.Globals["dosum"] = (Func<List<int>, int>)(l => l.Sum());

			DynValue res = script.DoString(scriptCode);

			return res.Number;
		}

		#endregion


		#region TableTestReverseSafer

		[Tutorial]
		public static double TableTestReverseSafer()
		{
			string scriptCode = @"    
				return dosum { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
			";

			Script script = new Script();

			script.Globals["dosum"] = (Func<List<object>, int>)(l => l.OfType<int>().Sum());

			DynValue res = script.DoString(scriptCode);

			return res.Number;
		}

		#endregion


		#region TableTestReverseWithTable

		static double Sum(Table t)
		{
			var nums = from v in t.Values
					   where v.Type == DataType.Number
					   select v.Number;

			return nums.Sum();
		}


		[Tutorial]
		public static double TableTestReverseWithTable()
		{
			string scriptCode = @"    
					return dosum { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
				";

			Script script = new Script();

			script.Globals["dosum"] = (Func<Table, double>)Sum;

			DynValue res = script.DoString(scriptCode);

			return res.Number;
		}

		#endregion


	}
}
