using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Constants of well known "symbols" in the Moon# grammar
	/// </summary>
	public static class WellKnownSymbols
	{
		/// <summary>
		/// The variadic argument symbol ("...")
		/// </summary>
		public const string VARARGS = "...";

		/// <summary>
		/// The environment symbol ("_ENV")
		/// </summary>
		public const string ENV = "_ENV";
	}
}
