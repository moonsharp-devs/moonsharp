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
                      
                      
Each directory contains, where applyable, subdirectories for different .NET framework targets:


- net35 : 
This is build targeting .NET 3.5 Client Framework. 
Use this if you are building an app targeting .NET 3.5 or later, Mono 2.x (or later), Xamarin or Unity 3D.

- net40 : 
This is build targeting .NET 4.0 Client Framework. 
Use this if you are building an app targeting .NET 4.0 or later, Mono 3.x or Xamarin.

- portable_net40 : 
This is a Portable Class Library targeting .NET 4.0, Silverlight 5, Xamarin Android, Xamarin iOS, Windows Store 8, Windows Phone 8.1 
Use this if you target these platforms. Note that some functionality (involving file system access or the debuggers) is not available 
in this build due to limitations of PCLs.
You also have to use this library if you target WSA/WP8 apps in Unity3D. Refer to this guide: http://docs.unity3d.com/Manual/windowsstore-plugins.html

- netcore : 
This is a build targeting .NET Core.

- sources
This contains just the C# sources, with no project files. Import this in any project and you are ready to go. 
Stripped sources are available only for the interpreter and vscode debugger. For the other parts, see on github. 
Symbols might need to be defined to have it build correctly. Check the sources (you're on your own on this, sorry).


 


 
 


