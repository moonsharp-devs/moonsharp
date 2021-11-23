using System;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class TimeZoneTests
	{
		[Test]
		public void LocalTime1()
		{
			Script S = new Script();
			try
			{
				S.Options.LocalTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
			}
			catch (TimeZoneNotFoundException)
			{
				S.Options.LocalTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
			}

			DynValue res = S.DoString("return os.date(\"%Y-%m-%d %H:%M:%S\", 0)");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1970-01-01 01:00:00", res.String);
		}

		[Test]
		public void LocalTime2()
		{
			Script S = new Script();
			try
			{
				S.Options.LocalTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
			}
			catch (TimeZoneNotFoundException)
			{
				S.Options.LocalTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
			}

			DynValue res = S.DoString("return os.date(\"!%Y-%m-%d %H:%M:%S\", 0)");

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1970-01-01 00:00:00", res.String);
		}

		[Test]
		public void LocalTime3()
		{
			Script S = new Script();

			TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(new DateTime(1970, 1, 1));

			DynValue res = S.DoString(string.Format("return os.date(\"%Y-%m-%d %H:%M:%S\", -{0})", offset.TotalSeconds));

			Assert.AreEqual(DataType.String, res.Type);
			Assert.AreEqual("1970-01-01 00:00:00", res.String);
		}
	}
}
