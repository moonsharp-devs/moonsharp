rm -R ./MoonSharp/Assets/Tests
rm -R ./MoonSharp/Assets/Plugins/MoonSharp/Interpreter
rm -R ./MoonSharp/Assets/Plugins/MoonSharp/Debugger
mkdir ./MoonSharp/Assets/Tests
mkdir ./MoonSharp/Assets/Plugins/MoonSharp/Interpreter
mkdir ./MoonSharp/Assets/Plugins/MoonSharp/Debugger

rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.VsCodeDebugger/ ./MoonSharp/Assets/Plugins/MoonSharp/Debugger/
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.Interpreter/ ./MoonSharp/Assets/Plugins/MoonSharp/Interpreter/
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.Interpreter.Tests/ ./MoonSharp/Assets/Tests
