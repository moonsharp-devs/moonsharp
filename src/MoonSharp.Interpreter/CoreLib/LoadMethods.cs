using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class LoadMethods
	{
		//dofile ([filename])
		//--------------------------------------------------------------------------------------------------------------
		//Opens the named file and executes its contents as a Lua chunk. When called without arguments, 
		//dofile executes the contents of the standard input (stdin). Returns all values returned by the chunk. 
		//In case of errors, dofile propagates the error to its caller (that is, dofile does not run in protected mode). 
		[MoonSharpMethod]
		public static DynValue dofile(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			Script S = executionContext.GetOwnerScript();
			DynValue v = args.AsType(0, "dofile", DataType.String, false);

			DynValue fn = S.LoadFile(v.String);

			return DynValue.NewTailCallReq(fn); // tail call to dofile
		}

		//require (modname)
		//----------------------------------------------------------------------------------------------------------------
		//Loads the given module. The function starts by looking into the package.loaded table to determine whether 
		//modname is already loaded. If it is, then require returns the value stored at package.loaded[modname]. 
		//Otherwise, it tries to find a loader for the module.
		//
		//To find a loader, require is guided by the package.loaders array. By changing this array, we can change 
		//how require looks for a module. The following explanation is based on the default configuration for package.loaders.
		//
		//First require queries package.preload[modname]. If it has a value, this value (which should be a function) 
		//is the loader. Otherwise require searches for a Lua loader using the path stored in package.path. 
		//If that also fails, it searches for a C loader using the path stored in package.cpath. If that also fails, 
		//it tries an all-in-one loader (see package.loaders).
		//
		//Once a loader is found, require calls the loader with a single argument, modname. If the loader returns any value, 
		//require assigns the returned value to package.loaded[modname]. If the loader returns no value and has not assigned 
		//any value to package.loaded[modname], then require assigns true to this entry. In any case, require returns the 
		//final value of package.loaded[modname].
		//
		//If there is any error loading or running the module, or if it cannot find any loader for the module, then require 
		//signals an error. 
		[MoonSharpMethod]
		public static DynValue __require_clr_impl(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			Script S = executionContext.GetOwnerScript();
			DynValue v = args.AsType(0, "__require_clr_impl", DataType.String, false);

			DynValue fn = S.RequireModule(v.String);

			return fn; // tail call to dofile
		}


		[MoonSharpMethod]
		public const string require = @"
			function(modulename)
				if (package == nil) then package = { }; end
				if (package.loaded == nil) then package.loaded = { }; end

				local m = package.loaded[modulename];

				if (m ~= nil) then
					return m;
				end

				local func = __require_clr_impl(modulename);

				local res = func();

				package.loaded[modulename] = res;

				return res;
			end
		";



	}
}
