using System.IO;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests.EndToEnd
{
	[TestFixture]
	public class IoTests
	{
		private string WriteFileTest(string mode, string lua, string existingFileContent = null)
		{
			string filePath = Path.GetTempFileName();

			if (existingFileContent != null)
			{
				File.WriteAllText(filePath, existingFileContent);
			}

			Script script = new Script();
			script.DoString("file = io.open('" + filePath.Replace("'", "\\'") + "' , '" + mode + "')");
			script.DoString(lua);
			DynValue result = script.DoString(@"
				file:close()
				return file
			");

			Assert.AreEqual(DataType.UserData, result.Type);
			Assert.AreEqual("file (closed)", result.UserData.Object.ToString());

			string content = File.ReadAllText(filePath);
			File.Delete(filePath);
			return content;
		}

		[Test]
		public void Read()
		{
			string filePath = Path.GetTempFileName();
			File.WriteAllText(filePath, "Hello, World!");

			Script script = new Script();
			DynValue result = script.DoString(@"
				file = io.open('" + filePath.Replace("'", "\\'") + @"' , 'r')
				content = file:read()
				file:close()
				return content
			");

			File.Delete(filePath);

			Assert.AreEqual(DataType.String, result.Type);
			Assert.AreEqual("Hello, World!", result.String);
		}

		[Test]
		public void WriteExisting()
		{
			string lua = "file:write('Hello, World!')";
			string fileContents = WriteFileTest("w", lua, "Long bit of text that shouldn't be present after writing.");
			Assert.AreEqual("Hello, World!", fileContents);
		}

		[Test]
		public void WriteNew()
		{
			string lua = "file:write('Hello, World!')";
			string fileContents = WriteFileTest("w", lua);
			Assert.AreEqual("Hello, World!", fileContents);
		}

		[Test]
		public void AppendExisting()
		{
			string lua = "file:write(', World!')";
			string fileContents = WriteFileTest("a", lua, "Hello");
			Assert.AreEqual("Hello, World!", fileContents);
		}

		[Test]
		public void AppendNew()
		{
			string lua = "file:write('Hello, World!')";
			string fileContents = WriteFileTest("a", lua);
			Assert.AreEqual("Hello, World!", fileContents);
		}

		[Test]
		public void ReadPlus()
		{
			string lua = @"
				content = file:read()
				file:write(', ' .. content)
			";
			string fileContents = WriteFileTest("r+", lua, "Hello");
			Assert.AreEqual("Hello, Hello", fileContents);
		}

		[Test]
		public void WritePlusExisting()
		{
			string lua = @"
				file:write('Hello')
				file:seek('set', 0)
				content = file:read()
				file:write(', ' .. content)
			";
			string fileContents = WriteFileTest("w+", lua, "Long bit of text that shouldn't be present after writing");
			Assert.AreEqual("Hello, Hello", fileContents);
		}

		[Test]
		public void WritePlusNew()
		{
			string lua = @"
				file:write('Hello')
				file:seek('set', 0)
				content = file:read()
				file:write(', ' .. content)
			";
			string fileContents = WriteFileTest("w+", lua);
			Assert.AreEqual("Hello, Hello", fileContents);
		}

		[Test]
		public void AppendPlusExisting()
		{
			string lua = @"
				file:write(', World!')
				file:seek('set', 0)
				content = file:read()
				file:write(' ' .. content)
			";
			string fileContents = WriteFileTest("a+", lua, "Hello");
			Assert.AreEqual("Hello, World! Hello, World!", fileContents);
		}

		[Test]
		public void AppendPlusNew()
		{
			string lua = @"
				file:write('Hello, World!')
				file:seek('set', 0)
				content = file:read()
				file:write(' ' .. content)
			";
			string fileContents = WriteFileTest("a+", lua);
			Assert.AreEqual("Hello, World! Hello, World!", fileContents);
		}
	}
}
