#!/usr/bin/env sh

set -e

# Build
msbuild /t:Restore src/moonsharp_ci.sln
msbuild /p:Configuration=Release src/moonsharp_ci.sln

# Mono Tests
cd src/TestRunners/ConsoleTestRunner/bin/Release/net35
mono ConsoleTestRunner.exe

# Mono ahead of time compilation tests
#  - chmod 777 aotregen.sh
#  - chmod 777 aottest.sh
#  - sudo ./aotregen.sh
#  - sudo ./aottest.sh

# .NET Core Tests
cd ../../../../DotNetCoreTestRunner/bin/Release/netcoreapp2.0
dotnet DotNetCoreTestRunner.dll /unit

