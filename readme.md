# FileBasedApp.Toolkit

[![FileBasedApp.Toolkit](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.svg?style=flat-square&label=FileBasedApp.Toolkit)](https://www.nuget.org/packages/FileBasedApp.Toolkit)
[![TruePath.TestableIO.System.IO](https://img.shields.io/nuget/v/TruePath.TestableIO.System.IO.svg?style=flat-square&label=TruePath.TestableIO.System.IO)](https://www.nuget.org/packages/TruePath.TestableIO.System.IO)
[![FileBasedApp.Toolkit.Template](https://img.shields.io/nuget/v/FileBasedApp.Toolkit.Template.svg?style=flat-square&label=FileBasedApp.Toolkit.Template)](https://www.nuget.org/packages/FileBasedApp.Toolkit.Template)

A toolkit for building file-based applications in .NET.  (see  [more](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps))

## Packages

- [FileBasedApp.Toolkit](Source/FileBasedApp/FileBasedApp.Toolkit/README.md) - Core toolkit with helpers and extensions.
- [TruePath.TestableIO.System.IO](Source/TruePath.TestableIO/TruePath.TestableIO.System.IO/README.md) - Bridges TruePath and TestableIO.
- [FileBasedApp.Toolkit.Template](Source/FileBasedApp/Templates/README.md) - Item template for creating a file based app that uses the FileBasedApp.Toolkit.

It takes some inspiration based on experiences with brilliant tools like Cake and Nuke. But tries to give some of what those libraries provides but aimed at just using FileBasedApps.

It relies heavily (hence opinionated) on [TruePath](https://github.com/ForNeVeR/TruePath),[SimpleExec](https://github.com/adamralph/simple-exec) and [Spectre.Console and Spectre.Console.Cli](https://spectreconsole.net/). In this library I strive for especially TruePath types to be first class citizens.

A side effect of this project is the `TruePath.TestableIO.System.IO` library which provides a bridge between `TruePath` and [System.Io.Abstractions](https://github.com/TestableIO/System.IO.Abstractions) 



## Preview packages

Preview versions of packages are published to [GitHub Packages](https://github.com/jeppevammenkristensen/FileBasedApp.Toolkit/packages) before they are released to nuget.org. To use preview packages, add the GitHub NuGet source:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/jeppevammenkristensen/index.json" \
  --name "<NameForSource>" \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

Your GitHub username can be found at [github.com/settings/profile](https://github.com/settings/profile). To create a Personal Access Token (PAT) with the required `read:packages` scope, follow the [GitHub guide for creating a PAT](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic).

## Eating my own dogfood

This project uses FileBasedApp.Toolkit itself to power its own CI/CD pipeline. The [Scripts](Scripts/) folder contains `.cs` scripts that reference `FileBasedApp.Toolkit` as a `#:package` dependency and are executed directly with `dotnet run`:

- [build-and-run-test.cs](Scripts/build-and-run-test.cs) - Builds the solution and runs tests, used by the [Build and run Tests](.github/workflows/dotnet.yml) workflow on every push and pull request.
- [build-and-publish-nuget-packages.cs](Scripts/build-and-publish-nuget-packages.cs) - Packs and publishes NuGet packages to either GitHub Packages (staging) or nuget.org (production), used by the [Publish nuget packages](.github/workflows/publish.yml) workflow.

## Samples

- [wrapper.cs](Samples/wrapper.cs) - Generates an interface and a wrapper class for a given static or public class. It was used to generate `ISimpleExecCommandWrapper` and `SimpleExecCommand` from the SimpleExec library's static `Command` class.


