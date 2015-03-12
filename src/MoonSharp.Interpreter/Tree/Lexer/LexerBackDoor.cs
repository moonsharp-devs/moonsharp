using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Tree
{
	public class LexerBackDoor
	{
		public void Test()
		{
			string code = File.ReadAllText(@"c:\temp\test.lua");
			List<string> output = new List<string>();

			Lexer lexer = new Lexer(code);

			try
			{
				while (true)
				{
					Token tkn = lexer.Current();
					lexer.Next();
					output.Add(tkn.ToString());
					if (tkn.Type == TokenType.Eof)
						break;
				}
			}
			catch (Exception ex)
			{
				output.Add(ex.Message);
			}

			File.WriteAllLines(@"c:\temp\test.lex", output.ToArray());
		}



	}
}
