rem I know this works only on my machine. Sue me.

md c:\temp\moonsharp_release
del /Q c:\temp\moonsharp_release\*.*
md c:\temp\moonsharp_release\tests
del /Q c:\temp\moonsharp_release\tests\*.*
md c:\temp\moonsharp_release\repl
del /Q c:\temp\moonsharp_release\repl\*.*
md c:\temp\moonsharp_release\debugger
del /Q c:\temp\moonsharp_release\debugger\*.*
md c:\temp\moonsharp_release\help
del /Q c:\temp\moonsharp_release\help\*.*
md c:\temp\moonsharp_release\library
del /Q c:\temp\moonsharp_release\library\*.*

robocopy /E C:\git\moonsharp\src\MoonSharpTests\bin\Release c:\temp\moonsharp_release\tests
robocopy /E C:\git\moonsharp\src\MoonSharp\bin\Release c:\temp\moonsharp_release\repl
robocopy /E C:\git\moonsharp\src\MoonSharp.Debugger\bin\Release c:\temp\moonsharp_release\debugger
robocopy /E C:\git\moonsharp\src\MoonSharp.Interpreter\bin\Release c:\temp\moonsharp_release\library

move "C:\git\moonsharp\src\MoonSharp.Documentation\Help\MoonSharp.Reference.Documentation.chm" c:\temp\moonsharp_release\help

robocopy /E C:\git\moonsharp\src\MoonSharp.Interpreter\bin\Release c:\temp\moonsharp_release\library

robocopy /E C:\git\moonsharp\src\MoonSharp.Documentation\Help\fti C:\git\moonsharp_org\reference\fti
robocopy /E C:\git\moonsharp\src\MoonSharp.Documentation\Help\html C:\git\moonsharp_org\reference\html
robocopy /E C:\git\moonsharp\src\MoonSharp.Documentation\Help\icons C:\git\moonsharp_org\reference\icons
robocopy /E C:\git\moonsharp\src\MoonSharp.Documentation\Help\scripts C:\git\moonsharp_org\reference\scripts
robocopy /E C:\git\moonsharp\src\MoonSharp.Documentation\Help\styles C:\git\moonsharp_org\reference\styles
robocopy /E C:\git\moonsharp\src\MoonSharp.Documentation\Help\toc C:\git\moonsharp_org\reference\toc

copy /Y index.html C:\git\moonsharp_org\reference
copy /Y search.html C:\git\moonsharp_org\reference
copy /Y WebKI.xml C:\git\moonsharp_org\reference
copy /Y WebTOC.xml C:\git\moonsharp_org\reference

