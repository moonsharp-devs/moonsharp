using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MoonSharp.Interpreter")]
[assembly: AssemblyDescription("An interpreter for the Lua language")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("http://www.moonsharp.org")]
[assembly: AssemblyProduct("MoonSharp.Interpreter")]
[assembly: AssemblyCopyright("Copyright © 2014-2015, Marco Mastropaolo")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c971e5a8-dbec-4408-8046-86e4fdd1b2e3")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision 
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(MoonSharp.Interpreter.Script.VERSION)]
[assembly: AssemblyFileVersion(MoonSharp.Interpreter.Script.VERSION)]

// Give 
[assembly: InternalsVisibleTo("MoonSharp.Interpreter.Tests")]
[assembly: InternalsVisibleTo("MoonSharp.Interpreter.Tests.net40-client")]
[assembly: InternalsVisibleTo("MoonSharp.Interpreter.Tests.portable40")]
[assembly: InternalsVisibleTo("MoonSharp.Interpreter.Tests.net35-client")]











