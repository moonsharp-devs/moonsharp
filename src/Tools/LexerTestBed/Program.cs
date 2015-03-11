using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexerTestBed
{
	class Program
	{
		static void Main(string[] args)
		{
			(new MoonSharp.Interpreter.Tree.LexerBackDoor()).Test();
		}
	}
}
