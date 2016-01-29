using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MoonSharp.Interpreter.Serialization;
using System.IO;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[SetUpFixture]
	public class DumpTableGenerator
	{
		[SetUp]
		public void RunBeforeAnyTests()
		{
			File.WriteAllText(@"c:\temp\testdump.lua", "RunBeforeAnyTests");
		}

		[TearDown]
		public void RunAfterAnyTests()
		{
			Table dump = UserData.GetDescriptionOfRegisteredTypes(true);

			string str = dump.Serialize();

			File.WriteAllText(@"c:\temp\testdump.lua", str);
		}
	}
}