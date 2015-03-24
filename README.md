MoonSharp
=========
http://www.moonsharp.org

A complete Lua solution written entirely in C# for the .NET, Mono and Unity3D platforms.

Includes an interpreter, debugger and will soon have code analysis facilities.

It is 99% compatible with Lua 5.2 (with the only unsupported feature being weak tables support) and includes some nifty additions, plus a tight integration with .NET code. Highlights on differences between MoonSharp and standard Lua, see http://www.moonsharp.org/moonluadifferences.html

Please see http://www.moonsharp.org for downloads, infos, tutorials, etc.


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








