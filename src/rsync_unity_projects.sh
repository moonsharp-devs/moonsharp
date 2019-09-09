#!/bin/bash

BASE_DIR="$(dirname $0)"

# -- DO NOT CHANGE ORDER OTHERWISE WE RISK COPY OVERS...

echo Cleaning...
echo ... Unity
rm -R ./Unity/MoonSharp/Assets/Tests
rm -R ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter
rm -R ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger
mkdir ./Unity/MoonSharp/Assets/Tests
mkdir ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter
mkdir ./Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger

echo

echo Copying files...

echo ... Unity - interpreter
rsync -a --prune-empty-dirs --exclude 'obj/' --exclude "*.csproj" --include '*/' --include '*.cs' --exclude '*' "$BASE_DIR/MoonSharp.Interpreter/" "$BASE_DIR/Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter/"

echo ... Unity - vscode debugger...
rsync -a --prune-empty-dirs --exclude 'obj/' --exclude "*.csproj" --include '*/' --include '*.cs' --exclude '*' "$BASE_DIR/MoonSharp.VsCodeDebugger/" "$BASE_DIR/Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger/"

echo ... Unity - unit tests...
rsync -a --prune-empty-dirs --exclude 'obj/' --exclude "*.csproj" --include '*/' --include '*.cs' --exclude '*' "$BASE_DIR/MoonSharp.Interpreter.Tests/" "$BASE_DIR/Unity/MoonSharp/Assets/Tests/"
