#!/bin/bash

# -- DO NOT CHANGE ORDER OTHERWISE WE RISK COPY OVERS...

echo Cleaning...
echo ... Unity
rm -R ./Unity/MoonSharp/Assets/Tests
rm -R ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter
rm -R ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger
mkdir ./Unity/MoonSharp/Assets/Tests
mkdir ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter
mkdir ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger

echo ... .NET Core
rm -R ./MoonSharp.Interpreter/_Projects/MoonSharp.Interpreter.netcore/src
rm -R ./MoonSharp.VsCodeDebugger/_Projects/MoonSharp.VsCodeDebugger.netcore/src
rm -R ./TestRunners/DotNetCoreTestRunner/src
mkdir ./MoonSharp.Interpreter/_Projects/MoonSharp.Interpreter.netcore/src
mkdir ./MoonSharp.VsCodeDebugger/_Projects/MoonSharp.VsCodeDebugger.netcore/src
mkdir ./TestRunners/DotNetCoreTestRunner/src

echo

echo Copying files...

echo ... Unity - interpreter
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.Interpreter/ ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter/

echo ... Unity - vscode debugger...
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.VsCodeDebugger/ ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger/

echo ... Unity - unit tests...
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.Interpreter.Tests/ ./Unity/MoonSharp/Assets/Tests

echo ... Unity - cleaning cruft...
rm -R ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter/_Projects
rm -R ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger/_Projects


echo ... .NET Core - interpreter
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.Interpreter/ ./MoonSharp.Interpreter/_Projects/MoonSharp.Interpreter.netcore/src/

echo ... .NET Core - vscode debugger...
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.VsCodeDebugger/ ./MoonSharp.VsCodeDebugger/_Projects/MoonSharp.VsCodeDebugger.netcore/src

echo ... .NET Core - unit tests...
rsync -a --prune-empty-dirs --exclude 'AssemblyInfo.cs' --include '*/' --include '*.cs' --exclude '*' /git/my/moonsharp/src/MoonSharp.Interpreter.Tests/ ./TestRunners/DotNetCoreTestRunner/src