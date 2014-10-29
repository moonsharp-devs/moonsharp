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
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.Interpreter.Tree;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// This class implements a Moon# scripting session. Multiple Script objects can coexist in the same program but cannot share
	/// data among themselves unless some mechanism is put in place.
	/// </summary>
	public class Script
	{
		Processor m_MainProcessor = null;
		ByteCode m_ByteCode;
		List<SourceCode> m_Sources = new List<SourceCode>();
		Table m_GlobalTable;
		IDebugger m_Debugger;
		IScriptLoader m_ScriptLoader = DefaultScriptLoader;
		Table[] m_TypeMetatables = new Table[(int)LuaTypeExtensions.MaxMetaTypes];


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
			PerformanceStats = new PerformanceStatistics();

			DebugPrint = s => { Console.WriteLine(s); };
			m_ByteCode = new ByteCode();
			m_GlobalTable = new Table(this).RegisterCoreModules(coreModules);
			m_MainProcessor = new Processor(this, m_GlobalTable, m_ByteCode);
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
		/// Gets access to performance statistics.
		/// </summary>
		public PerformanceStatistics PerformanceStats { get; private set; }

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
		/// <param name="funcFriendlyName">Name of the function used to report errors, etc.</param>
		/// <returns>
		/// A DynValue containing a function which will execute the loaded code.
		/// </returns>
		public DynValue LoadFunction(string code, Table globalTable = null, string funcFriendlyName = null)
		{
			string chunkName = string.Format("libfunc_{0}", funcFriendlyName ?? m_Sources.Count.ToString());

			SourceCode source = new SourceCode(chunkName, code, m_Sources.Count, this);

			int address = Loader_Antlr.LoadFunction(this, source, m_ByteCode, globalTable ?? m_GlobalTable);

			m_Sources.Add(source);

			SignalSourceCodeChange(source);
			SignalByteCodeChange();
		
			return MakeClosure(address);
		}

		private void SignalByteCodeChange()
		{
			if (m_Debugger != null)
			{
				m_Debugger.SetByteCode(m_ByteCode.Code.Select(s => s.ToString()).ToArray());
			}
		}

		private void SignalSourceCodeChange(SourceCode source)
		{
			if (m_Debugger != null)
			{
				m_Debugger.SetSourceCode(source);
			}
		}


		/// <summary>
		/// Loads a string containing a Lua/Moon# script.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="globalTable">The global table to bind to this chunk.</param>
		/// <param name="codeFriendlyName">Name of the code - used to report errors, etc.</param>
		/// <returns>
		/// A DynValue containing a function which will execute the loaded code.
		/// </returns>
		public DynValue LoadString(string code, Table globalTable = null, string codeFriendlyName = null)
		{
			string chunkName = string.Format("{0}", codeFriendlyName ?? "chunk_" + m_Sources.Count.ToString());

			SourceCode source = new SourceCode(codeFriendlyName ?? chunkName, code, m_Sources.Count, this);

			int address = Loader_Antlr.LoadChunk(this,
				source,
				m_ByteCode,
				globalTable ?? m_GlobalTable);

			m_Sources.Add(source);

			SignalSourceCodeChange(source);
			SignalByteCodeChange();

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
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <returns>
		/// The return value(s) of the function call.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
		public DynValue Call(DynValue function)
		{
			if (function.Type != DataType.Function)
				throw new ArgumentException("function is not of DataType.Function");

			return m_MainProcessor.Call(function, new DynValue[0]);
		}

		/// <summary>
		/// Calls the specified function.
		/// </summary>
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>
		/// The return value(s) of the function call.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
		public DynValue Call(DynValue function, params DynValue[] args)
		{
			if (function.Type != DataType.Function)
				throw new ArgumentException("function is not of DataType.Function");

			return m_MainProcessor.Call(function, args);
		}

		/// <summary>
		/// Calls the specified function.
		/// </summary>
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>
		/// The return value(s) of the function call.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
		public DynValue Call(DynValue function, params object[] args)
		{
			if (function.Type != DataType.Function)
				throw new ArgumentException("function is not of DataType.Function");

			DynValue[] dargs = new DynValue[args.Length];

			for (int i = 0; i < dargs.Length; i++)
				dargs[i] = DynValue.FromObject(this, args[i]);

			return m_MainProcessor.Call(function, dargs);
		}

		/// <summary>
		/// Calls the specified function.
		/// </summary>
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
		public DynValue Call(object function)
		{
			return Call(DynValue.FromObject(this, function));
		}

		/// <summary>
		/// Calls the specified function.
		/// </summary>
		/// <param name="function">The Lua/Moon# function to be called - callbacks are not supported.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
		public DynValue Call(object function, params object[] args)
		{
			return Call(DynValue.FromObject(this, function), args);
		}

		/// <summary>
		/// Creates a coroutine pointing at the specified function.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns>
		/// The coroutine handle.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function or DataType.ClrFunction</exception>
		public DynValue CreateCoroutine(DynValue function)
		{
			if (function.Type == DataType.Function)
				return m_MainProcessor.Coroutine_Create(function.Function);
			else if (function.Type == DataType.ClrFunction)
				return DynValue.NewCoroutine(new Coroutine(function.Callback));
			else
				throw new ArgumentException("function is not of DataType.Function or DataType.ClrFunction");
		}

		/// <summary>
		/// Creates a coroutine pointing at the specified function.
		/// </summary>
		/// <param name="function">The function.</param>
		/// <returns>
		/// The coroutine handle.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function or DataType.ClrFunction</exception>
		public DynValue CreateCoroutine(object function)
		{
			return CreateCoroutine(DynValue.FromObject(this, function));
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
		public void AttachDebugger(IDebugger debugger)
		{
			m_Debugger = debugger;
			m_MainProcessor.AttachDebugger(debugger);

			foreach (SourceCode src in m_Sources)
				SignalSourceCodeChange(src);

			SignalByteCodeChange();
		}

		/// <summary>
		/// Gets the source code.
		/// </summary>
		/// <param name="sourceCodeID">The source code identifier.</param>
		/// <returns></returns>
		public SourceCode GetSourceCode(int sourceCodeID)
		{
			return m_Sources[sourceCodeID];
		}


		/// <summary>
		/// Gets the source code count.
		/// </summary>
		/// <value>
		/// The source code count.
		/// </value>
		public int SourceCodeCount
		{
			get { return m_Sources.Count; }
		}



		/// <summary>
		/// Loads a module as per the "require" Lua function. http://www.lua.org/pil/8.1.html
		/// </summary>
		/// <param name="modname">The module name</param>
		/// <param name="globalContext">The global context.</param>
		/// <returns></returns>
		/// <exception cref="ScriptRuntimeException">Raised if module is not found</exception>
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


		/// <summary>
		/// Warms up the parser/lexer structures so that Moon# operations start faster.
		/// </summary>
		public static void WarmUp()
		{
			Script s = new Script(CoreModules.Basic);
			s.LoadString("return 1;");
		}





	}
}
