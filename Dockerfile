FROM ubuntu:24.04

# .NET Core

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
  && apt-get install -y --no-install-recommends ca-certificates gnupg wget \
  && wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
  && dpkg -i /tmp/packages-microsoft-prod.deb \
  && rm -f /tmp/packages-microsoft-prod.deb \
  && apt-get update \
  && apt-get install -y --no-install-recommends dotnet-sdk-8.0 \
  && rm -rf /var/lib/apt/lists/*

# Unity Mono

ARG UNITY_MONO_URL=https://github.com/Benjamin-Dobell/unity-mono/releases/download/2026-02-24.3/unity-mono-linux-2026-02-24.3.tar.gz
ARG UNITY_MONO_SHA256=3cecef994c10eb4eeae95fd34052decac4d62bb45b241e83b0ae1bc0e3ca0008

ADD ${UNITY_MONO_URL} /tmp/unity-mono.tar.gz

RUN echo "${UNITY_MONO_SHA256}  /tmp/unity-mono.tar.gz" | sha256sum -c - \
  && tar --keep-directory-symlink -xzf /tmp/unity-mono.tar.gz -C / \
  && rm -f /tmp/unity-mono.tar.gz

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
