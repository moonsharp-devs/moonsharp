#if EMBEDTEST || UNITY_5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUnit.Framework
{
	public class TestAttribute : Attribute
	{ }

	public class IgnoreAttribute : Attribute
	{ }

	public class TestFixtureAttribute : Attribute
	{ }

	public class ExpectedExceptionAttribute : Attribute
	{
		public Type ExpectedException { get; set; }

		public ExpectedExceptionAttribute(Type t)
		{
			ExpectedException = t;
		}
	}

	public static class Assert
	{
		public static void AreEqualNum(int expected, double other, string message = null)
		{
			Assert.IsTrue((double)expected == other, message ?? string.Format("{0} was expected, {1} was returned", expected, other));
		}

		public static void AreEqual(object expected, object other, string message = null)
		{
			if (expected is int && other is double) 
				AreEqualNum((int)expected, (double)other, message);
			else if (expected != null)
				Assert.IsTrue(expected.Equals(other), message ?? string.Format("{0} was expected, {1} was returned", expected, other));
			else
				Assert.IsTrue(other == null, message ?? string.Format("null was expected, {0} was returned", other));
		}

		public static void IsTrue(bool condition, string message)
		{
			if (!condition)
				throw new Exception("Test failed : " + message);
		}

		public static void IsFalse(bool condition, string message)
		{
			IsTrue(!condition, message);
		}

		internal static void IsNotNull(object o)
		{
			IsFalse(o == null, "Object is null");
		}

		internal static void IsNotNullOrEmpty(string p)
		{
			IsFalse(string.IsNullOrEmpty(p), "String is null or empty");
		}

		internal static void IsTrue(bool p)
		{
			IsTrue(p, "Value not true");
		}

		internal static void Catch<ET>(Action a)
		{
			try
			{
				a();
			}
			catch(Exception e)
			{
				Assert.IsTrue(e is ET);
			}
		}



		internal static void Fail()
		{
			Assert.IsTrue(false);
		}
	}



}


#endif