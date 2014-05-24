using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MoonSharp.Interpreter.Execution
{
	public enum Operator
	{
		Or,
		And,
		Less, Greater, LessOrEqual, GreaterOrEqual, NotEqual, Equal,
		StrConcat,
		Add, Sub,
		Mul, Div, Mod,
		Not, Size, Neg,
		Power
	}



}
