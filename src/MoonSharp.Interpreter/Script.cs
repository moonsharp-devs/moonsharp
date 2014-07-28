using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.Interpreter.Tree;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// This class implements a Moon# scripting session. Multiple Script objects can coexist in the same program but cannot share
	/// data among themselves except through the _UNIVERSE table. Data accessed through the _UNIVERSE table is thread safe, so
	/// Script objects can execute in different threads with no issue, as long as all their callbacks are aware and compatible.
	/// </summary>
	public class Script
	{
		Processor m_MainRoutine = null;
		List<Processor> m_Coroutines = new List<Processor>();
		ByteCode m_ByteCode;
		List<SourceCode> m_Sources = new List<SourceCode>();
		Table m_GlobalTable;
		IDebugger m_Debugger;
		IScriptLoader m_ScriptLoader = DefaultScriptLoader;
		Table[] m_TypeMetatables = new Table[(int)DataType.MaxMetaTypes];

		static Script()
		{
			DefaultScriptLoader = new ClassicLuaScriptLoader();
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Script"/> class.
		/// </summary>
		public Script()
			: this(CoreModules.Preset_Default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Script"/> class.
		/// </summary>
		/// <param name="coreModules">The core modules to be pre-registered in the default global table.</param>
		public Script(CoreModules coreModules)
		{
			DebugPrint = s => { Console.WriteLine(s); };
			m_ByteCode = new ByteCode();
			m_GlobalTable = new Table(this).RegisterCoreModules(coreModules);
			m_MainRoutine = new Processor(this, m_GlobalTable, m_ByteCode);
			ReseedRandomGenerator(DateTime.Now.Millisecond);
		}


		/// <summary>
		/// Gets or sets the script loader to use. A script loader wraps all code loading from files, so that access
		/// to the filesystem can be completely overridden.
		/// </summary>
		/// <value>
		/// The current script loader.
		/// </value>
		public IScriptLoader ScriptLoader
		{
			get { return m_ScriptLoader; }
			set { m_ScriptLoader = value; }
		}

		/// <summary>
		/// Gets or sets the script loader which will be used as the value of the
		/// ScriptLoader property for all newly created scripts.
		/// </summary>
		public static IScriptLoader DefaultScriptLoader { get; set; }

		/// <summary>
		/// Gets the default global table for this script. Unless a different table is intentionally passed (or setfenv has been used)
		/// execution uses this table.
		/// </summary>
		public Table Globals
		{
			get { return m_GlobalTable; }
		}

		/// <summary>
		/// Loads a string containing a Lua/Moon# function.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="globalTable">The global table to bind to this chunk.</param>
		/// <returns>A DynValue containing a function which will execute the loaded code.</returns>
		public DynValue LoadFunction(string code, Table globalTable = null, string funcFriendlyName = null)
		{
			string chunkName = string.Format("<string:{0:X4}>", m_Sources.Count);

			SourceCode source = new SourceCode(funcFriendlyName ?? chunkName, code);

			m_Sources.Add(source);

			int address = Loader.LoadFunctionFromICharStream(new AntlrInputStream(code),
				m_ByteCode,
				funcFriendlyName ?? chunkName,
				m_Sources.Count - 1,
				globalTable ?? m_GlobalTable);

			if (m_Debugger != null)
				m_Debugger.SetSourceCode(m_ByteCode, null);

			return MakeClosure(address);
		}



		/// <summary>
		/// Loads a string containing a Lua/Moon# script.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="globalTable">The global table to bind to this chunk.</param>
		/// <returns>A DynValue containing a function which will execute the loaded code.</returns>
		public DynValue LoadString(string code, Table globalTable = null, string codeFriendlyName = null)
		{
			string chunkName = string.Format("<string:{0:X4}>", m_Sources.Count);

			SourceCode source = new SourceCode(codeFriendlyName ?? chunkName, code);

			m_Sources.Add(source);

			int address = Loader.LoadChunkFromICharStream(new AntlrInputStream(code),
				m_ByteCode,
				codeFriendlyName ?? chunkName,
				m_Sources.Count - 1,
				globalTable ?? m_GlobalTable);

			if (m_Debugger != null)
				m_Debugger.SetSourceCode(m_ByteCode, null);

			return MakeClosure(address);
		}

		/// <summary>
		/// Loads a string containing a Lua/Moon# script.
		/// </summary>
		/// <param name="filename">The code.</param>
		/// <param name="globalContext">The global table to bind to this chunk.</param>
		/// <returns>A DynValue containing a function which will execute the loaded code.</returns>
		public DynValue LoadFile(string filename, Table globalContext = null)
		{
			filename = m_ScriptLoader.ResolveFileName(filename, globalContext ?? m_GlobalTable);

			return LoadResolvedFile(filename, globalContext);
		}

		private DynValue LoadResolvedFile(string filename, Table globalContext)
		{
			if (m_ScriptLoader.HasCustomFileLoading())
			{
				return LoadString(m_ScriptLoader.LoadFile(filename, globalContext ?? m_GlobalTable), globalContext, filename);
			}
			else
			{
				return LoadString(File.ReadAllText(filename), globalContext, filename);
			}
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
		public DynValue DoString(string code, Table globalContext = null)
		{
			DynValue func = LoadString(code, globalContext);
			return Call(func);
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
		public DynValue DoFile(string filename, Table globalContext = null)
		{
			DynValue func = LoadFile(filename, globalContext);
			return Call(func);
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
		/// <returns></returns>
		private DynValue MakeClosure(int address)
		{
			Closure c = new Closure(this, address,
				new SymbolRef[0],
				new DynValue[0]);

			return DynValue.NewClosure(c);
		}

		/// <summary>
		/// Calls the specified function.
		/// </summary>
		/// <param name="coroutine">The coroutine on which to execute, or -1 to use the first-free (or current) coroutine.</param>
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns></returns>
		public DynValue Call(DynValue function, params DynValue[] args)
		{
			return m_MainRoutine.Call(function, args);
		}

		/// <summary>
		/// Gets the main chunk function.
		/// </summary>
		/// <returns>A DynValue containing a function which executes the first chunk that has been loaded.</returns>
		public DynValue GetMainChunk()
		{
			return MakeClosure(0);
		}

		/// <summary>
		/// Attaches a debugger.
		/// </summary>
		/// <param name="debugger">The debugger object.</param>
		/// </param>
		public void AttachDebugger(IDebugger debugger)
		{
			m_Debugger = debugger;
			m_MainRoutine.AttachDebugger(debugger);

			foreach (var C in m_Coroutines)
				C.AttachDebugger(debugger);

			m_Debugger.SetSourceCode(m_ByteCode, null);
		}


		/// <summary>
		/// Loads a module as per the "require" Lua function. http://www.lua.org/pil/8.1.html
		/// </summary>
		/// <param name="modname">The module name</param>
		/// <returns></returns>
		public DynValue RequireModule(string modname, Table globalContext = null)
		{
			Table globals = globalContext ?? m_GlobalTable;
			string filename = m_ScriptLoader.ResolveModuleName(modname, globals);

			if (filename == null)
				throw new ScriptRuntimeException("module '{0}' not found", modname);

			DynValue func = LoadResolvedFile(filename, globalContext);
			return func;
		}


		/// <summary>
		/// Gets the random generator associated with this Script
		/// </summary>
		public Random RandomGenerator { get; private set; }

		/// <summary>
		/// Reseeds the random generator.
		/// </summary>
		/// <param name="seed">The seed.</param>
		public void ReseedRandomGenerator(double seed)
		{
			RandomGenerator = new Random();
		}


		/// <summary>
		/// Gets a type metatable.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public Table GetTypeMetatable(DataType type)
		{
			int t = (int)type;

			if (t >= 0 && t < m_TypeMetatables.Length)
				return m_TypeMetatables[t];

			return null;
		}

		/// <summary>
		/// Sets a type metatable.
		/// </summary>
		/// <param name="type">The type. Must be Nil, Boolean, Number, String or Function</param>
		/// <param name="metatable">The metatable.</param>
		/// <exception cref="System.ArgumentException">Specified type not supported :  + type.ToString()</exception>
		public void SetTypeMetatable(DataType type, Table metatable)
		{
			int t = (int)type;

			if (t >= 0 && t < m_TypeMetatables.Length)
				m_TypeMetatables[t] = metatable;
			else
				throw new ArgumentException("Specified type not supported : " + type.ToString());
		}

		/// <summary>
		/// Gets or sets the debug print handler
		/// </summary>
		public Action<string> DebugPrint { get; set; }

	}
}
