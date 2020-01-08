using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;
using NUnit.Framework;


namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class JsonSerializationTests
	{
		void AssertTableValues(Table t)
		{
			Assert.AreEqual(DataType.Number, t.Get("aNumber").Type);
			Assert.AreEqual(1, t.Get("aNumber").Number);

			Assert.AreEqual(DataType.String, t.Get("aString").Type);
			Assert.AreEqual("2", t.Get("aString").String);

			Assert.AreEqual(DataType.Table, t.Get("anObject").Type);
			Assert.AreEqual(DataType.Table, t.Get("anArray").Type);

			Table o = t.Get("anObject").Table;

			Assert.AreEqual(DataType.Number, o.Get("aNumber").Type);
			Assert.AreEqual(3, o.Get("aNumber").Number);

			Assert.AreEqual(DataType.String, o.Get("aString").Type);
			Assert.AreEqual("4", o.Get("aString").String);

			Table a = t.Get("anArray").Table;

			//				'anArray' : [ 5, '6', true, null, { 'aNumber' : 7, 'aString' : '8' } ]

			Assert.AreEqual(DataType.Number, a.Get(1).Type);
			Assert.AreEqual(5, a.Get(1).Number);

			Assert.AreEqual(DataType.String, a.Get(2).Type);
			Assert.AreEqual("6", a.Get(2).String);

			Assert.AreEqual(DataType.Boolean, a.Get(3).Type);
			Assert.IsTrue(a.Get(3).Boolean);

			Assert.AreEqual(DataType.Boolean, a.Get(3).Type);
			Assert.IsTrue(a.Get(3).Boolean);

			Assert.AreEqual(DataType.UserData, a.Get(4).Type);
			Assert.IsTrue(JsonNull.IsJsonNull(a.Get(4)));

			Assert.AreEqual(DataType.Table, a.Get(5).Type);
			Table s = a.Get(5).Table;

			Assert.AreEqual(DataType.Number, s.Get("aNumber").Type);
			Assert.AreEqual(7, s.Get("aNumber").Number);

			Assert.AreEqual(DataType.String, s.Get("aString").Type);
			Assert.AreEqual("8", s.Get("aString").String);

			Assert.AreEqual(DataType.Number, t.Get("aNegativeNumber").Type);
			Assert.AreEqual(-9, t.Get("aNegativeNumber").Number);
		}


		[Test]
		public void JsonDeserialization()
		{
			string json = @"{
				'aNumber' : 1,
				'aString' : '2',
				'anObject' : { 'aNumber' : 3, 'aString' : '4' },
				'anArray' : [ 5, '6', true, null, { 'aNumber' : 7, 'aString' : '8' } ],
				'aNegativeNumber' : -9
				}
			".Replace('\'', '\"');

			Table t = JsonTableConverter.JsonToTable(json);
			AssertTableValues(t);
		}

		[Test]
		public void JsonSerialization()
		{
			string json = @"{
				'aNumber' : 1,
				'aString' : '2',
				'anObject' : { 'aNumber' : 3, 'aString' : '4' },
				'anArray' : [ 5, '6', true, null, { 'aNumber' : 7, 'aString' : '8' } ],
				'aNegativeNumber' : -9
				}
			".Replace('\'', '\"');

			Table t1 = JsonTableConverter.JsonToTable(json);

			string json2 = JsonTableConverter.TableToJson(t1);

			Table t = JsonTableConverter.JsonToTable(json2);

			AssertTableValues(t);
		}


		[Test]
		public void JsonObjectSerialization()
		{
			object o = new
			{
				aNumber = 1,
				aString = "2",
				anObject = new
				{
					aNumber = 3,
					aString = "4"
				},
				anArray = new object[]
				{
					5,
					"6",
					true,
					null,
					new
					{
						aNumber = 7,
						aString = "8"
					}
				},
				aNegativeNumber = -9
			};


			string json = JsonTableConverter.ObjectToJson(o);

			Table t = JsonTableConverter.JsonToTable(json);

			AssertTableValues(t);
		}


	}
}
