rem I know this works only on my machine. Sue me.

md c:\temp\moonsharp_release
md c:\temp\moonsharp_release\tests
md c:\temp\moonsharp_release\repl
md c:\temp\moonsharp_release\debugger
md c:\temp\moonsharp_release\library

robocopy /E C:\git\moonsharp\src\MoonSharpTests\bin\Release c:\temp\moonsharp_release\tests
robocopy /E C:\git\moonsharp\src\MoonSharp\bin\Release c:\temp\moonsharp_release\repl
robocopy /E C:\git\moonsharp\src\MoonSharp.Debugger\bin\Release c:\temp\moonsharp_release\debugger
robocopy /E C:\git\moonsharp\src\MoonSharp.Interpreter\bin\Release c:\temp\moonsharp_release\library



