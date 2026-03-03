MoonSharp       [![CI](../../actions/workflows/ci.yml/badge.svg)](../../actions/workflows/ci.yml) [![Build Status](https://img.shields.io/nuget/v/MoonSharp.svg)](https://www.nuget.org/packages/MoonSharp/)
=========
http://www.moonsharp.org   



A complete Lua solution written entirely in C# for the .NET, Mono, Xamarin and Unity3D platforms.

Features:
* 99% compatible with Lua 5.2 (with the only unsupported feature being weak tables support)
* Support for metalua style anonymous functions (lambda-style)
* Easy to use API
* **Debugger** support via Debug Adapter Protocol e.g. Visual Studio Code
* Runs on .NET 4.5, .NET Platform (formerly Core), Mono, Xamarin and Unity
* Runs on Ahead-of-time platforms like iOS
* Runs on IL2CPP converted code
* No external dependencies, implemented in as few targets as possible
* Easy and performant interop with CLR objects, with runtime code generation where supported
* Interop with methods, extension methods, overloads, fields, properties and indexers supported
* Support for the complete Lua standard library with very few exceptions (mostly located on the 'debug' module) and a few extensions (in the string library, mostly)
* Async method support
* Supports dumping/loading bytecode for obfuscation and quicker parsing at runtime
* An embedded JSON parser (with no dependencies) to convert between JSON and Lua tables
* Easy opt-out of Lua standard library modules to sandbox what scripts can access
* Easy to use error handling (script errors are exceptions)
* Support for coroutines, including invocation of coroutines as C# iterators 
* REPL interpreter, plus facilities to easily implement your own REPL in few lines of code
* Complete XML help, and walkthroughs on http://www.moonsharp.org

For highlights on differences between MoonSharp and standard Lua, see http://www.moonsharp.org/moonluadifferences.html

Please see http://www.moonsharp.org for downloads, infos, tutorials, etc.

## Unity Package (UPM)

### Build package locally

```bash
tools/upm/stage-local-package.sh 3.0.0-local
cd .upm-staging/org.moonsharp.moonsharp
npm pack
```

This produces a tarball like:

`org.moonsharp.moonsharp-3.0.0-local.tgz`

### Install in Unity

Install from version branch:

1. In your Unity project's `Packages/manifest.json`, add:
   `"org.moonsharp.moonsharp": "https://github.com/moonsharp-devs/moonsharp.git?path=/interpreter#upm/v3.0"`
2. If you just want to pin to a major version (3 instead 3.0), use branches like:
   `upm/v3`
3. The VSCode debugger is a separate package and can be added with:
   `"org.moonsharp.debugger.vscode": "https://github.com/moonsharp-devs/moonsharp.git?path=/debugger/vscode#upm/v3.0"`

<blockquote>
<p>[!NOTE]
Beta branches are available with names like `upm/beta/v3.0`
</p></blockquote>

**License**

The program and libraries are released under a 3-clause BSD license - see the license section.

Parts of the string library are based on the KopiLua project (https://github.com/NLua/KopiLua).
Debugger icons are from the Eclipse project (https://www.eclipse.org/).


**Usage**

Use of the library is easy as:

```C#
double MoonSharpFactorial()
{
	string script = @"    
		-- defines a factorial function
		function fact (n)
			if (n == 0) then
				return 1
			else
				return n*fact(n - 1)
			end
		end

	return fact(5)";

	DynValue res = Script.RunString(script);
	return res.Number;
}
```

For more in-depth tutorials, samples, etc. please refer to http://www.moonsharp.org/getting_started.html
