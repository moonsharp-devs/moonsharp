FROM mono:5.18.1.28

# .NET Core

RUN apt-get update \
  && apt-get install -y apt-transport-https gnupg wget \
  && wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
  && mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
  && wget -q https://packages.microsoft.com/config/debian/9/prod.list \
  && mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
  && apt-get update \
  && apt-get install -y dotnet-sdk-2.2

# MoonSharp

WORKDIR /build

COPY ci.sh ci.sh
COPY src/MoonSharp.Interpreter src/MoonSharp.Interpreter/
COPY src/MoonSharp.Interpreter.Tests src/MoonSharp.Interpreter.Tests/
COPY src/MoonSharp.RemoteDebugger src/MoonSharp.RemoteDebugger/
COPY src/MoonSharp.VsCodeDebugger src/MoonSharp.VsCodeDebugger/
COPY src/TestRunners/ConsoleTestRunner src/TestRunners/ConsoleTestRunner/
COPY src/TestRunners/DotNetCoreTestRunner src/TestRunners/DotNetCoreTestRunner/
COPY src/moonsharp_ci.sln src/moonsharp_ci.sln

ENTRYPOINT ["sh", "/build/ci.sh"]

