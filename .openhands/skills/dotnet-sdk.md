---
name: dotnet-sdk
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
- dotnet
- dotnet build
- csproj
- msbuild
- .NET SDK
- nuget
---

# Building this repository

**The sandbox has no .NET SDK.** Do not run `which dotnet` to find out, and do
not test whether the download host is reachable. Install it directly:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir /tmp/dotnet-sdk
export PATH=/tmp/dotnet-sdk:$PATH DOTNET_NOLOGO=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
```

The install takes two to four minutes. Run it in the foreground and wait. Do not
background it with `nohup` and then poll a log file — that costs more turns than
simply waiting.

Export `PATH` in the **same** command as the build. Each terminal call starts a
fresh shell, so an export from an earlier step is gone:

```bash
export PATH=/tmp/dotnet-sdk:$PATH && cd progcoder-shop && dotnet build src/Services/Catalog/Core/Catalog.Application/Catalog.Application.csproj
```

## Build the smallest thing that proves your point

Building the whole solution restores every package for every service. To check
that a change in a query handler compiles, build only its project:

```
progcoder-shop/src/Services/Catalog/Core/Catalog.Application/Catalog.Application.csproj
```

The target framework is `net8.0`, declared once in
`progcoder-shop/src/Directory.Build.props`. There is no
`Directory.Packages.props` — versions live in each `.csproj`.

## When a build is not worth it

A one-line null guard, a comment change, or a string edit does not need a
compile. Reading the diff is enough. Install the SDK only when the change could
plausibly fail to compile.
