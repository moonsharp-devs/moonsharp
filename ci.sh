#!/usr/bin/env sh

set -e

export DOTNET_ROLL_FORWARD=Major

# Build
dotnet build -c Release src/TestRunners/ConsoleTestRunner/ConsoleTestRunner.csproj
dotnet build -c Release src/TestRunners/DotNetCoreTestRunner/DotNetCoreTestRunner.csproj

# Mono Tests
cd src/TestRunners/ConsoleTestRunner/bin/Release/net45
mono ConsoleTestRunner.exe

# Mono ahead of time compilation tests
#  - chmod 777 aotregen.sh
#  - chmod 777 aottest.sh
#  - sudo ./aotregen.sh
#  - sudo ./aottest.sh

# .NET Core Tests
cd ../../../../DotNetCoreTestRunner/bin/Release/net8.0
dotnet DotNetCoreTestRunner.dll /unit
