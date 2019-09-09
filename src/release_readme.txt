MoonSharp [http://www.moonsharp.org]
------------------------------------

This archive contains all the files required to setup MoonSharp on your machine.

Contents:

 - /interpreter    -> The main DLL of the MoonSharp interpreter itself.
                      Use this if you want to just embed the interpreter in your application.

 - /vscodedebugger -> The DLL for the Visual Studio Code debugger facilities (plus the interpreter DLL itself).
                      Use this if you want to embed the intepreter in your application with vscode debugging enabled.

 - /remotedebugger -> The DLL for the remote debugger facilities (plus the interpreter DLL itself).
                      Use this if you want to embed the intepreter in your application with remote debugging enabled.

 - /repl           -> The REPL interpreter. It's not really meant for production as much as to quickly test scripts,
                      or to compile bytecode, or for hardwiring.

 - /unity          -> This contains a unity package you can use in your project. It includes interpreter and vscodedebugger.


Each directory contains C# sources and a project file. Import this in any project and you are ready to go.









