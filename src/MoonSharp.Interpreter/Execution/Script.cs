using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Execution
{
	/// <summary>
	/// This class implements a Moon# scripting session. Multiple Script objects can coexist in the same program but cannot share
	/// data among themselves except through the _UNIVERSE table. Data accessed through the _UNIVERSE table is thread safe, so
	/// Script objects can execute in different threads with no issue, as long as all their callbacks are aware and compatible.
	/// </summary>
	public class Script
	{
		List<Processor> m_Coroutines = new List<Processor>();
		ByteCode m_ByteCode;
		Table m_GlobalTable;
		IDebugger m_Debugger;

		/// <summary>
		/// Gets the default global table for this script. Unless a different table is intentionally passed (or setfenv has been used)
		/// execution uses this table.
		/// </summary>
		public Table Globals
		{
			get { return m_GlobalTable; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Script"/> class.
		/// </summary>
		public Script()
			: this(ModuleRegister.RegisterCoreModules(new Table(), CoreModules.Preset_Default))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Script"/> class.
		/// </summary>
		/// <param name="coreModules">The core modules to be pre-registered in the default global table.</param>
		public Script(CoreModules coreModules)
			: this(ModuleRegister.RegisterCoreModules(new Table(), coreModules))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Script"/> class.
		/// </summary>
		/// <param name="globalTable">The default global table.</param>
		public Script(Table globalTable)
		{
			m_ByteCode = new ByteCode();
			m_GlobalTable = globalTable;
			m_Coroutines.Add(new Processor(this, m_GlobalTable, m_ByteCode));
		}


		/// <summary>
		/// Loads a string containing a Lua/Moon# script.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="globalTable">The global table to bind to this chunk.</param>
		/// <returns>A DynValue containing a function which will execute the loaded code.</returns>
		public DynValue LoadString(string code, Table globalTable = null)
		{
			int address = ChunkStatement.LoadFromICharStream(new AntlrInputStream(code), 
				string.Format("<string:{0:X8}>", code.GetHashCode()),
				m_ByteCode);

			return MakeClosure(address, globalTable);
		}

		/// <summary>
		/// Loads a string containing a Lua/Moon# script.
		/// </summary>
		/// <param name="filename">The code.</param>
		/// <param name="globalContext">The global table to bind to this chunk.</param>
		/// <returns>A DynValue containing a function which will execute the loaded code.</returns>
		public DynValue LoadFile(string filename, Table globalContext = null)
		{
			int address = ChunkStatement.LoadFromICharStream(new AntlrFileStream(filename), filename, m_ByteCode);
			return MakeClosure(address, globalContext);
		}

		/// <summary>
		/// Loads and executes a string containing a Lua/Moon# script.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="globalContext">The global context.</param>
		/// <param name="coroutine">The coroutine index, or -1 to use the first free (or current) coroutine index.</param>
		/// <returns>
		/// A DynValue containing the result of the processing of the loaded chunk.
		/// </returns>
		public DynValue DoString(string code, Table globalContext = null, int coroutine = -1)
		{
			coroutine = FixCoroutineIndex(coroutine);

			DynValue func = LoadString(code, globalContext);
			return Call(coroutine, func);
		}

		/// <summary>
		/// Loads and executes a file containing a Lua/Moon# script.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="globalContext">The global context.</param>
		/// <param name="coroutine">The coroutine index, or -1 to use the first free (or current) coroutine index.</param>
		/// <returns>
		/// A DynValue containing the result of the processing of the loaded chunk.
		/// </returns>
		public DynValue DoFile(string filename, Table globalContext = null, int coroutine = -1)
		{
			coroutine = FixCoroutineIndex(coroutine);

			DynValue func = LoadFile(filename, globalContext);
			return Call(coroutine, func);
		}


		/// <summary>
		/// Runs the specified file with all possible defaults for quick experimenting.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// A DynValue containing the result of the processing of the executed script.
		public static DynValue RunFile(string filename)
		{
			Script S = new Script();
			return S.DoFile(filename);
		}

		/// <summary>
		/// Runs the specified code with all possible defaults for quick experimenting.
		/// </summary>
		/// <param name="code">The Lua/Moon# code.</param>
		/// A DynValue containing the result of the processing of the executed script.
		public static DynValue RunString(string code)
		{
			Script S = new Script();
			return S.DoString(code);
		}

		/// <summary>
		/// Creates a closure from a bytecode address.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		private DynValue MakeClosure(int address, Table globalContext)
		{
			Closure c = new Closure(address, new SymbolRef[0], new DynValue[0]);
			c.GlobalEnv = globalContext;
			return DynValue.NewClosure(c);
		}
		/// <summary>
		/// Fixes the index of a coroutine translating a -1 parameter.
		/// </summary>
		/// <param name="coroutine">The coroutine.</param>
		private int FixCoroutineIndex(int coroutine)
		{
			return (coroutine >= 0) ? coroutine : 0;
		}

		/// <summary>
		/// Calls the specified function.
		/// </summary>
		/// <param name="coroutine">The coroutine on which to execute, or -1 to use the first-free (or current) coroutine.</param>
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns></returns>
		public DynValue Call(int coroutine, DynValue function, params DynValue[] args)
		{
			return m_Coroutines[coroutine].Call(function, args);
		}

		/// <summary>
		/// Gets the main chunk function.
		/// </summary>
		/// <param name="globalContext">The global context.</param>
		/// <returns>A DynValue containing a function which executes the first chunk that has been loaded.</returns>
		public DynValue GetMainChunk(Table globalContext = null)
		{
			return MakeClosure(0, globalContext);
		}

		/// <summary>
		/// Attaches a debugger.
		/// </summary>
		/// <param name="debugger">The debugger object.</param>
		/// <param name="coroutine">
		/// The coroutine to which the debugger attaches, or -1 to attach it to all coroutines. 
		/// If -1 is specified, the debugger is also automatically attached to new coroutines.
		/// </param>
		public void AttachDebugger(IDebugger debugger, int coroutine = -1)
		{
			if (coroutine < 0)
			{
				m_Debugger = debugger;
				foreach (var C in m_Coroutines)
					C.AttachDebugger(debugger);
			}
			else
			{
				m_Coroutines[coroutine].AttachDebugger(debugger);
			}

			m_Debugger.SetSourceCode(m_ByteCode, null);
		}

	}
}
